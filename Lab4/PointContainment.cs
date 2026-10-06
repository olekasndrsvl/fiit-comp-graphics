namespace Lab4;

// Алгоритм проверки принадлежности точки полигону.
public enum ContainmentAlgorithm
{
    // Метод трассировки луча (crossing number).
    RayCasting,

    // Метод суммы ориентированных углов.
    AngleSum
}

public sealed class PolygonContainmentService
{
    private const float Epsilon = 1e-3f;

    // Граничные точки (вершина, ребро) считаются принадлежащими полигону.
    public bool ContainsPoint(PolygonShape polygon, PointF point) =>
        ContainsPoint(polygon, point, ContainmentAlgorithm.RayCasting);

    // Граничные точки (вершина, ребро) считаются принадлежащими полигону.
    public bool ContainsPoint(PolygonShape polygon, PointF point, ContainmentAlgorithm algorithm)
    {
        var vertices = polygon.Vertices;

        switch (vertices.Count)
        {
            case 0:
                return false;
            case 1:
                return Distance(point, vertices[0]) <= Epsilon;
            case 2:
                return IsOnSegment(vertices[0], vertices[1], point);
        }

        // Граница полигона проверяется до алгоритмов — общая часть для обоих.
        for (var i = 0; i < vertices.Count; i++)
        {
            if (IsOnSegment(vertices[i], vertices[(i + 1) % vertices.Count], point))
                return true;
        }

        return algorithm switch
        {
            ContainmentAlgorithm.AngleSum => IsInsideByAngles(vertices, point),
            _ => IsInsideByRayCasting(vertices, point)
        };
    }

    // Луч вправо от точки; полуправило (a.Y > y) != (b.Y > y) считает проходы через вершины один раз.
    private static bool IsInsideByRayCasting(List<PointF> vertices, PointF point)
    {
        var inside = false;
        for (int i = 0, j = vertices.Count - 1; i < vertices.Count; j = i++)
        {
            var a = vertices[i];
            var b = vertices[j];

            if ((a.Y > point.Y) == (b.Y > point.Y))
                continue;

            var t = (point.Y - a.Y) / (b.Y - a.Y);
            if (point.X < a.X + t * (b.X - a.X))
                inside = !inside;
        }

        return inside;
    }

    // Сумма ориентированных углов, под которыми полигон виден из точки:
    // ±2π внутри, 0 снаружи (вне зависимости от выпуклости).
    private static bool IsInsideByAngles(List<PointF> vertices, PointF point)
    {
        double sum = 0;
        for (var i = 0; i < vertices.Count; i++)
        {
            var a = vertices[i];
            var b = vertices[(i + 1) % vertices.Count];

            // Векторы от проверяемой точки к концам ребра.
            var ax = a.X - point.X;
            var ay = a.Y - point.Y;
            var bx = b.X - point.X;
            var by = b.Y - point.Y;

            // Ориентированный угол через atan2(векторное произведение, скалярное).
            sum += Math.Atan2(ax * by - ay * bx, ax * bx + ay * by);
        }

        return Math.Abs(sum) > Math.PI;
    }

    private static bool IsOnSegment(PointF a, PointF b, PointF point)
    {
        var abX = b.X - a.X;
        var abY = b.Y - a.Y;
        var lengthSquared = abX * abX + abY * abY;

        if (lengthSquared <= Epsilon * Epsilon)
            return Distance(point, a) <= Epsilon;

        var t = ((point.X - a.X) * abX + (point.Y - a.Y) * abY) / lengthSquared;
        t = Math.Clamp(t, 0f, 1f);

        var dx = point.X - (a.X + t * abX);
        var dy = point.Y - (a.Y + t * abY);

        return dx * dx + dy * dy <= Epsilon * Epsilon;
    }

    private static float Distance(PointF a, PointF b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }
}
