using Godot;
using ldjam52.Game.Scarecrow.Spells;

namespace ldjam52.Game.UserInterface;

public partial class SoulCounter : Control
{
	[Export]
	private int _maxSoulCount;
	
	[Export]
	private PackedScene _soulIconScene;
	
	public override void _Ready()
	{
		SoulHarvestedEvent.Listen(OnSoulHarvested);
	}

	private void OnSoulHarvested(SoulHarvestedEvent soulHarvestedEvent)
	{
		if (GetChildCount() < _maxSoulCount)
		{
			var soulIcon = _soulIconScene.Instantiate();
			AddChild(soulIcon);
		}
	}
	
}
