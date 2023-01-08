using Godot;
using ldjam52.Game.Scarecrow.Spells;

namespace ldjam52.Game.Field.Crops;

public partial class LivingCropSpawner : Node2D
{
    [Export]
    private PackedScene _livingCropScene;
    
    public override void _Ready()
    {
        SpawnLivingCropEvent.Listen(OnSpawnLivingCropEvent);
    }

    private void OnSpawnLivingCropEvent(SpawnLivingCropEvent spawnLivingCropEvent)
    {
        var livingCrop = _livingCropScene.Instantiate<Node2D>();
        AddSibling(livingCrop);
        livingCrop.GlobalPosition = spawnLivingCropEvent.Position;
    }
}
