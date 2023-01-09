using Godot;

namespace ldjam52.Game.Cutscene;

public partial class CutScene : Node2D
{
    private enum Stage
    {
        WalkingToCastPosition,
        Casting,
        WalkingHalfToEndPosition,
        WalkingToEndPosition
    }

    [Export]
    private Druid _druid;

    [Export]
    private Marker2D _castPosition;

    [Export]
    private Marker2D _halfEndPosition;

    [Export]
    private Marker2D _endPosition;

    [Export]
    private GPUParticles2D _awakenParticles;

    [Export]
    private Timer _firstTimer;

    [Export]
    private Timer _secondTimer;

    [Export]
    private Timer _thirdTimer;

    [Export]
    private GPUParticles2D _castParticles1;

    [Export]
    private GPUParticles2D _castParticles2;

    private Stage _stage;

    public override void _Ready()
    {
        _stage = Stage.WalkingToCastPosition;
        _druid.TargetReached += OnDruidTargetReached;
        _druid.WalkTo(_castPosition.GlobalPosition);
    }

    private void OnDruidTargetReached()
    {
        if (_stage == Stage.WalkingToCastPosition)
        {
            _druid.Cast();
            _castParticles2.Emitting = true;
            _firstTimer.Timeout += () =>
            {
                _castParticles1.Emitting = true;
                _secondTimer.Start();
            };
            _secondTimer.Timeout += () =>
            {
                _castParticles1.Emitting = false;
                _castParticles2.Emitting = false;
                _thirdTimer.Start();
            };
            _thirdTimer.Timeout += () =>
            {
                _stage = Stage.WalkingHalfToEndPosition;
                _druid.WalkTo(_halfEndPosition.GlobalPosition);
            };
            _firstTimer.Start();
        }
        else if (_stage == Stage.WalkingHalfToEndPosition)
        {
            CutsceneEndedEvent.Emit();
            _awakenParticles.Emitting = true;
            _druid.WalkTo(_endPosition.GlobalPosition);
            _stage = Stage.WalkingToEndPosition;
        }
        else if (_stage == Stage.WalkingToEndPosition)
        {
            QueueFree();
        }
    }
}
