using Godot;
using ldjam52.Game.Field;
using ldjam52.Game.Tutorial;
using ldjam52.Game.Utils;

namespace ldjam52.Game.Farmers;

public partial class FarmerSpawner : Node2D
{
    [Export]
    private PackedScene _farmerScene;

    [Export]
    private float _spawnTimeMin;

    [Export]
    private float _spawnTimeMax;
    
    private Timer _spawnTimer;

    private bool _active;

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

    private void RestartSpawnTimer()
    {
        if (_active)
        {
            _spawnTimer.Start(Random.Generator.RandfRange(_spawnTimeMin, _spawnTimeMax));
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
