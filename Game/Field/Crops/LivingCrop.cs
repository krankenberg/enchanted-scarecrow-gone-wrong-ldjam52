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

    public override void _Ready()
    {
        _sprite.FlipH = Random.Generator.Randf() > 0.5F;
        _sprite.Animation = _awakenAnimationName;


        _sprite.Connect(AnimatedSprite2D.SignalName.AnimationFinished, new Callable(this, MethodName.OnAwaken), (uint) ConnectFlags.OneShot);
    }

    private void OnAwaken()
    {
        _sprite.Animation = _walkingAnimationName;
    }
    
}
