using System;
using Godot;

namespace ldjam52.Game.Tutorial;

public partial class MouseCursor : Node2D
{
    private const int NoButton = 0;
    private const int LeftButton = 1;
    private const int RightButton = 2;

    private enum Stage
    {
        WaitingAtStart,
        Moving,
        WaitingAtEnd,
    }

    [Export]
    private Sprite2D _sprite;

    [Export]
    private float _velocity;

    [Export]
    private float _waitTime;

    private bool _looping;
    private Vector2 _loopFrom;
    private Vector2 _loopTo;
    private int _loopButton;
    private Stage _loopStage;
    private float _loopProgress;
    private Color _lineColor;
    private bool _singleClick;

    public override void _Ready()
    {
        Visible = false;
    }

    private void PressButton(int button)
    {
        _sprite.Frame = button;
    }

    public void LoopRightClick(Vector2 from, Vector2 to, Color color)
    {
        Loop(from, to, color, RightButton);
    }
    
    public void LoopLeftClick(Vector2 from, Vector2 to, Color color)
    {
        Loop(from, to, color, LeftButton);
    }
    
    public void LoopLeftClickNoLine(Vector2 from, Vector2 to)
    {
        Loop(from, to, Colors.White, LeftButton);
        _singleClick = true;
    }

    private void Loop(Vector2 from, Vector2 to, Color color, int button)
    {
        _singleClick = false;
        _sprite.GlobalPosition = from;
        _loopButton = button;
        _loopFrom = from;
        _loopTo = to;
        _looping = true;
        _loopStage = Stage.WaitingAtStart;
        _loopProgress = 0;
        _lineColor = color;
        Visible = true;
    }

    public void StopLoop()
    {
        Visible = false;
        _looping = false;
    }

    public override void _Process(double delta)
    {
        if (!_looping)
        {
            return;
        }
        
        QueueRedraw();
        
        if (_loopStage == Stage.WaitingAtStart)
        {
            _sprite.GlobalPosition = _loopFrom;
            PressButton(NoButton);
            WaitUntilNextStage(delta);
        }
        else if (_loopStage == Stage.Moving)
        {
            PressButton(_singleClick ? NoButton : _loopButton);
            var movementDelta = (float) delta * _velocity;
            _sprite.GlobalPosition = _sprite.GlobalPosition.MoveToward(_loopTo, movementDelta);
            if (movementDelta >= _sprite.GlobalPosition.DistanceTo(_loopTo))
            {
                NextStage();
            }
        }
        else if (_loopStage == Stage.WaitingAtEnd)
        {
            PressButton(_singleClick ? _loopButton : NoButton);
            _sprite.GlobalPosition = _loopTo;
            WaitUntilNextStage(delta);
        }
    }

    private void WaitUntilNextStage(double delta)
    {
        _loopProgress += (float)delta;
        if (_loopProgress >= _waitTime)
        {
            NextStage();
            _loopProgress = 0;
        }
    }

    private void NextStage()
    {
        _loopStage = _loopStage switch
        {
            Stage.WaitingAtStart => Stage.Moving,
            Stage.Moving => Stage.WaitingAtEnd,
            Stage.WaitingAtEnd => Stage.WaitingAtStart,
            _ => _loopStage
        };
    }

    public override void _Draw()
    {
        if (!_looping || _singleClick)
        {
            return;
        }
        
        DrawDashedLine(ToLocal(_loopFrom), _sprite.Position, _lineColor);
    }
}
