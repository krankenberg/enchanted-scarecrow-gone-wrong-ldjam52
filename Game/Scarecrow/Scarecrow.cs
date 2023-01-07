using System;
using Godot;
using ldjam52.Game.Input;
using ldjam52.Game.Scarecrow.Spells;

namespace ldjam52.Game.Scarecrow;

public partial class Scarecrow : Node2D
{
    [Export]
    private PackedScene _barrierScene;

    private Barrier _currentBarrier;

    public override void _Process(double delta)
    {
        _currentBarrier?.EndPosition(GetGlobalMousePosition());
    }

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed(InputConstants.Interact))
        {
            HandleInteractStart();
        }
        if (inputEvent.IsActionReleased(InputConstants.Interact))
        {
            HandleInteractEnd();
        }
    }

    private void HandleInteractStart()
    {
        if (_currentBarrier == null)
        {
            _currentBarrier = _barrierScene.Instantiate<Barrier>();
            GetParent().AddChild(_currentBarrier);
            
            _currentBarrier.Begin(GetGlobalMousePosition());
        }
    }

    private void HandleInteractEnd()
    {
        _currentBarrier.Cast();
        _currentBarrier = null;
    }
}
