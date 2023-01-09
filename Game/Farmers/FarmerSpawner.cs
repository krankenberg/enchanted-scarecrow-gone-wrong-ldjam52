using Godot;
using ldjam52.Game.Field;
using ldjam52.Game.Tutorial;

namespace ldjam52.Game.Farmers;

public partial class FarmerSpawner : Node2D
{
    [Export]
    private PackedScene _farmerScene;

    [Export]
    private Curve _spawnTimeCurve;

    [Export]
    private Curve _spawnTimeVarianceCurve;

    private Timer _spawnTimer;

    private bool _active;

    private float _timeSpawning;

    public override void _Ready()
    {
        _spawnTimer = new Timer();
        AddChild(_spawnTimer);

        _spawnTimer.Timeout += SpawnFarmer;
        _spawnTimer.OneShot = true;
        RestartSpawnTimer();

        BarrierTutorialDoneEvent.Listen(_ =>
        {
            _active = true;
            RestartSpawnTimer();
        });
    }

    public override void _Process(double delta)
    {
        if (_active)
        {
            _timeSpawning += (float)delta;
        }
    }

    private void RestartSpawnTimer()
    {
        if (_active)
        {
            _spawnTimer.Start(BalancingConstants.Sample(_timeSpawning, _spawnTimeCurve, _spawnTimeVarianceCurve));
        }
    }

    private void SpawnFarmer()
    {
        var requestFullyGrownCropEvent = new RequestFullyGrownCropEvent();
        requestFullyGrownCropEvent.Callback = crop =>
        {
            if (crop.PickedUp)
            {
                return;
            }

            var farmer = _farmerScene.Instantiate<Farmer>();
            AddSibling(farmer);
            farmer.GlobalPosition = GlobalPosition;
            farmer.Start();
            FarmerSpawnedEvent.Emit(farmer);
        };
        requestFullyGrownCropEvent.Emit();
        RestartSpawnTimer();
    }
}
