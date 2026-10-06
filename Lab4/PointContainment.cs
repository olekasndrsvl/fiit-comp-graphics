namespace Lab4;

public sealed class PolygonContainmentService
{
    private const float Epsilon = 1e-3f;

    // Граничные точки (вершина, ребро) считаются принадлежащими полигону.
    public bool ContainsPoint(PolygonShape polygon, PointF point)
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

        // Точка на любом ребре (включая вырожденные нулевые рёбра) — внутри полигона.
        for (var i = 0; i < vertices.Count; i++)
        {
            if (IsOnSegment(vertices[i], vertices[(i + 1) % vertices.Count], point))
                return true;
        }

        // Метод трассировки луча (crossing number): луч вправо от точки.
        // Полуправило (a.Y > y) != (b.Y > y) корректно считает проходы ровно через вершины.
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

    /// <summary>
    /// Проверяет, лежит ли точка на отрезке (с допуском <see cref="Epsilon"/>).
    /// </summary>
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
