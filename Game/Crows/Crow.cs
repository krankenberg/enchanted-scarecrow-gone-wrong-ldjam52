using Godot;
using ldjam52.Game.Field;
using ldjam52.Game.Scarecrow.Spells;

namespace ldjam52.Game.Crows;

public partial class Crow : Node2D
{

	[Export]
	private float _speed;

	[Export]
	private Sprite2D _sprite;

	[Export]
	private Area2D _collisionArea;

	private bool _flyingBackUp;

	private Vector2 _startPosition;
	
	private Crop _target;
	
	private Vector2 _targetPosition;

	public override void _Ready()
	{
		_collisionArea.Connect(Area2D.SignalName.AreaEntered, new Callable(this, MethodName.OnAreaEntered));
	}

	private void OnAreaEntered(Area2D area)
	{
		var owner = area.Owner;
		if (owner is Barrier barrier)
		{
			FlyBack(_target.GlobalPosition - GlobalPosition, barrier.Normal());
			barrier.Destroy();
		}
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
		if (_target == null)
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
				QueueFree();
				return;
			}
			
			GlobalPosition = _targetPosition;
			_target.Disconnect(Crop.SignalName.CropPickedUp, new Callable(this, MethodName.OnTargetPickedUp));
			_target.PickUp();
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
		_collisionArea.Disconnect(Area2D.SignalName.AreaEntered, new Callable(this, MethodName.OnAreaEntered));
		_flyingBackUp = true;
		
		var directionBackUp = movementDirection.Bounce(reflectionNormal);
		directionBackUp.y *= directionBackUp.y < 0 ? 1 : -1;
		
		var minUpAngle = Mathf.DegToRad(-135);
		var maxUpAngle = Mathf.DegToRad(-45);
		directionBackUp = Vector2.Right.Rotated(Mathf.Clamp(directionBackUp.Angle(), minUpAngle, maxUpAngle));
		
		var intersection = Geometry2D.LineIntersectsLine(GlobalPosition, directionBackUp, _startPosition, Vector2.Right);
		_targetPosition = intersection.AsVector2();
	}
}
