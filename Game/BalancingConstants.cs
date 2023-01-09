using Godot;
using ldjam52.Game.Utils;

namespace ldjam52.Game;

public static class BalancingConstants
{
    public const float TimeHorizon = 60 * 3;

    public static float Sample(float timeSpawning, Curve spawnTimeCurve, Curve spawnTimeVarianceCurve)
    {
        var baseTime = spawnTimeCurve.SampleBaked(timeSpawning / TimeHorizon);
        var variance = spawnTimeVarianceCurve.SampleBaked(timeSpawning / TimeHorizon);
        return Random.Generator.RandfRange(baseTime - variance, baseTime + variance);
    }
}
