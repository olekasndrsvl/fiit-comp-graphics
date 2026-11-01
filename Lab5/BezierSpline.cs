namespace Lab5;

internal readonly record struct BezierSegment(
    PointF P0,
    PointF P1,
    PointF P2,
    PointF P3);

internal sealed class BezierSpline
{
    private readonly List<PointF> _points = [];
    private int _activePoint = -1;

    public IReadOnlyList<PointF> Points => _points;

    public void AddPoint(PointF point)
    {
        _points.Add(point);
    }

    public void SetPoints(IEnumerable<PointF> points)
    {
        _points.Clear();
        _points.AddRange(points);
        _activePoint = -1;
    }

    public void BeginEdit(PointF point)
    {
        _activePoint = FindNearest(point, 10f);

        if (_activePoint >= 0)
            return;

        _points.Add(point);
        _activePoint = _points.Count - 1;
    }

    public void MoveActive(PointF point)
    {
        if (_activePoint >= 0)
            _points[_activePoint] = point;
    }

    public void EndEdit()
    {
        _activePoint = -1;
    }

    public void RemoveNearest(PointF point)
    {
        int index = FindNearest(point, 10f);

        if (index >= 0)
            _points.RemoveAt(index);

        _activePoint = -1;
    }

    public void Clear()
    {
        _points.Clear();
        _activePoint = -1;
    }

    public List<PointF> BuildPolyline(int stepsPerSegment)
    {
        var polyline = new List<PointF>();

        if (_points.Count < 2)
            return polyline;

        int steps = Math.Max(1, stepsPerSegment);

        for (int step = 0; step <= steps; step++)
        {
            float t = (float)step / steps;
            polyline.Add(EvaluateGeneral(_points, t));
        }

        return polyline;
    }

    private static PointF EvaluateGeneral(
        IReadOnlyList<PointF> points,
        float t)
    {
        var temp = points.ToArray();

        for (int level = temp.Length - 1; level > 0; level--)
        {
            for (int i = 0; i < level; i++)
            {
                temp[i] = new PointF(
                    (1f - t) * temp[i].X + t * temp[i + 1].X,
                    (1f - t) * temp[i].Y + t * temp[i + 1].Y);
            }
        }

        return temp[0];
    }

    private int FindNearest(PointF point, float radius)
    {
        float radiusSquared = radius * radius;
        int nearestIndex = -1;
        float minDistanceSquared = radiusSquared;

        for (int i = 0; i < _points.Count; i++)
        {
            float dx = _points[i].X - point.X;
            float dy = _points[i].Y - point.Y;
            float distanceSquared = dx * dx + dy * dy;

            if (distanceSquared < minDistanceSquared)
            {
                minDistanceSquared = distanceSquared;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }
}