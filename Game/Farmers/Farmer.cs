using Godot;
using ldjam52.Game.Events;
using ldjam52.Game.Field.Crops;
using ldjam52.Game.Scarecrow.Spells;
using ldjam52.Game.Utils;

namespace ldjam52.Game.Farmers;

public partial class Farmer : Node2D
{
    [Export]
    private float _speedMin;

    [Export]
    private float _speedMax;

    [Export]
    private Sprite2D _sprite;

    [Export(PropertyHint.Layers2dPhysics)]
    private uint _cropLayerMask;

    [Export]
    private Shape2D _cropDetectorShape;

    [Export]
    private Area2D _collisionArea;

    [Export]
    private GPUParticles2D _soulParticles;

    [Export]
    private Line2D _pulledSoulLine;

    [Export]
    private float _pulledSoulMaxLength;

    [Export]
    private float _soulPullBackSpeed;

    [Export]
    private float _pulledSoulMinLength;

    private float _speed;
    private Crop _target;

    private Vector2 _startPosition;
    private bool _goingBack;
    private Vector2 _targetPosition;
    private bool _soulOut;
    private bool _pullingSoulBack;
    private Vector2 _soulVector;
    private IEventHandler _soulCutEventHandler;

    public override void _Ready()
    {
        _soulCutEventHandler = SoulCutEvent.Listen(OnSoulCutEvent);
        _pulledSoulLine.Visible = false;
        _soulParticles.Emitting = false;
        _speed = Random.Generator.RandfRange(_speedMin, _speedMax);
        _collisionArea.MouseEntered += OnMouseEntered;
        _collisionArea.MouseExited += OnMouseExited;
    }

    public override void _ExitTree()
    {
        EventBus.Unregister(ref _soulCutEventHandler);
    }

    private void OnSoulCutEvent(SoulCutEvent soulCutEvent)
    {
        if (_soulOut)
        {
            var intersection = Geometry2D.SegmentIntersectsSegment(
                _pulledSoulLine.GlobalPosition, _pulledSoulLine.GlobalPosition + _soulVector,
                soulCutEvent.Start, soulCutEvent.End
            );
            if (intersection.Obj is Vector2 intersectionPoint)
            {
                var soulHarvestedEvent = new SoulHarvestedEvent();
                soulHarvestedEvent.Emit();
                QueueFree();
            }
        }
    }

    private void OnMouseEntered()
    {
        _soulParticles.Emitting = true;
    }

    private void OnMouseExited()
    {
        _soulParticles.Emitting = false;
    }

    public void Start()
    {
        _startPosition = GlobalPosition;
        FindNextCrop();
    }

    private void FindNextCrop()
    {
        var firstDirection = Random.Generator.Randf() > 0.5F ? Vector2.Right : Vector2.Left;
        var secondDirection = firstDirection == Vector2.Right ? Vector2.Left : Vector2.Right;

        _target = LookForCrop(firstDirection) ?? LookForCrop(secondDirection);
        if (_target != null)
        {
            _target.Connect(Crop.SignalName.CropPickedUp, new Callable(this, MethodName.OnTargetPickedUp), (uint)ConnectFlags.OneShot);
            _targetPosition = _target.GlobalPosition;
        }
        else
        {
            GoBack();
        }
    }

    private void OnTargetPickedUp(Crop crop)
    {
        FindNextCrop();
    }

    private Crop LookForCrop(Vector2 direction)
    {
        var parameters = new PhysicsShapeQueryParameters2D();
        parameters.CollisionMask = _cropLayerMask;
        parameters.Shape = _cropDetectorShape;
        parameters.CollideWithAreas = true;
        parameters.Transform = new Transform2D(0, GlobalPosition);
        parameters.Motion = direction * 1000;
        var hits = GetWorld2d().DirectSpaceState.IntersectShapeEnhanced(parameters);

        Crop closestCrop = null;
        var lowestDistance = float.MaxValue;

        for (var i = 0; i < hits.Length; i++)
        {
            var shapecastHit = hits[i];
            var collidedArea = shapecastHit.Collider.As<Area2D>();
            var crop = (Crop)collidedArea.Owner;
            if (!crop.PickedUp)
            {
                var distanceToCrop = (crop.GlobalPosition - GlobalPosition).Length();
                if (distanceToCrop < lowestDistance)
                {
                    lowestDistance = distanceToCrop;
                    closestCrop = crop;
                }
            }
        }

        return closestCrop;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_soulOut)
        {
            if (_pullingSoulBack)
            {
                PullSoulBack(delta);
            }
            return;
        }
        
        if (_target == null && !_goingBack)
        {
            return;
        }

        var xMotion = (float)delta * _speed;
        var xDifferenceToTarget = _targetPosition.x - GlobalPosition.x;
        _sprite.FlipH = xDifferenceToTarget < 0;
        if (Mathf.Abs(xDifferenceToTarget) > xMotion)
        {
            GlobalPosition += new Vector2(Mathf.Sign(xDifferenceToTarget) * xMotion, 0);
        }
        else
        {
            if (_goingBack)
            {
                QueueFree();
                return;
            }

            if (_target != null)
            {
                _target.Disconnect(Crop.SignalName.CropPickedUp, new Callable(this, MethodName.OnTargetPickedUp));
                _target.PickUp();
                _target.GetParent().RemoveChild(_target);
            }

            GoBack();
        }
    }

    private void GoBack()
    {
        _goingBack = true;
        _targetPosition = _startPosition;
    }

    public void StartPullingSoul(Vector2 mousePosition)
    {
        _soulOut = true;
        _pullingSoulBack = false;
        _pulledSoulLine.Visible = true;
        PullSoulTo(mousePosition);
    }

    public void ContinuePullingSoul(Vector2 mousePosition)
    {
        PullSoulTo(mousePosition);
    }

    private void PullSoulTo(Vector2 mousePosition)
    {
        _soulVector = mousePosition - _pulledSoulLine.GlobalPosition;
        _soulVector = _soulVector.LimitLength(_pulledSoulMaxLength);
        UpdatePulledSoul();
    }

    private void UpdatePulledSoul()
    {
        _pulledSoulLine.Points = new[]
        {
            Vector2.Zero,
            _soulVector
        };
    }

    public void StopPullingSoul()
    {
        _pullingSoulBack = true;
    }

    private void PullSoulBack(double delta)
    {
        var soulDistance = (float)delta * _soulPullBackSpeed;
        var length = _soulVector.Length();
        if (length < _pulledSoulMinLength)
        {
            _soulOut = false;
            _pullingSoulBack = false;
            _pulledSoulLine.Visible = false;
            return;
        }
        _soulVector = _soulVector.LimitLength(length - soulDistance);
        UpdatePulledSoul();
    }
}
