using Godot;

namespace ldjam52.Game.Crows;

public partial class Crow : Node2D
{
	public Vector2 StartPosition;
	public Vector2? Target;

	[Export]
	private float _speed;

	[Export]
	private Sprite2D _sprite;

	private bool _flyingBackUp;

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
			_flyingBackUp = true;
			var directionBackUp = direction.Reflect(Vector2.Up);
			var intersection = Geometry2D.LineIntersectsLine(GlobalPosition, directionBackUp, StartPosition, Vector2.Right);
			Target = intersection.AsVector2();
		}
		else
		{
			_sprite.FlipH = direction.x < 0;
			GlobalPosition += direction * movementDistance;
		}
	}
}
