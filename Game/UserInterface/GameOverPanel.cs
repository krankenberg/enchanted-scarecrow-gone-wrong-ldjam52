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

    public int SoulsCollectedBase => 25;
    public int SoulsCollectedSum => SoulsCollectedBase * SoulsCollected;

    public int CropsEscapedBase => 100;
    public int CropsEscapedSum => CropsEscapedBase * CropsEscaped;

    public int CrowsBlockedBase => 25;
    public int CrowsBlockedSum => CrowsBlockedBase * CrowsBlocked;

    public int CrowsEscapedBase => -75;
    public int CrowsEscapedSum => CrowsEscapedBase * CrowsEscaped;

    public int FarmersEscapedBase => -100;
    public int FarmersEscapedSum => FarmersEscapedBase * FarmersEscaped;

    public int Total => SoulsCollectedSum + CropsEscapedSum + CrowsBlockedSum + CrowsEscapedSum + FarmersEscapedSum;
}

public partial class GameOverPanel : PanelContainer
{
    [Export(PropertyHint.File)]
    private string _mainMenuScenePath;

    [Export]
    private TextureButton _retryButton;

    [Export]
    private TextureButton _exitButton;

    [Export]
    private Color _positiveScoreColor;

    [Export]
    private Color _negativeScoreColor;

    [Export]
    private Color _neutralScoreColor;

    [Export]
    private TextureButton _nextButton1;

    [Export]
    private TextureButton _nextButton2;

    [Export]
    private TextureButton _prevButton1;

    [Export]
    private TextureButton _prevButton2;

    [Export]
    private Label _finalScoreSumLabel;

    [Export]
    private Label _carrotsEscapedBaseLabel;

    [Export]
    private Label _carrotsEscapedSumLabel;

    [Export]
    private Label _crowsBlockedBaseLabel;

    [Export]
    private Label _crowsBlockedSumLabel;

    [Export]
    private Label _soulsCollectedBaseLabel;

    [Export]
    private Label _soulsCollectedSumLabel;

    [Export]
    private Label _crowsEscapedBaseLabel;

    [Export]
    private Label _crowsEscapedSumLabel;

    [Export]
    private Label _farmersEscapedBaseLabel;

    [Export]
    private Label _farmersEscapedSumLabel;

    [Export]
    private TabContainer _scoreTabContainer;

    public override void _Ready()
    {
        _retryButton.Pressed += Retry;
        _exitButton.Pressed += Exit;

        _nextButton1.Pressed += () => ChangeTab(1);
        _nextButton2.Pressed += () => ChangeTab(1);
        _prevButton1.Pressed += () => ChangeTab(-1);
        _prevButton2.Pressed += () => ChangeTab(-1);
    }

    private void ChangeTab(int change)
    {
        _scoreTabContainer.CurrentTab += change;
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
        SetScore(_soulsCollectedBaseLabel, score.SoulsCollectedBase, _soulsCollectedSumLabel, score.SoulsCollectedSum, score.SoulsCollected);
        SetScore(_carrotsEscapedBaseLabel, score.CropsEscapedBase, _carrotsEscapedSumLabel, score.CropsEscapedSum, score.CropsEscaped);
        SetScore(_crowsBlockedBaseLabel, score.CrowsBlockedBase, _crowsBlockedSumLabel, score.CrowsBlockedSum, score.CrowsBlocked);
        SetScore(_crowsEscapedBaseLabel, score.CrowsEscapedBase, _crowsEscapedSumLabel, score.CrowsEscapedSum, score.CrowsEscaped);
        SetScore(_farmersEscapedBaseLabel, score.FarmersEscapedBase, _farmersEscapedSumLabel, score.FarmersEscapedSum, score.FarmersEscaped);
        SetSum(score);
    }

    private void SetScore(Label baseLabel, int baseAmount, Label sumLabel, int sum, int amount)
    {
        baseLabel.Text = amount + "x";
        sumLabel.Text = sum.ToString();
        var color = sum > 0 ? _positiveScoreColor : sum == 0 ? _neutralScoreColor : _negativeScoreColor;
        baseLabel.AddThemeColorOverride("font_color", color);
        sumLabel.AddThemeColorOverride("font_color", color);
    }
    private void SetSum(Score score)
    {
        _finalScoreSumLabel.Text = score.Total.ToString();
        var color = score.Total > 0 ? _positiveScoreColor : score.Total == 0 ? _neutralScoreColor : _negativeScoreColor;
        _finalScoreSumLabel.AddThemeColorOverride("font_color", color);
    }
}
