using Godot;
using ldjam52.Game.Field;
using ldjam52.Game.Field.Crops;
using ldjam52.Game.Scarecrow.Spells;
using ldjam52.Game.Utils;

namespace ldjam52.Game.Crows;

public partial class Crow : Node2D
{
	[Signal]
	public delegate void CrowCollidedWithBarrierEventHandler();

	[Export]
	private float _speed;

	[Export]
	private AnimatedSprite2D _sprite;

	[Export]
	private Area2D _collisionArea;

	[Export]
	private Marker2D _slot;

	[Export]
	private float _cropSpawnProbability;

	[Export]
	private VisibleOnScreenNotifier2D _visibleOnScreenNotifier;

	[Export]
	private AudioStreamPlayer _visibleSound;

	[Export]
	private AudioStreamPlayer _bounceSound;

	private bool _flyingBackUp;

	private Vector2 _startPosition;
	
	private Crop _target;
	
	private Vector2 _targetPosition;

	private bool _pausedForTutorial;

	public override void _Ready()
	{
		_collisionArea.Connect(Area2D.SignalName.AreaEntered, new Callable(this, MethodName.OnAreaEntered));
		_visibleOnScreenNotifier.Connect(VisibleOnScreenNotifier2D.SignalName.ScreenEntered, new Callable(this, MethodName.VisibleOnScreen), (uint) ConnectFlags.OneShot);
		CrowsOnFieldEvent.Emit(true);
	}

	private void VisibleOnScreen()
	{
		_visibleSound.PitchScale = Random.Pitch(0.1F);
		_visibleSound.Play();
	}

	private void OnAreaEntered(Area2D area)
	{
		if (_pausedForTutorial || _flyingBackUp)
		{
			return;
		}
		
		var owner = area.Owner;
		if (owner is Barrier barrier)
		{
			EmitSignal(SignalName.CrowCollidedWithBarrier);
			CrowBlockedEvent.Emit();
			_target.Disconnect(Crop.SignalName.CropPickedUp, new Callable(this, MethodName.OnTargetPickedUp));
			FlyBack(_target.GlobalPosition - GlobalPosition, barrier.Normal());
			barrier.Destroy();
			_bounceSound.PitchScale = Random.Pitch(0.05F);
			_bounceSound.Play();
			if (Random.Generator.Randf() <= _cropSpawnProbability)
			{
				CallDeferred(MethodName.SpawnCrop);
			}
		}
	}

	private void SpawnCrop()
	{
		var fallingCropSpawnEvent = new FallingCropSpawnEvent();
		fallingCropSpawnEvent.Position = GlobalPosition;
		fallingCropSpawnEvent.Emit();
	}

	public void GrabCrop(Vector2 startPosition, Crop crop)
	{
		_startPosition = startPosition;
		_target = crop;
		_targetPosition = _target.GlobalPosition;
		_target.Connect(Crop.SignalName.CropPickedUp, new Callable(this, MethodName.OnTargetPickedUp), (uint) ConnectFlags.OneShot);
	}

	private void OnTargetPickedUp(Crop crop)
	{
		var requestCropEvent = new RequestCropEvent();
		requestCropEvent.Callback = newCrop =>
		{
			GrabCrop(_startPosition, newCrop);
		};
		requestCropEvent.NoCropAvailableCallback = () =>
		{
			FlyBack(_target.GlobalPosition - GlobalPosition, Vector2.Up);
		};
		requestCropEvent.Emit();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_pausedForTutorial || _target == null)
		{
			return;
		}

		var difference = _targetPosition - GlobalPosition;

		var movementDistance = _speed * (float)delta;
		var direction = difference.Normalized();
		if (difference.Length() <= movementDistance)
		{
			if (_flyingBackUp)
			{
				if (_slot.GetChildCount() > 0)
				{
					CrowEscapedEvent.Emit();
				}
				CrowsOnFieldEvent.Emit(false);
				QueueFree();
				return;
			}
			
			GlobalPosition = _targetPosition;
			_target.Disconnect(Crop.SignalName.CropPickedUp, new Callable(this, MethodName.OnTargetPickedUp));
			_target.PickUp();
			_target.GetParent().RemoveChild(_target);
			_slot.AddChild(_target);
			_target.Position = Vector2.Zero;
			_target.ZIndex = ZIndex - 1;
			_target.Rotate(Mathf.DegToRad(180));
			FlyBack(direction, Vector2.Up);
		}
		else
		{
			_sprite.FlipH = direction.x < 0;
			GlobalPosition += direction * movementDistance;
		}
	}

	private void FlyBack(Vector2 movementDirection, Vector2 reflectionNormal)
	{
		_flyingBackUp = true;
		
		var directionBackUp = movementDirection.Bounce(reflectionNormal);
		directionBackUp.y *= directionBackUp.y < 0 ? 1 : -1;
		
		var minUpAngle = Mathf.DegToRad(-135);
		var maxUpAngle = Mathf.DegToRad(-45);
		directionBackUp = Vector2.Right.Rotated(Mathf.Clamp(directionBackUp.Angle(), minUpAngle, maxUpAngle));
		
		var intersection = Geometry2D.LineIntersectsLine(GlobalPosition, directionBackUp, _startPosition, Vector2.Right);
		_targetPosition = intersection.AsVector2();
	}

	public void Pause()
	{
		_pausedForTutorial = true;
		_sprite.SpeedScale = 0;
	}

	public void Unpause()
	{
		_pausedForTutorial = false;
		_sprite.SpeedScale = 1;
	}
}
