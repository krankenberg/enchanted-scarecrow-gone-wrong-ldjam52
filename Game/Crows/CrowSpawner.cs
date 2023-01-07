using Godot;
using ldjam52.Game.Field;
using ldjam52.Game.Utils;

namespace ldjam52.Game.Crows;

public partial class CrowSpawner : Node2D
{
    [Export]
    private PackedScene _crowScene;

    [Export]
    private float _spawnTimeMin;

    [Export]
    private float _spawnTimeMax;

    [Export]
    private float _spawnXOffsetMin;

    [Export]
    private float _spawnXOffsetMax;

    private Timer _spawnTimer;

    public override void _Ready()
    {
        _spawnTimer = new Timer();
        AddChild(_spawnTimer);

        _spawnTimer.Timeout += SpawnCrow;
        _spawnTimer.OneShot = true;
        RestartSpawnTimer();
    }

    private void RestartSpawnTimer()
    {
        _spawnTimer.Start(Random.Generator.RandfRange(_spawnTimeMin, _spawnTimeMax));
    }

    private void SpawnCrow()
    {
        var requestCropEvent = new RequestCropEvent();
        requestCropEvent.Callback = crop =>
        {
            var crow = _crowScene.Instantiate<Crow>();
            var spawnPosition = ChooseRandomSpawnPosition(crop.GlobalPosition);

            GetParent().AddChild(crow);
            crow.GlobalPosition = spawnPosition;
            crow.GrabCrop(spawnPosition, crop);
        };
        requestCropEvent.Emit();

        RestartSpawnTimer();
    }

    private Vector2 ChooseRandomSpawnPosition(Vector2 target)
    {
        var direction = Random.Generator.RandiRange(0, 1) == 0 ? 1 : -1;
        var offset = Random.Generator.RandfRange(_spawnXOffsetMin, _spawnXOffsetMax);
        return new Vector2(target.x + offset * direction, GlobalPosition.y);
    }
}
