namespace Task1;

public sealed class ConvexHull
{
    private readonly List<Point> _points = [];

    public IReadOnlyList<Point> Points => _points;

    public void Add(Point point)
    {
        if (!_points.Contains(point))
            _points.Add(point);
    }

    public void Clear() => _points.Clear();

    public Point[] Build()
    {
        Point[] points = _points.Distinct().ToArray();
        
        if (points.Length <= 2)
            return points;

        Point left = points.MinBy(point => (point.X, point.Y));
        Point right = points.MaxBy(point => (point.X, point.Y));

        var upper = new List<Point>();
        var lower = new List<Point>();

        foreach (Point point in points)
        {
            int side = Cross(left, right, point);
            if (side < 0)
                upper.Add(point);
            else if (side > 0)
                lower.Add(point);
        }

        var hull = new List<Point> { left };
        FindHull(left, right, upper, hull);
        hull.Add(right);
        FindHull(right, left, lower, hull);

        return hull.ToArray();
    }

    private static void FindHull(Point first, Point second, List<Point> candidates, List<Point> hull)
    {
        if (candidates.Count == 0)
            return;

        Point farthest = candidates.MaxBy(point => Math.Abs(Cross(first, second, point)));

        var firstSide = new List<Point>();
        var secondSide = new List<Point>();

        foreach (Point point in candidates)
        {
            if (point == farthest)
                continue;

            if (Cross(first, farthest, point) < 0)
                firstSide.Add(point);
            else if (Cross(farthest, second, point) < 0)
                secondSide.Add(point);
        }

        FindHull(first, farthest, firstSide, hull);
        hull.Add(farthest);
        FindHull(farthest, second, secondSide, hull);
    }

    private static int Cross(Point a, Point b, Point c) => (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
}
