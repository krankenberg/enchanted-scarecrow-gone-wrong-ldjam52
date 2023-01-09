using System;
using Godot;
using ldjam52.Game.Events;

namespace ldjam52.Game.MainMenu;

public partial class MainMenu : Node2D
{
    [Export(PropertyHint.File)]
    private string _gameRootScenePath;

    [Export]
    private TextureButton _startButton;

    [Export]
    private TextureButton _exitButton;

    public override void _Ready()
    {
        _startButton.Pressed += Start;
        _exitButton.Pressed += Exit;
    }

    private void Start()
    {
        EventBus.Clear();
        GetTree().ChangeSceneToFile(_gameRootScenePath);
    }

    private void Exit()
    {
        GetTree().Quit();
    }
}
