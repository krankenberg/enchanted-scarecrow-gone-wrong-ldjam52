using Godot;

namespace ldjam52.Game.UserInterface;

public partial class GameOverPanelEnabler : Node
{
    [Export]
    private PackedScene _gameOverPanelScene;

    public override void _Ready()
    {
        GameOverEvent.Listen(_ => GameOver());
    }

    private void GameOver()
    {
        var gameOverPanel = _gameOverPanelScene.Instantiate();
        AddSibling(gameOverPanel);
    }
}
