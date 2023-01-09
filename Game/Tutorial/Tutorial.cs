using System;
using Godot;
using ldjam52.Game.Crows;
using ldjam52.Game.Field.Crops;
using ldjam52.Game.Utils;

namespace ldjam52.Game.Tutorial;

public partial class Tutorial : Node2D
{
    private static readonly Color BarrierColor = new("#2d3d72");
    private const int BarrierHintDistance = 20;

    private enum Stage
    {
        WaitingForCropSpawn,
        WaitingForCrowSpawn,
        WaitingForCrowDistance,
        WaitingForBarrier,
        
        WaitingForSecondCrowSpawn,
        WaitingForSecondCrowDistance,
        WaitingForSecondBarrier,
        
        EndBarrierTutorial,
        BarrierTutorialEnded,
    }

    [Export]
    private float _crowDistanceToCropForHint;

    [Export]
    private MouseCursor _mouseCursor;

    [Export(PropertyHint.Layers2dPhysics)]
    private uint _barrierCollisionMask;

    private Stage _stage;

    private bool _crowFendedOffBeforeTutorial;
    
    private Crop _crop;
    private Crow _crow;

    public override void _Ready()
    {
        _stage = Stage.WaitingForCropSpawn;
        SpawnTutorialCropEvent.Emit(crop =>
        {
            _crop = crop;
            _stage = Stage.WaitingForCrowSpawn;
        });
    }

    public override void _Process(double delta)
    {
        switch (_stage)
        {
            case Stage.BarrierTutorialEnded:
                break;
            case Stage.WaitingForCropSpawn:
                break;
            case Stage.WaitingForCrowSpawn:
                SpawnCrowWhenCropIsReady();
                break;
            case Stage.WaitingForCrowDistance:
                if (_crowFendedOffBeforeTutorial)
                {
                    _stage = Stage.WaitingForBarrier;
                    break;
                }
                ShowBarrierTutorialWhenCrowIsCloseEnough(Stage.WaitingForBarrier);
                break;
            case Stage.WaitingForBarrier:
                UnpauseWhenBarrierDrawn(Stage.WaitingForSecondCrowSpawn, true);
                break;
            case Stage.WaitingForSecondCrowSpawn:
                break;
            case Stage.WaitingForSecondCrowDistance:
                if (_crowFendedOffBeforeTutorial)
                {
                    _stage = Stage.WaitingForSecondBarrier;
                    break;
                }
                ShowBarrierTutorialWhenCrowIsCloseEnough(Stage.WaitingForSecondBarrier);
                break;
            case Stage.WaitingForSecondBarrier:
                UnpauseWhenBarrierDrawn(Stage.EndBarrierTutorial, false);
                break;
            case Stage.EndBarrierTutorial:
                EndBarrierTutorial();
                break;
        }
    }

    private void SpawnCrowWhenCropIsReady()
    {
        if (_crow == null && _crop.HalfGrown)
        {
            SpawnTutorialCrowEvent.Emit(crow =>
            {
                _crowFendedOffBeforeTutorial = false;
                _crow = crow;
                _crow.Connect(Crow.SignalName.CrowCollidedWithBarrier, new Callable(this, MethodName.CrowCollidedWithBarrier), (uint) ConnectFlags.OneShot);
                _stage = Stage.WaitingForCrowDistance;
            });
        }
    }

    private void CrowCollidedWithBarrier()
    {
        _crowFendedOffBeforeTutorial = true;
    }

    private void ShowBarrierTutorialWhenCrowIsCloseEnough(Stage nextStage)
    {
        DoWhenCrowIsCloseEnough(crowMovementVector =>
        {
            var hintPosition = _crow.GlobalPosition + crowMovementVector.LimitLength(BarrierHintDistance);
            var hintDirection = crowMovementVector.Orthogonal().Normalized();
            var hintHalfLength = 20;
            var positionA = hintPosition - hintDirection * hintHalfLength;
            var positionB = hintPosition + hintDirection * hintHalfLength;
            var aHigherThanB = positionA.y < positionB.y;
            _mouseCursor.LoopRightClick(aHigherThanB ? positionA : positionB, aHigherThanB ? positionB : positionA, BarrierColor);
            _stage = nextStage;
        });
    }

    private void DoWhenCrowIsCloseEnough(Action<Vector2> doWithCrowMovementVector)
    {
        var crowMovementVector = _crop.GlobalPosition - _crow.GlobalPosition;
        var crowDistanceToCrop = crowMovementVector.Length();
        if (crowDistanceToCrop < _crowDistanceToCropForHint)
        {
            _crow.Pause();
            SetTutorialPause(true);
            doWithCrowMovementVector.Invoke(crowMovementVector);
        }
    }

    private void UnpauseWhenBarrierDrawn(Stage nextStage, bool spawnAnotherCrow)
    {
        if (_crowFendedOffBeforeTutorial || CrowWillFlyIntoBarrier())
        {
            _stage = nextStage;
            _mouseCursor.StopLoop();
            _crow.Unpause();
            SetTutorialPause(false);
            var crowCollidedWithBarrierCallable = new Callable(this, MethodName.CrowCollidedWithBarrier);
            if (_crow.IsConnected(Crow.SignalName.CrowCollidedWithBarrier, crowCollidedWithBarrierCallable))
            {
                _crow.Disconnect(Crow.SignalName.CrowCollidedWithBarrier, crowCollidedWithBarrierCallable);
            }

            if (spawnAnotherCrow)
            {
                SpawnTutorialCrowEvent.Emit(crow =>
                {
                    _crowFendedOffBeforeTutorial = false;
                    _crow = crow;
                    _crow.Connect(Crow.SignalName.CrowCollidedWithBarrier, crowCollidedWithBarrierCallable, (uint) ConnectFlags.OneShot);
                    _stage = Stage.WaitingForSecondCrowDistance;
                });
            }
        }
    }

    private bool CrowWillFlyIntoBarrier()
    {
        var crowDirection = (_crop.GlobalPosition - _crow.GlobalPosition).Normalized();
        var parameters = new PhysicsRayQueryParameters2D();
        parameters.From = _crow.GlobalPosition;
        parameters.To = _crow.GlobalPosition + crowDirection * (BarrierHintDistance + 20);
        parameters.CollideWithAreas = true;
        parameters.CollisionMask = _barrierCollisionMask;
        var raycastHit = GetWorld2d().DirectSpaceState.IntersectRayEnhanced(parameters);
        return raycastHit.HasValue;
    }

    private void EndBarrierTutorial()
    {
        _stage = Stage.BarrierTutorialEnded;
        BarrierTutorialDoneEvent.Emit();
    }

    private void SetTutorialPause(bool pause)
    {
        GetTree().Paused = pause;
        PhysicsServer2D.SetActive(true);
    }
    
}
