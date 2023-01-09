using System.Collections.Generic;
using Godot;
using ldjam52.Game.Field.Crops;
using ldjam52.Game.Tutorial;
using ldjam52.Game.UserInterface;
using ldjam52.Game.Utils;

namespace ldjam52.Game.Field;

public partial class CropManager : Node2D
{
    [Export]
    private PackedScene _cropScene;

    [Export]
    private int _startCropCount;

    private List<Vector2> _freeCropSlots;
    private Dictionary<Crop, Vector2> _occupiedCropSlots;

    private List<Crop> _crops;

    private bool _active;

    public override void _Ready()
    {
        RequestCropEvent.Listen(OnRequestCropEvent);
        RequestFullyGrownCropEvent.Listen(OnRequestFullyGrownCropEvent);
        CropDropEvent.Listen(OnCropDroppedEvent);
        CropLandedEvent.Listen(OnCropLandedEvent);
        FallingCropSpawnEvent.Listen(OnFallingCropSpawnEvent);
        BarrierTutorialDoneEvent.Listen(_ =>
        {
            _active = true;
            CallDeferred(MethodName.SpawnCrops);
        });
        SpawnTutorialCropEvent.Listen(OnSpawnTutorialCrop);
        _freeCropSlots = new List<Vector2>();
        _occupiedCropSlots = new Dictionary<Crop, Vector2>();
        for (var i = 0; i < GetChildCount(); i++)
        {
            _freeCropSlots.Add(GetChild<Node2D>(i).GlobalPosition);
        }

        _crops = new List<Crop>();
    }

    private void OnSpawnTutorialCrop(SpawnTutorialCropEvent spawnTutorialCropEvent)
    {
        var closestSlot = GetClosestSlot(new Vector2(160, 100), out _);
        
        var crop = _cropScene.Instantiate<Crop>();
        AddSibling(crop);

        var position = _freeCropSlots[closestSlot];
        _freeCropSlots.RemoveAt(closestSlot);
        _occupiedCropSlots[crop] = position;
        crop.GlobalPosition = position;
        
        crop.Connect(Crop.SignalName.CropPickedUp, new Callable(this, MethodName.OnCropPickedUp));
        _crops.Add(crop);
        crop.StartGrowing();
        
        spawnTutorialCropEvent.Callback.Invoke(crop);
    }

    private void OnFallingCropSpawnEvent(FallingCropSpawnEvent fallingCropSpawnEvent)
    {
        var crop = _cropScene.Instantiate<Crop>();
        AddSibling(crop);
        crop.GlobalPosition = fallingCropSpawnEvent.Position;
        crop.Drop();
    }

    private void OnRequestFullyGrownCropEvent(RequestFullyGrownCropEvent requestFullyGrownCropEvent)
    {
        if (requestFullyGrownCropEvent.Consumed)
        {
            return;
        }

        requestFullyGrownCropEvent.Consumed = true;
        for (var i = 0; i < _crops.Count; i++)
        {
            var crop = _crops[i];
            if (crop.FullyGrown && !crop.PickedUp)
            {
                requestFullyGrownCropEvent.Callback?.Invoke(crop);
                return;
            }
        }
    }

    private void SpawnCrops()
    {
        for (int i = 0; i < _startCropCount; i++)
        {
            if (_freeCropSlots.Count == 0)
            {
                GD.PrintErr("Couldn't find position for crop...");
                continue;
            }

            var crop = _cropScene.Instantiate<Crop>();
            AddSibling(crop);
            OccupyRandomCropSlot(crop);
            crop.Connect(Crop.SignalName.CropPickedUp, new Callable(this, MethodName.OnCropPickedUp));
            _crops.Add(crop);
            crop.StartGrowing();
        }
    }

    private void OccupyRandomCropSlot(Crop crop)
    {
        var randomIndex = Random.Generator.RandiRange(0, _freeCropSlots.Count - 1);
        var position = _freeCropSlots[randomIndex];
        _freeCropSlots.RemoveAt(randomIndex);
        _occupiedCropSlots[crop] = position;
        crop.GlobalPosition = position;
    }

    private void OnCropDroppedEvent(CropDropEvent cropDropEvent)
    {
        var crop = cropDropEvent.Crop;
        var cropPosition = crop.GlobalPosition;
        crop.GetParent().RemoveChild(crop);
        AddSibling(crop);
        crop.GlobalPosition = cropPosition;
    }

    private void OnCropLandedEvent(CropLandedEvent cropLandedEvent)
    {
        var crop = cropLandedEvent.Crop;
        var cropPosition = crop.GlobalPosition;
        var closestSlot = GetClosestSlot(cropPosition, out var lowestDistance);

        if (closestSlot == -1 || lowestDistance > crop.MaxBounceDistance)
        {
            crop.Smash();
            return;
        }

        var position = _freeCropSlots[closestSlot];
        _freeCropSlots.RemoveAt(closestSlot);
        _occupiedCropSlots[crop] = position;
        crop.Connect(Crop.SignalName.CropPickedUp, new Callable(this, MethodName.OnCropPickedUp));
        crop.BounceToSlot(position, () => _crops.Add(crop));
    }

    private int GetClosestSlot(Vector2 cropPosition, out float lowestDistance)
    {
        var closestSlot = -1;
        lowestDistance = float.MaxValue;
        for (var i = 0; i < _freeCropSlots.Count; i++)
        {
            var distance = cropPosition.DistanceTo(_freeCropSlots[i]);
            if (distance < lowestDistance)
            {
                closestSlot = i;
                lowestDistance = distance;
            }
        }

        return closestSlot;
    }

    private void OnCropPickedUp(Crop crop)
    {
        _crops.Remove(crop);
        _occupiedCropSlots.Remove(crop, out var freedSlot);
        _freeCropSlots.Add(freedSlot);

        if (_crops.Count == 0 && _active)
        {
            new GameOverEvent().Emit();
        }
    }

    private void OnRequestCropEvent(RequestCropEvent requestCropEvent)
    {
        if (requestCropEvent.Consumed)
        {
            return;
        }

        requestCropEvent.Consumed = true;
        var crop = ChooseRandomCrop();
        if (crop != null)
        {
            requestCropEvent.Callback?.Invoke(crop);
        }
        else
        {
            requestCropEvent.NoCropAvailableCallback?.Invoke();
        }
    }

    private Crop ChooseRandomCrop()
    {
        if (_crops.Count == 0)
        {
            return null;
        }

        for (int i = 0; i < 10; i++)
        {
            var randomIndex = Random.Generator.RandiRange(0, _crops.Count - 1);
            var crop = _crops[randomIndex];
            if (!crop.PickedUp)
            {
                return crop;
            }
        }

        return null;
    }
}
