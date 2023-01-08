using Godot;
using ldjam52.Game.Utils;

namespace ldjam52.Game.Field.Crops;

public partial class LivingCrop : Node2D
{
    [Export]
    private StringName _awakenAnimationName;
    
    [Export]
    private StringName _walkingAnimationName;
    
    [Export]
    private AnimatedSprite2D _sprite;

    [Export]
    private float _velocity;

    private bool _walking;

    private int _direction;

    public override void _Ready()
    {
        _sprite.FlipH = Random.Generator.Randf() > 0.5F;
        _sprite.Animation = _awakenAnimationName;


        _sprite.Connect(AnimatedSprite2D.SignalName.AnimationFinished, new Callable(this, MethodName.OnAwaken), (uint) ConnectFlags.OneShot);
    }

    private void OnAwaken()
    {
        _sprite.Animation = _walkingAnimationName;
        _walking = true;
        _direction = GlobalPosition.x > 160 ? 1 : -1;
        _sprite.FlipH = _direction == -1;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_walking)
        {
            return;
        }

        var distance = _velocity * (float) delta;
        GlobalPosition += new Vector2(distance * _direction, 0);

        if (GlobalPosition.x is < -10 or > 330)
        {
            QueueFree();
        }
    }
}
