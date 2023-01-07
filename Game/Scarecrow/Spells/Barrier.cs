using Godot;

namespace ldjam52.Game.Scarecrow.Spells;

public partial class Barrier : Node2D
{
    [Export]
    private Line2D _line;

    [Export]
    private Timer _lifetimeTimer;

    [Export]
    private float _maxLength;

    [Export]
    private CollisionShape2D _collisionShape;
    
    private Vector2 _startPosition;
    private Vector2 _endPosition;
    
    private SegmentShape2D _segmentShape;

    public override void _Ready()
    {
        _segmentShape = new SegmentShape2D();
        _collisionShape.Shape = _segmentShape;
        
        _line.Visible = false;
        _line.Points = new Vector2[2];

        _lifetimeTimer.Timeout += QueueFree;
    }

    public void Begin(Vector2 startPosition)
    {
        _line.Visible = true;
        UpdatePositions(startPosition, startPosition);
    }

    public void EndPosition(Vector2 endPosition)
    {
        UpdatePositions(_startPosition, endPosition);
    }

    private void UpdatePositions(Vector2 startPosition, Vector2 endPosition)
    {
        var difference = endPosition - startPosition;
        difference = difference.LimitLength(_maxLength);
        _startPosition = startPosition;
        _endPosition = startPosition + difference;
        _line.Points = new[]
        {
            _line.ToLocal(_startPosition),
            _line.ToLocal(_endPosition)
        };
        _segmentShape.A = _startPosition;
        _segmentShape.B = _endPosition;
    }

    public void Cast()
    {
        _lifetimeTimer.Start();
        _collisionShape.Disabled = false;
    }

    public Vector2 Normal()
    {
        return (_endPosition - _startPosition).Normalized().Rotated(Mathf.DegToRad(-90));
    }

    public void Destroy()
    {
        QueueFree();
    }
}
