using Godot;
using ldjam52.Game.Cutscene;
using ldjam52.Game.Farmers;
using ldjam52.Game.Field.Crops;
using ldjam52.Game.Input;
using ldjam52.Game.Scarecrow.Spells;
using ldjam52.Game.Tutorial;
using ldjam52.Game.UserInterface;
using ldjam52.Game.Utils;

namespace ldjam52.Game.Scarecrow;

public partial class Scarecrow : Node2D
{
    public static readonly Color CutColor = new("#b63c35");

    [Export]
    private PackedScene _barrierScene;

    [Export]
    private Node2D _minimumBarrierHeightMarker;

    [Export] 
    private GPUParticles2D _barrierCursorParticles;

    [Export]
    private GPUParticles2D _sparkles;

    [Export(PropertyHint.Layers2dPhysics)]
    private uint _farmerCollisionMask;

    [Export(PropertyHint.Layers2dPhysics)]
    private uint _soulReadyCropCollisionMask;

    [Export]
    private int _maxBarrierCount = 3;

    private Barrier _currentBarrier;
    private Farmer _currentFarmer;
    private SoulCutEvent _soulCutEvent;

    private bool _gameOver;
    private float _maxBarrierY;

    private int _barrierCount;

    private bool _barrierTutorialDone;

    private bool _cutsceneDone;

    public override void _Ready()
    {
        BarrierTutorialDoneEvent.Listen(OnBarrierTutorialDoneEvent);
        _maxBarrierY = _minimumBarrierHeightMarker.GlobalPosition.y;
        _barrierCursorParticles.Emitting = false;
        GameOverEvent.Listen(_ => _gameOver = true);
        CutsceneEndedEvent.Listen(_ =>
        {
            _cutsceneDone = true;
            _sparkles.Emitting = true;
        });
    }

    private void OnBarrierTutorialDoneEvent(BarrierTutorialDoneEvent _)
    {
        _barrierTutorialDone = true;
    }

    public override void _Process(double delta)
    {
        if (!_cutsceneDone)
        {
            return;
        }
        
        var mousePosition = GetGlobalMousePosition();
        _barrierCursorParticles.GlobalPosition = mousePosition;
        if (_currentBarrier != null)
        {
            _barrierCursorParticles.Emitting = false;

            if (_gameOver)
            {
                HandleShieldEnd();
                return;
            }

            _currentBarrier.EndPosition(mousePosition);
        }
        else
        {
            _barrierCursorParticles.Emitting = mousePosition.y < _maxBarrierY && CanCastBarrier();
            if (_currentFarmer != null)
            {
                _currentFarmer.ContinuePullingSoul(mousePosition);
            }

            if (_soulCutEvent != null)
            {
                _soulCutEvent.End = mousePosition;
                QueueRedraw();
            }
        }
    }

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (_gameOver || !_cutsceneDone)
        {
            return;
        }

        if (_currentBarrier == null)
        {
            if (inputEvent.IsActionPressed(InputConstants.Interact))
            {
                HandleInteractStart();
            }

            if (inputEvent.IsActionReleased(InputConstants.Interact))
            {
                HandleInteractEnd();
            }
        }

        if (_currentFarmer == null && _soulCutEvent == null)
        {
            if (inputEvent.IsActionPressed(InputConstants.Shield))
            {
                HandleShieldStart();
            }

            if (inputEvent.IsActionReleased(InputConstants.Shield))
            {
                HandleShieldEnd();
            }
        }
    }

    private void HandleShieldStart()
    {
        if (_currentBarrier == null && CanCastBarrier())
        {
            var mousePosition = GetGlobalMousePosition();
            if (mousePosition.y > _maxBarrierY)
            {
                return;
            }

            _currentBarrier = _barrierScene.Instantiate<Barrier>();
            GetParent().AddChild(_currentBarrier);
            _currentBarrier.Connect(Node.SignalName.TreeExited, new Callable(this, MethodName.OnBarrierDestroyed), (uint)ConnectFlags.OneShot);
            _barrierCount++;

            _currentBarrier.Begin(_maxBarrierY, mousePosition);
        }
    }

    private bool CanCastBarrier()
    {
        return !_barrierTutorialDone || _barrierCount < _maxBarrierCount;
    }

    private void OnBarrierDestroyed()
    {
        _barrierCount--;
    }

    private void HandleShieldEnd()
    {
        if (_currentBarrier == null)
        {
            return;
        }

        _currentBarrier.Cast();
        _currentBarrier = null;
    }

    private void HandleInteractStart()
    {
        var mousePosition = GetGlobalMousePosition();
        var farmerUnderMouse = GetFarmerUnderMouse(mousePosition);
        if (farmerUnderMouse != null)
        {
            _currentFarmer = farmerUnderMouse;
            _currentFarmer.StartPullingSoul(mousePosition);
        }
        else
        {
            var soulReadyCropUnderMouse = GetSoulReadyCropUnderMouse(mousePosition);
            if (soulReadyCropUnderMouse != null)
            {
                soulReadyCropUnderMouse.Awaken();
            }
            else
            {
                _soulCutEvent = new SoulCutEvent();
                _soulCutEvent.Start = mousePosition;
                QueueRedraw();
            }
        }
    }

    private void HandleInteractEnd()
    {
        if (_currentFarmer != null)
        {
            _currentFarmer.StopPullingSoul();
            _currentFarmer = null;
        }

        if (_soulCutEvent != null)
        {
            _soulCutEvent.End = GetGlobalMousePosition();
            _soulCutEvent.Emit();
            _soulCutEvent = null;
            QueueRedraw();
        }
    }

    private Farmer GetFarmerUnderMouse(Vector2 mousePosition)
    {
        return GetStuffUnderMouse<Farmer>(mousePosition, _farmerCollisionMask);
    }

    private Crop GetSoulReadyCropUnderMouse(Vector2 mousePosition)
    {
        return GetStuffUnderMouse<Crop>(mousePosition, _soulReadyCropCollisionMask);
    }

    private TStuff GetStuffUnderMouse<TStuff>(Vector2 mousePosition, uint collisionMask) where TStuff : Node
    {
        var parameters = new PhysicsPointQueryParameters2D();
        parameters.CollideWithAreas = true;
        parameters.Position = mousePosition;
        parameters.CollisionMask = collisionMask;
        var shapecastHits = GetWorld2d().DirectSpaceState.IntersectPointEnhanced(parameters, 1);
        if (shapecastHits.Length == 1)
        {
            return (TStuff)shapecastHits[0].Collider.As<Area2D>().Owner;
        }

        return null;
    }

    public override void _Draw()
    {
        if (_soulCutEvent != null)
        {
            DrawDashedLine(ToLocal(_soulCutEvent.Start), ToLocal(_soulCutEvent.End), CutColor);
        }
    }
}
