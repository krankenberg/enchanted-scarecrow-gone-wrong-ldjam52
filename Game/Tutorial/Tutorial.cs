using System;
using Godot;
using ldjam52.Game.Crows;
using ldjam52.Game.Farmers;
using ldjam52.Game.Field;
using ldjam52.Game.Field.Crops;
using ldjam52.Game.Scarecrow.Spells;
using ldjam52.Game.UserInterface;
using ldjam52.Game.Utils;

namespace ldjam52.Game.Tutorial;

public partial class Tutorial : Node2D
{
    private static readonly Color BarrierColor = new("#2d3d72");
    private const int BarrierHintDistance = 20;
    private const int SoulHintDistance = 80;
    private const float CropCheckTime = 5;

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

        WaitingForFarmerGettingClose,
        WaitingForSoulPullingOut,
        WaitingForSoulCut,

        WaitingForSecondFarmerSpawning,

        WaitingForSecondFarmerGettingClose,
        WaitingForSecondSoulPullingOut,
        WaitingForSecondSoulCut,

        WaitingForCropSoulReady,
        WaitingForCropAwaken,

        Ended,
    }

    [Export]
    private float _crowDistanceToCropForHint;

    [Export]
    private float _farmerDistanceToCropForHint;

    [Export]
    private MouseCursor _mouseCursor;

    [Export(PropertyHint.Layers2dPhysics)]
    private uint _barrierCollisionMask;

    private Stage _stage;

    private bool _crowFendedOffBeforeTutorial;

    private Crop _crop;
    private Crow _crow;
    private Farmer _firstFarmer;
    private Farmer _secondFarmer;
    private bool _firstSoulCut;
    private bool _secondSoulCut;

    private float _timePassed;

    public override void _Ready()
    {
        FarmerSpawnedEvent.Listen(OnFarmerSpawnedEvent);

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
            case Stage.Ended:
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

            case Stage.WaitingForFarmerGettingClose:
                if (_firstSoulCut)
                {
                    _stage = Stage.WaitingForSoulCut;
                    break;
                }

                ShowSoulPullHintWhenFarmerIsCloseEnough(_firstFarmer, Stage.WaitingForSoulPullingOut);
                break;
            case Stage.WaitingForSoulPullingOut:
                if (_firstSoulCut)
                {
                    _stage = Stage.WaitingForSoulCut;
                    break;
                }

                ShowSoulCutHintWhenSoulPulledOut(_firstFarmer, Stage.WaitingForSoulCut);
                break;
            case Stage.WaitingForSoulCut:
                ContinueWhenSoulCut(_firstSoulCut, Stage.WaitingForSecondFarmerSpawning);
                break;

            case Stage.WaitingForSecondFarmerSpawning:
                if (_secondSoulCut)
                {
                    _stage = Stage.WaitingForSecondSoulCut;
                    break;
                }

                WaitForSecondFarmerSpawn();
                break;
            case Stage.WaitingForSecondFarmerGettingClose:
                if (_secondSoulCut)
                {
                    _stage = Stage.WaitingForSecondSoulCut;
                    break;
                }

                ShowSoulPullHintWhenFarmerIsCloseEnough(_secondFarmer, Stage.WaitingForSecondSoulPullingOut);
                break;
            case Stage.WaitingForSecondSoulPullingOut:
                if (_secondSoulCut)
                {
                    _stage = Stage.WaitingForSecondSoulCut;
                    break;
                }

                ShowSoulCutHintWhenSoulPulledOut(_secondFarmer, Stage.WaitingForSecondSoulCut);
                break;
            case Stage.WaitingForSecondSoulCut:
                ContinueWhenSoulCut(_secondSoulCut, Stage.WaitingForCropSoulReady);
                break;

            case Stage.WaitingForCropSoulReady:
                ShowAwakenCropHintWhenCropSoulReady(delta);
                break;
            case Stage.WaitingForCropAwaken:
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
                _crow.Connect(Crow.SignalName.CrowCollidedWithBarrier, new Callable(this, MethodName.CrowCollidedWithBarrier), (uint)ConnectFlags.OneShot);
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
                    _crow.Connect(Crow.SignalName.CrowCollidedWithBarrier, crowCollidedWithBarrierCallable, (uint)ConnectFlags.OneShot);
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

    private void OnFarmerSpawnedEvent(FarmerSpawnedEvent farmerSpawnedEvent)
    {
        if (_firstFarmer == null)
        {
            _firstFarmer = farmerSpawnedEvent.Farmer;
            _firstFarmer.Connect(Farmer.SignalName.SoulWasCut, new Callable(this, MethodName.FirstSoulCut), (uint)ConnectFlags.OneShot);
            _stage = Stage.WaitingForFarmerGettingClose;
        }
        else if (_secondFarmer == null)
        {
            _secondFarmer = farmerSpawnedEvent.Farmer;
            _secondFarmer.Connect(Farmer.SignalName.SoulWasCut, new Callable(this, MethodName.SecondSoulCut), (uint)ConnectFlags.OneShot);
        }
    }

    private void FirstSoulCut()
    {
        _firstSoulCut = true;
    }

    private void SecondSoulCut()
    {
        _secondSoulCut = true;
    }

    private void WaitForSecondFarmerSpawn()
    {
        if (_secondFarmer != null)
        {
            _stage = Stage.WaitingForSecondFarmerGettingClose;
        }
    }

    private void ShowSoulPullHintWhenFarmerIsCloseEnough(Farmer farmer, Stage nextStage)
    {
        var farmerPosition = farmer.GlobalPosition;
        if (farmerPosition.x is > 64 and < 256 || farmer.DistanceToTarget < _farmerDistanceToCropForHint)
        {
            var hintStart = farmerPosition + new Vector2(0, -6);
            var hintDirection = (new Vector2(160, 0) - hintStart).Normalized();
            var hintEnd = hintStart + hintDirection * SoulHintDistance;

            _mouseCursor.LoopLeftClick(hintStart, hintEnd, Colors.White);

            SetTutorialPause(true);
            _stage = nextStage;
        }
    }

    private void ShowSoulCutHintWhenSoulPulledOut(Farmer farmer, Stage nextStage)
    {
        if (farmer.SoulOutDistance() > SoulHintDistance - 20)
        {
            _mouseCursor.StopLoop();
            var cutLine = farmer.GetCutLine();
            var cutPosition = cutLine[0];
            var cutDirection = cutLine[1];

            var hintHalfLength = 20;
            var positionA = cutPosition - cutDirection * hintHalfLength;
            var positionB = cutPosition + cutDirection * hintHalfLength;
            var aHigherThanB = positionA.y < positionB.y;
            _mouseCursor.LoopLeftClick(aHigherThanB ? positionA : positionB, aHigherThanB ? positionB : positionA, Scarecrow.Scarecrow.CutColor);
            _stage = nextStage;
        }
    }

    private void ContinueWhenSoulCut(bool isSoulCut, Stage nextStage)
    {
        if (isSoulCut)
        {
            _mouseCursor.StopLoop();
            SetTutorialPause(false);
            _stage = nextStage;
        }
    }

    private void ShowAwakenCropHintWhenCropSoulReady(double delta)
    {
        _timePassed += (float)delta;
        if (_timePassed > CropCheckTime)
        {
            _timePassed = 0;
            var requestSoulCountEvent = new RequestSoulCountEvent();
            requestSoulCountEvent.Callback = soulCount =>
            {
                var requestFullyGrownCropEvent = new RequestFullyGrownCropEvent();
                requestFullyGrownCropEvent.Callback = crop =>
                {
                    if (soulCount >= crop.SoulsNeeded)
                    {
                        ShowAwakenCropHint(crop);
                    }
                };
                requestFullyGrownCropEvent.Emit();
            };
            requestSoulCountEvent.Emit();
        }
    }

    private void ShowAwakenCropHint(Crop crop)
    {
        SetTutorialPause(true);
        UseSoulsEvent.Listen(OnUseSoulsEvent);
        _mouseCursor.LoopLeftClickNoLine(crop.GlobalPosition + new Vector2(0, -20), crop.GlobalPosition + new Vector2(0, -3));
        _stage = Stage.WaitingForCropAwaken;
    }

    private void OnUseSoulsEvent(UseSoulsEvent obj)
    {
        if (_stage == Stage.Ended)
        {
            return;
        }

        SetTutorialPause(false);
        _mouseCursor.StopLoop();
        _stage = Stage.Ended;
    }

    private void SetTutorialPause(bool pause)
    {
        GetTree().Paused = pause;
        PhysicsServer2D.SetActive(true);
    }
}
