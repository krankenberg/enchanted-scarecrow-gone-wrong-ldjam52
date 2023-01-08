using System;
using Godot;

namespace ldjam52.Game.Field.Crops;

public partial class Crop : Node2D
{
    [Signal]
    public delegate void CropPickedUpEventHandler(Crop crop);

    public bool PickedUp { private set; get; }
    public bool FullyGrown => _growthStage == _cropResource.GrowthStageCount - 1;
    public float MaxBounceDistance => _maxBounceDistance;

    [Export]
    private CropResource _cropResource;

    [Export]
    private Timer _growthTimer;

    [Export]
    private Sprite2D _sprite;

    [Export]
    private Area2D _collisionArea;

    [Export]
    private float _gravity;

    [Export]
    private float _maxBounceDistance;

    [Export]
    private float _bounceVelocity;

    [Export]
    private float _bounceHeight;

    [Export]
    private Curve _bounceCurve;

    private int _growthStage;

    private bool _falling;

    private float _velocity;

    private Action _afterBounceCallback;

    private bool _bouncing;

    private Vector2 _bounceFrom;

    private Vector2 _bounceTo;

    private float _bounceProgress;

    public override void _Ready()
    {
        _sprite.Texture = _cropResource.SpriteTexture;
        _sprite.Hframes = _cropResource.GrowthStageCount;

        _growthTimer.Timeout += OnGrowthTimer;
    }

    public void Drop()
    {
        _velocity = 0;
        _falling = true;
        PickedUp = true;
        _collisionArea.Monitorable = false;
        _growthTimer.Stop();
        var cropDropEvent = new CropDropEvent();
        cropDropEvent.Crop = this;
        cropDropEvent.Emit();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_falling)
        {
            Fall(delta);
        }

        if (_bouncing)
        {
            Bounce(delta);
        }
    }

    private void Fall(double delta)
    {
        _velocity += (float)delta * _gravity;
        GlobalPosition += new Vector2(0, (float)delta * _velocity);
        if (GlobalPosition.y > 164)
        {
            Land();
        }
    }

    private void Bounce(double delta)
    {
        var bounceDistance = (float)delta * _bounceVelocity;
        _bounceProgress += bounceDistance;
        var bounceSegment = _bounceTo - _bounceFrom;
        var progress = _bounceProgress / bounceSegment.Length();
        var height = _bounceCurve.SampleBaked(progress) * _bounceHeight;

        var newPosition = bounceSegment * progress;
        newPosition += new Vector2(0, -height);

        GlobalPosition = _bounceFrom + newPosition;

        if (progress >= 1)
        {
            LandAfterBounce();
        }
    }

    private void Land()
    {
        _falling = false;
        var cropLandedEvent = new CropLandedEvent();
        cropLandedEvent.Crop = this;
        cropLandedEvent.Emit();
    }

    private void LandAfterBounce()
    {
        GlobalPosition = _bounceTo;
        _bouncing = false;
        PickedUp = false;
        _collisionArea.Monitorable = true;
        _growthTimer.Start();
        _afterBounceCallback.Invoke();
        _afterBounceCallback = null;
    }

    private void OnGrowthTimer()
    {
        _growthStage++;
        _sprite.Frame = _growthStage;

        if (FullyGrown)
        {
            _growthTimer.Stop();
        }
    }

    public void PickUp()
    {
        PickedUp = true;
        EmitSignal(SignalName.CropPickedUp, this);
        _collisionArea.Monitorable = false;
        _growthTimer.Stop();
    }

    public void Smash()
    {
        GD.Print($"{Name} smashed!");
        QueueFree();
    }

    public void BounceToSlot(Vector2 closestSlot, Action afterBounceCallback)
    {
        _bounceProgress = 0;
        _bounceFrom = GlobalPosition;
        _bounceTo = closestSlot;
        _afterBounceCallback = afterBounceCallback;
        if (_bounceTo.DistanceTo(_bounceFrom) < 3)
        {
            LandAfterBounce();
        }
        else
        {
            _bouncing = true;
        }
    }
}
