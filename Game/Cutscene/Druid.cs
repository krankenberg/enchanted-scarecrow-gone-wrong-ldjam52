using Godot;
using ldjam52.Game.Utils;

namespace ldjam52.Game.Cutscene;

public partial class Druid : Node2D
{
    private static readonly StringName WalkingAnimation = new("walking");
    private static readonly StringName CastingAnimation = new("casting");

    [Signal]
    public delegate void TargetReachedEventHandler();

    [Export]
    private AnimatedSprite2D _sprite;

    [Export]
    private float _velocity;

    [Export]
    private AudioStreamPlayer _walkSound;

    [Export]
    private AudioStreamPlayer _castSound;

    private bool _walking;
    private Vector2 _target;

    public override void _Ready()
    {
        _sprite.AnimationFinished += OnAnimationFinished;
    }

    private void OnAnimationFinished()
    {
        if (_walking)
        {
            _walkSound.PitchScale = Random.Pitch();
            _walkSound.Play();
        }
    }

    public void WalkTo(Vector2 target)
    {
        _castSound.Playing = false;
        _walking = true;
        _target = target;
        _sprite.Animation = WalkingAnimation;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_walking)
        {
            return;
        }

        var movementDistance = (float)delta * _velocity;
        var neededMovement = _target - GlobalPosition;
        _sprite.FlipH = neededMovement.x < 0;
        if (movementDistance < neededMovement.Length())
        {
            GlobalPosition = GlobalPosition.MoveToward(_target, movementDistance);
        }
        else
        {
            _walking = false;
            GlobalPosition = _target;
            EmitSignal(SignalName.TargetReached);
        }
    }

    public void Cast()
    {
        _castSound.Playing = true;
        _sprite.Animation = CastingAnimation;
    }
}
