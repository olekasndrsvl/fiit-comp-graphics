namespace Lab5;

internal sealed class BezierSpline
{
    private readonly List<PointF> _points = [];
    private int _activePoint = -1;

    public IReadOnlyList<PointF> Points => _points;

    public void SetPoints(IEnumerable<PointF> points)
    {
        _points.Clear();
        _points.AddRange(points);
        _activePoint = -1;
    }

    public void BeginEdit(PointF point)
    {
        _activePoint = FindNearest(point, 10f);
        if (_activePoint >= 0) return;
        _points.Add(point);
        _activePoint = _points.Count - 1;
    }

    public void MoveActive(PointF point)
    {
        if (_activePoint >= 0) _points[_activePoint] = point;
    }

    public void EndEdit() => _activePoint = -1;

    public void RemoveNearest(PointF point)
    {
        var index = FindNearest(point, 10f);
        if (index >= 0) _points.RemoveAt(index);
        _activePoint = -1;
    }

    public void Clear()
    {
        _points.Clear();
        _activePoint = -1;
    }

    private int FindNearest(PointF point, float radius)
    {
        var radiusSquared = radius * radius;
        for (var i = 0; i < _points.Count; i++)
        {
            var dx = _points[i].X - point.X;
            var dy = _points[i].Y - point.Y;
            if (dx * dx + dy * dy <= radiusSquared) return i;
        }
        return -1;
    }
}
