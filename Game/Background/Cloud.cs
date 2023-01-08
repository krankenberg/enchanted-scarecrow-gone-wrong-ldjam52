using Godot;
using ldjam52.Game.Utils;

namespace ldjam52.Game.Background;

public partial class Cloud : Node2D
{
	[Export]
	private Texture2D[] _textures;

	[Export]
	private Sprite2D _sprite;

	[Export]
	private float _minSpeed;

	[Export]
	private float _maxSpeed;

	[Export]
	private float _sinModifier;

	[Export]
	private float _sinHeightMin;
	
	[Export]
	private float _sinHeightMax;

	[Export]
	private VisibleOnScreenNotifier2D _visibleOnScreenNotifier;

	private float _speed;

	private float _startHeight;

	private float _sinHeight;
	
	public override void _Ready()
	{
		Randomize();
		_startHeight = GlobalPosition.y;
		_visibleOnScreenNotifier.ScreenExited += OnScreenExited;
	}

	private void Randomize()
	{
		_sprite.Texture = _textures[Random.Generator.RandiRange(0, _textures.Length - 1)];
		_speed = Random.Generator.RandfRange(_minSpeed, _maxSpeed);
		_sinHeight = Random.Generator.RandfRange(_sinHeightMin, _sinHeightMax);
	}

	private void OnScreenExited()
	{
		Randomize();
		GlobalPosition = new Vector2(-_sprite.Texture.GetWidth(), _startHeight);
	}

	public override void _PhysicsProcess(double delta)
	{
		GlobalPosition = new Vector2(GlobalPosition.x + (float)delta * _speed, _startHeight + Mathf.Sin(GlobalPosition.x * _sinModifier) * _sinHeight);
	}
}
