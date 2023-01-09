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

    private int _livingCropsOnField;
    private int _cropsOnField;
    private int _crowsOnField;

    public override void _Ready()
    {
        _score = new Score();
        CropsOnFieldEvent.Listen(OnCropsOnFieldEvent);
        CrowsOnFieldEvent.Listen(OnCrowsOnFieldEvent);
        LivingCropsOnFieldEvent.Listen(OnLivingCropsOnFieldEvent);
        SoulHarvestedEvent.Listen(_ => _score.SoulsCollected++);
        UseSoulsEvent.Listen(useSoulsEvent => _score.SoulsSpent += useSoulsEvent.Amount);
        CrowEscapedEvent.Listen(_ => _score.CrowsEscaped++);
        CrowBlockedEvent.Listen(_ => _score.CrowsBlocked++);
        FarmerEscapedEvent.Listen(_ => _score.FarmersEscaped++);
        CropEscapedEvent.Listen(_ => _score.CropsEscaped++);
    }

    private void OnLivingCropsOnFieldEvent(LivingCropsOnFieldEvent obj)
    {
        _livingCropsOnField += obj.LivingCropsAvailable ? 1 : -1;
        CheckGameOver();
    }

    private void OnCrowsOnFieldEvent(CrowsOnFieldEvent obj)
    {
        _crowsOnField += obj.CrowsAvailable ? 1 : -1;
        CheckGameOver();
    }

    private void OnCropsOnFieldEvent(CropsOnFieldEvent cropsOnFieldEvent)
    {
        _cropsOnField += cropsOnFieldEvent.CropsAvailable ? 1 : -1;
        CheckGameOver();
    }

    private void CheckGameOver()
    {
        if (_crowsOnField <= 0 && _cropsOnField <= 0 && _livingCropsOnField <= 0)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        GameOverEvent.Emit();
        var gameOverPanel = _gameOverPanelScene.Instantiate<GameOverPanel>();
        AddSibling(gameOverPanel);
        gameOverPanel.UpdateScore(_score.Copy());
    }
}
