using Godot;

namespace ldjam52.Game.Field.Crops;

public partial class CropResource : Resource
{
    [Export]
    public Texture2D SpriteTexture;

    [Export]
    public int GrowthStageCount;

}
