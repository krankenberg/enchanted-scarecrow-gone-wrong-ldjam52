using Godot;
using ldjam52.Game.Scarecrow.Spells;

namespace ldjam52.Game.Crows;

public partial class Crow : Node2D
{
	public Vector2 StartPosition;
	public Vector2? Target;

	[Export]
	private float _speed;

	[Export]
	private Sprite2D _sprite;

	[Export]
	private Area2D _collisionArea;

	private bool _flyingBackUp;

	public override void _Ready()
	{
		_collisionArea.AreaEntered += OnAreaEntered;
	}

	private void OnAreaEntered(Area2D area)
	{
		var owner = area.Owner;
		if (owner is Barrier barrier)
		{
			FlyBack(Target!.Value - GlobalPosition, barrier.Normal());
			barrier.Destroy();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!Target.HasValue)
		{
			return;
		}

		var target = Target.Value;
		var difference = target - GlobalPosition;

		var movementDistance = _speed * (float)delta;
		var direction = difference.Normalized();
		if (difference.Length() <= movementDistance)
		{
			if (_flyingBackUp)
			{
				QueueFree();
				return;
			}
			
			GlobalPosition = target;
			FlyBack(direction, Vector2.Up);
		}
		else
		{
			_sprite.FlipH = direction.x < 0;
			GlobalPosition += direction * movementDistance;
		}
	}

	private void FlyBack(Vector2 movementDirection, Vector2 reflectionDirection)
	{
		_collisionArea.AreaEntered -= OnAreaEntered;
		_flyingBackUp = true;
		
		var directionBackUp = movementDirection.Bounce(reflectionDirection);
		directionBackUp.y *= directionBackUp.y < 0 ? 1 : -1;
		
		var minUpAngle = Mathf.DegToRad(-135);
		var maxUpAngle = Mathf.DegToRad(-45);
		directionBackUp = Vector2.Right.Rotated(Mathf.Clamp(directionBackUp.Angle(), minUpAngle, maxUpAngle));
		
		var intersection = Geometry2D.LineIntersectsLine(GlobalPosition, directionBackUp, StartPosition, Vector2.Right);
		Target = intersection.AsVector2();
	}
}
