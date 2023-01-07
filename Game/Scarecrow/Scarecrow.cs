using Godot;
using ldjam52.Game.Input;
using ldjam52.Game.Scarecrow.Spells;
using ldjam52.Game.UserInterface;

namespace ldjam52.Game.Scarecrow;

public partial class Scarecrow : Node2D
{
    [Export]
    private PackedScene _barrierScene;

    private Barrier _currentBarrier;
    
    private bool _gameOver;

    public override void _Ready()
    {
        GameOverEvent.Listen(_ => _gameOver = true);
    }

    public override void _Process(double delta)
    {
        if (_currentBarrier != null)
        {
            if (_gameOver)
            {
                HandleInteractEnd();
                return;
            }

            _currentBarrier.EndPosition(GetGlobalMousePosition());
        }
        
    }

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (_gameOver)
        {
            return;
        }
        
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
