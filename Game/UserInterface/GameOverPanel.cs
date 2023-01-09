using Godot;
using ldjam52.Game.Events;

namespace ldjam52.Game.UserInterface;

public class Score
{
    public int SoulsCollected;
    public int SoulsSpent;
    public int CrowsBlocked;
    public int CrowsEscaped;
    public int FarmersEscaped;
    public int CropsEscaped;

    public Score Copy()
    {
        var copy = new Score();
        copy.SoulsCollected = SoulsCollected;
        copy.SoulsSpent = SoulsSpent;
        copy.CrowsBlocked = CrowsBlocked;
        copy.CrowsEscaped = CrowsEscaped;
        copy.FarmersEscaped = FarmersEscaped;
        copy.CropsEscaped = CropsEscaped;
        return copy;
    }
}

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

    public void UpdateScore(Score score)
    {
        GD.Print("got score " + score);
    }
}
