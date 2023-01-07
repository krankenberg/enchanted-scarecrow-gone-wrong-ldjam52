using Godot;
using ldjam52.Game.Utils;

namespace ldjam52.Game.Field;

public partial class CropManager : Node2D
{
    public override void _Ready()
    {
        RequestCropEvent.Listen(OnRequestCropEvent);
    }

    private void OnRequestCropEvent(RequestCropEvent requestCropEvent)
    {
        if (requestCropEvent.Consumed)
        {
            return;
        }

        requestCropEvent.Consumed = true;
        requestCropEvent.Callback.Invoke(ChooseRandomCropPosition());
    }

    private Vector2 ChooseRandomCropPosition()
    {
        var cropPositions = GetChildren();
        var randomIndex = Random.Generator.RandiRange(0, cropPositions.Count - 1);
        return ((Node2D)cropPositions[randomIndex]).GlobalPosition;
    }
}
