using System.Collections.Generic;
using Godot;
using ldjam52.Game.Field.Crops;
using ldjam52.Game.UserInterface;
using ldjam52.Game.Utils;

namespace ldjam52.Game.Field;

public partial class CropManager : Node2D
{
    [Export]
    private PackedScene _cropScene;

    private List<Vector2> _freeCropSlots;
    private Dictionary<Crop, Vector2> _occupiedCropSlots;

    private List<Crop> _crops;

    public override void _Ready()
    {
        RequestCropEvent.Listen(OnRequestCropEvent);
        RequestFullyGrownCropEvent.Listen(OnRequestFullyGrownCropEvent);
        _freeCropSlots = new List<Vector2>();
        _occupiedCropSlots = new Dictionary<Crop, Vector2>();
        for (var i = 0; i < GetChildCount(); i++)
        {
            _freeCropSlots.Add(GetChild<Node2D>(i).GlobalPosition);
        }
        _crops = new List<Crop>();
        CallDeferred(MethodName.SpawnCrops);
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
        for (int i = 0; i < 20; i++)
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

    private void OnCropPickedUp(Crop crop)
    {
        _crops.Remove(crop);
        _occupiedCropSlots.Remove(crop, out var freedSlot);
        _freeCropSlots.Add(freedSlot);

        if (_crops.Count == 0)
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
