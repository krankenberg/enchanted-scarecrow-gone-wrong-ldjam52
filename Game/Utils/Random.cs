using Godot;

namespace ldjam52.Game.Utils;

public static class Random
{
    public static readonly RandomNumberGenerator Generator = new();

    public static float Pitch(float range = 0.025F)
    {
        return Generator.RandfRange(1 - range, 1 + range);
    }
    
    public static float Volume(float range = 0.025F)
    {
        return Generator.RandfRange(1 - range, 1 + range);
    }
}
