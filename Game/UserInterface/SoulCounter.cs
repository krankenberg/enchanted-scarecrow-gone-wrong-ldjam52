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
		RequestSoulCountEvent.Listen(OnRequestSoulCountEvent);
	}

	private void OnSoulHarvested(SoulHarvestedEvent soulHarvestedEvent)
	{
		var soulCount = GetChildCount();
		if (soulCount < _maxSoulCount)
		{
			var soulIcon = _soulIconScene.Instantiate();
			AddChild(soulIcon);
			var soulCountUpdatedEvent = new SoulCountUpdatedEvent();
			soulCountUpdatedEvent.SoulCount = soulCount + 1;
			soulCountUpdatedEvent.Emit();
		}
	}

	private void OnRequestSoulCountEvent(RequestSoulCountEvent requestSoulCountEvent)
	{
		requestSoulCountEvent.Callback.Invoke(GetChildCount());
	}
}
