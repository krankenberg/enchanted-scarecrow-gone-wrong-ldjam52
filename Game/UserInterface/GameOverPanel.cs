using Godot;
using ldjam52.Game.Events;

namespace ldjam52.Game.UserInterface;

public partial class GameOverPanel : PanelContainer
{
	[Export(PropertyHint.File)]
	private string _mainMenuScenePath;

	[Export]
	private TextureButton _retryButton;

	[Export]
	private TextureButton _exitButton;

	public override void _Ready()
	{
		_retryButton.Pressed += Retry;
		_exitButton.Pressed += Exit;
	}

	private void Retry()
	{
		EventBus.Clear();
		GetTree().ReloadCurrentScene();
	}

	private void Exit()
	{
		GetTree().ChangeSceneToFile(_mainMenuScenePath);
		EventBus.Clear();
	}
}
