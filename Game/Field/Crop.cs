using Godot;
using ldjam52.Game.Field.Crops;

namespace ldjam52.Game.Field;

public partial class Crop : Node2D
{
    [Signal]
    public delegate void CropPickedUpEventHandler(Crop crop);

    public bool PickedUp => !Visible;
    
    [Export]
    private CropResource _cropResource;

    [Export]
    private Timer _growthTimer;

    [Export]
    private Sprite2D _sprite;

    private int _growthStage;

    public override void _Ready()
    {
        _sprite.Texture = _cropResource.SpriteTexture;
        _sprite.Hframes = _cropResource.GrowthStageCount;

        _growthTimer.Timeout += OnGrowthTimer;
    }

    private void OnGrowthTimer()
    {
        _growthStage++;
        _sprite.Frame = _growthStage;
        
        if (_growthStage == _cropResource.GrowthStageCount - 1)
        {
            _growthTimer.Stop();
        }
    }

    public void PickUp()
    {
        EmitSignal(SignalName.CropPickedUp, this);
        Visible = false;
    }
}
