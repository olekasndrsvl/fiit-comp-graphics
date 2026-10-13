namespace Lab5;

internal readonly record struct BezierSegment(PointF P0, PointF P1, PointF P2, PointF P3);

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

    public List<BezierSegment> BuildSegments(bool closed)
    {
        var segments = new List<BezierSegment>();
        var count = _points.Count;
        if (count < 2)
            return segments;

        if (closed && count >= 3)
        {
            for (var i = 0; i < count; i++)
                segments.Add(BuildSegment(i, (i + 1) % count, true));
            return segments;
        }

        for (var i = 0; i < count - 1; i++)
            segments.Add(BuildSegment(i, i + 1, false));

        return segments;
    }

    public List<PointF> BuildPolyline(bool closed, int stepsPerSegment)
    {
        var polyline = new List<PointF>();
        var segments = BuildSegments(closed);
        var steps = Math.Max(1, stepsPerSegment);

        if (segments.Count == 0)
        {
            if (_points.Count == 1)
                polyline.Add(_points[0]);
            return polyline;
        }

        polyline.Add(segments[0].P0);
        foreach (var segment in segments)
        {
            for (var step = 1; step <= steps; step++)
                polyline.Add(Evaluate(segment, (float)step / steps));
        }

        return polyline;
    }

    public static PointF Evaluate(BezierSegment segment, float t)
    {
        var mt = 1f - t;
        var a = mt * mt * mt;
        var b = 3f * mt * mt * t;
        var c = 3f * mt * t * t;
        var d = t * t * t;

        return new PointF(
            a * segment.P0.X + b * segment.P1.X + c * segment.P2.X + d * segment.P3.X,
            a * segment.P0.Y + b * segment.P1.Y + c * segment.P2.Y + d * segment.P3.Y);
    }

    private BezierSegment BuildSegment(int from, int to, bool closed)
    {
        var p0 = _points[from];
        var p3 = _points[to];
        
        var start = Tangent(from, closed);
        var end = Tangent(to, closed);

        var p1 = new PointF(p0.X + start.X / 3f, p0.Y + start.Y / 3f);
        var p2 = new PointF(p3.X - end.X / 3f, p3.Y - end.Y / 3f);
        return new BezierSegment(p0, p1, p2, p3);
    }

    private PointF Tangent(int index, bool closed)
    {
        var count = _points.Count;

        if (closed)
        {
            var previous = _points[(index - 1 + count) % count];
            var next = _points[(index + 1) % count];
            return new PointF((next.X - previous.X) * 0.5f, (next.Y - previous.Y) * 0.5f);
        }

        if (index == 0)
            return new PointF(_points[1].X - _points[0].X, _points[1].Y - _points[0].Y);

        if (index == count - 1)
            return new PointF(
                _points[count - 1].X - _points[count - 2].X,
                _points[count - 1].Y - _points[count - 2].Y);

        var prev = _points[index - 1];
        var nextPoint = _points[index + 1];
        return new PointF((nextPoint.X - prev.X) * 0.5f, (nextPoint.Y - prev.Y) * 0.5f);
    }


    private int FindNearest(PointF point, float radius)
    {
        var radiusSquared = radius * radius;
        var nearestIndex = -1;
        var minDistanceSquared = radiusSquared;

        for (var i = 0; i < _points.Count; i++)
        {
            var dx = _points[i].X - point.X;
            var dy = _points[i].Y - point.Y;
            var distanceSquared = dx * dx + dy * dy;

            if (distanceSquared < minDistanceSquared)
            {
                minDistanceSquared = distanceSquared;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }

}
