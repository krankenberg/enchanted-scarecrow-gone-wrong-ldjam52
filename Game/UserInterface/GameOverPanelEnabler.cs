using Godot;
using ldjam52.Game.Crows;
using ldjam52.Game.Farmers;
using ldjam52.Game.Field.Crops;
using ldjam52.Game.Scarecrow.Spells;

namespace ldjam52.Game.UserInterface;

public partial class GameOverPanelEnabler : Node
{
    [Export]
    private PackedScene _gameOverPanelScene;

    private Score _score;

    public override void _Ready()
    {
        _score = new Score();
        GameOverEvent.Listen(_ => GameOver());
        SoulHarvestedEvent.Listen(_ => _score.SoulsCollected++);
        UseSoulsEvent.Listen(useSoulsEvent => _score.SoulsSpent += useSoulsEvent.Amount);
        CrowEscapedEvent.Listen(_ => _score.CrowsEscaped++);
        CrowBlockedEvent.Listen(_ => _score.CrowsBlocked++);
        FarmerEscapedEvent.Listen(_ => _score.FarmersEscaped++);
        CropEscapedEvent.Listen(_ => _score.CropsEscaped++);
    }

    private void GameOver()
    {
        var gameOverPanel = _gameOverPanelScene.Instantiate<GameOverPanel>();
        AddSibling(gameOverPanel);
        gameOverPanel.UpdateScore(_score.Copy());
    }
}
