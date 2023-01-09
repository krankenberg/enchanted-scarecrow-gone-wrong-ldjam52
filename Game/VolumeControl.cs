using Godot;
using ldjam52.Game.Input;

namespace ldjam52.Game;

public partial class VolumeControl : Node
{
	private int _masterIndex;

	private bool _muted;

	public override void _Ready()
	{
		_masterIndex = AudioServer.GetBusIndex("Master");
	}

	public override void _UnhandledKeyInput(InputEvent inputEvent)
	{
		if (inputEvent.IsActionPressed(InputConstants.ToggleSound))
		{
			_muted = !_muted;
			AudioServer.SetBusMute(_masterIndex, _muted);
		}

		if (inputEvent.IsActionPressed(InputConstants.IncreaseVolume))
		{
			AudioServer.SetBusVolumeDb(_masterIndex, AudioServer.GetBusVolumeDb(_masterIndex) + 3);
		}

		if (inputEvent.IsActionPressed(InputConstants.DecreaseVolume))
		{
			AudioServer.SetBusVolumeDb(_masterIndex, AudioServer.GetBusVolumeDb(_masterIndex) - 3);
		}
	}
}
