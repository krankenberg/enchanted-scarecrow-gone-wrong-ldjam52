using System;
using Godot;
using ldjam52.Game.Events;
using ldjam52.Game.Scarecrow.Spells;
using ldjam52.Game.UserInterface;
using Random = ldjam52.Game.Utils.Random;

namespace ldjam52.Game.Field.Crops;

public partial class Crop : Node2D
{
    [Signal]
    public delegate void CropPickedUpEventHandler(Crop crop);

    public bool PickedUp { private set; get; }
    public bool FullyGrown => _growthStage == _cropResource.GrowthStageCount - 1;
    public bool HalfGrown => _growthStage >= 2;
    public float MaxBounceDistance => _maxBounceDistance;
    public int SoulsNeeded => _soulsNeeded;

    [Export]
    private CropResource _cropResource;

    [Export]
    private Timer _startGrowthTimer;

    [Export]
    private Timer _growthTimer;

    [Export]
    private Sprite2D _sprite;

    [Export]
    private CollisionShape2D _collisionAreaShape;

    [Export]
    private CollisionShape2D _soulReadyCollisionShape;

    [Export]
    private float _gravity;

    [Export]
    private float _maxBounceDistance;

    [Export]
    private float _bounceVelocity;

    [Export]
    private float _bounceHeight;

    [Export]
    private Curve _bounceCurve;

    [Export]
    private AnimatedSprite2D _soulReadySprite;

    [Export]
    private GPUParticles2D _soulParticles;

    [Export]
    private int _soulsNeeded;

    [Export]
    private AudioStreamPlayer _bounceSound;

    [Export]
    private AudioStreamPlayer _cropInSlotSound;

    [Export]
    private AudioStreamPlayer _smashSound;

    [Export]
    private AudioStreamPlayer _dropSound;

    [Export]
    private AudioStreamPlayer _pickUpSound;

    private int _growthStage;

    private bool _falling;

    private float _velocity;

    private Action _afterBounceCallback;

    private bool _bouncing;

    private Vector2 _bounceFrom;

    private Vector2 _bounceTo;

    private float _bounceProgress;
    
    private IEventHandler _soulCountUpdatedEventHandler;
    
    private bool _soulsAvailable;

    public override void _Ready()
    {
        _soulParticles.Emitting = false;
        _soulReadySprite.Visible = false;
        
        _sprite.Texture = _cropResource.SpriteTexture;
        _sprite.Hframes = _cropResource.GrowthStageCount;

        _growthTimer.Timeout += OnGrowthTimer;
        _startGrowthTimer.Timeout += StartGrowingNowReally;

        var requestSoulCountEvent = new RequestSoulCountEvent();
        requestSoulCountEvent.Callback = SoulCountUpdate;
        requestSoulCountEvent.Emit();
    }

    public override void _EnterTree()
    {
        _soulCountUpdatedEventHandler = SoulCountUpdatedEvent.Listen(OnSoulCountUpdated);
    }

    public override void _ExitTree()
    {
        EventBus.Unregister(ref _soulCountUpdatedEventHandler);
    }

    private void OnSoulCountUpdated(SoulCountUpdatedEvent soulCountUpdatedEvent)
    {
        SoulCountUpdate(soulCountUpdatedEvent.SoulCount);
    }

    private void SoulCountUpdate(int soulCount)
    {
        _soulsAvailable = soulCount >= _soulsNeeded;
        WiggleIfPossible();
    }

    private void WiggleIfPossible()
    {
        if (FullyGrown && _soulsAvailable && !PickedUp)
        {
            StartWiggling();
        }
        else
        {
            StopWiggling();
        }
    }

    private void StartWiggling()
    {
        _soulParticles.Emitting = true;
        _soulReadySprite.Visible = true;
        _sprite.Visible = false;
        _soulReadyCollisionShape.Disabled = false;
    }

    private void StopWiggling()
    {
        _soulParticles.Emitting = false;
        _soulReadySprite.Visible = false;
        _sprite.Visible = true;
        _soulReadyCollisionShape.Disabled = true;
    }

    public void StartGrowing(float startTime)
    {
        _startGrowthTimer.Start(startTime);
        _collisionAreaShape.Disabled = false;
    }

    private void StartGrowingNowReally()
    {
        _growthTimer.Start();
    }

    public void Drop()
    {
        _velocity = 0;
        _falling = true;
        PickedUp = true;
        _collisionAreaShape.Disabled = true;
        _growthTimer.Stop();
        _startGrowthTimer.Stop();
        var cropDropEvent = new CropDropEvent();
        cropDropEvent.Crop = this;
        cropDropEvent.Emit();
        _dropSound.PitchScale = Random.Pitch(0.05F);
        _dropSound.Play();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_falling)
        {
            Fall(delta);
        }

        if (_bouncing)
        {
            Bounce(delta);
        }
    }

    private void Fall(double delta)
    {
        _velocity += (float)delta * _gravity;
        GlobalPosition += new Vector2(0, (float)delta * _velocity);
        if (GlobalPosition.y > 164)
        {
            Land();
        }
    }

    private void Bounce(double delta)
    {
        var bounceDistance = (float)delta * _bounceVelocity;
        _bounceProgress += bounceDistance;
        var bounceSegment = _bounceTo - _bounceFrom;
        var progress = _bounceProgress / bounceSegment.Length();
        var height = _bounceCurve.SampleBaked(progress) * _bounceHeight;

        var newPosition = bounceSegment * progress;
        newPosition += new Vector2(0, -height);

        GlobalPosition = _bounceFrom + newPosition;

        if (progress >= 1)
        {
            LandAfterBounce();
        }
    }

    private void Land()
    {
        _falling = false;
        var cropLandedEvent = new CropLandedEvent();
        cropLandedEvent.Crop = this;
        cropLandedEvent.Emit();
        _bounceSound.PitchScale = Random.Pitch();
        _bounceSound.Play();
    }

    private void LandAfterBounce()
    {
        _cropInSlotSound.PitchScale = Random.Pitch();
        _cropInSlotSound.Play();
        GlobalPosition = _bounceTo;
        _bouncing = false;
        PickedUp = false;
        StartGrowing(1F);
        _afterBounceCallback.Invoke();
        _afterBounceCallback = null;
        WiggleIfPossible();
    }

    private void OnGrowthTimer()
    {
        _growthStage++;
        _sprite.Frame = _growthStage;

        if (FullyGrown)
        {
            _growthTimer.Stop();
            WiggleIfPossible();
        }
    }

    public void PickUp()
    {
        PickedUp = true;
        StopWiggling();
        EmitSignal(SignalName.CropPickedUp, this);
        _collisionAreaShape.Disabled = true;
        _growthTimer.Stop();
        _startGrowthTimer.Stop();
        RemoveChild(_pickUpSound);
        AddSibling(_pickUpSound);
        _pickUpSound.PitchScale = Random.Pitch();
        _pickUpSound.Play();
        _pickUpSound.Finished += _pickUpSound.QueueFree;
    }

    public void Smash()
    {
        RemoveChild(_smashSound);
        AddSibling(_smashSound);
        _smashSound.PitchScale = Random.Pitch();
        _smashSound.Play();
        _smashSound.Finished += _smashSound.QueueFree;
        QueueFree();
    }

    public void BounceToSlot(Vector2 closestSlot, Action afterBounceCallback)
    {
        _bounceProgress = 0;
        _bounceFrom = GlobalPosition;
        _bounceTo = closestSlot;
        _afterBounceCallback = afterBounceCallback;
        if (_bounceTo.DistanceTo(_bounceFrom) < 3)
        {
            LandAfterBounce();
        }
        else
        {
            _bouncing = true;
        }
    }

    public void Awaken()
    {
        var useSoulsEvent = new UseSoulsEvent();
        useSoulsEvent.Amount = _soulsNeeded;
        useSoulsEvent.Callback += usePossible =>
        {
            if (usePossible)
            {
                var spawnLivingCropEvent = new SpawnLivingCropEvent();
                spawnLivingCropEvent.Position = GlobalPosition;
                spawnLivingCropEvent.Emit();
                PickedUp = true;
                _collisionAreaShape.Disabled = true;
                _growthTimer.Stop();
                _startGrowthTimer.Stop();
                EmitSignal(SignalName.CropPickedUp, this);
                QueueFree();
            }
        };
        useSoulsEvent.Emit();
    }
}
