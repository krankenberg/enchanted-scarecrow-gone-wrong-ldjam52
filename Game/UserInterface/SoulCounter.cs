using Godot;
using ldjam52.Game.Scarecrow.Spells;

namespace ldjam52.Game.UserInterface;

public partial class SoulCounter : Control
{
	[Export]
	private int _maxSoulCount;
	
	[Export]
	private PackedScene _soulIconScene;

	private int _soulCount;
	
	public override void _Ready()
	{
		SoulHarvestedEvent.Listen(OnSoulHarvested);
		RequestSoulCountEvent.Listen(OnRequestSoulCountEvent);
		UseSoulsEvent.Listen(OnUseSoulEvent);
	}

	private void OnUseSoulEvent(UseSoulsEvent useSoulsEvent)
	{
		if (useSoulsEvent.Amount <= _soulCount)
		{
			for (int i = 0; i < useSoulsEvent.Amount; i++)
			{
				GetChild(i).QueueFree();
			}

			_soulCount -= useSoulsEvent.Amount;
			useSoulsEvent.Callback.Invoke(true);
			EmitSoulCountUpdate();
		}
		else
		{
			useSoulsEvent.Callback.Invoke(false);
		}
	}

	private void OnSoulHarvested(SoulHarvestedEvent soulHarvestedEvent)
	{
		if (_soulCount < _maxSoulCount)
		{
			var soulIcon = _soulIconScene.Instantiate();
			AddChild(soulIcon);
			_soulCount++;
			EmitSoulCountUpdate();
		}
	}

	private void EmitSoulCountUpdate()
	{
		var soulCountUpdatedEvent = new SoulCountUpdatedEvent();
		soulCountUpdatedEvent.SoulCount = _soulCount;
		soulCountUpdatedEvent.Emit();
	}

	private void OnRequestSoulCountEvent(RequestSoulCountEvent requestSoulCountEvent)
	{
		requestSoulCountEvent.Callback.Invoke(_soulCount);
	}

}
