using System.Drawing.Drawing2D;

namespace Lab4;

public sealed class AffineTransformService
{
    public Matrix CreateTranslation(float dx, float dy)
    {
        // x' = x + dx;
        // y' = y + dy.
        // почему такая странна инициализация?
        // https://learn.microsoft.com/en-us/dotnet/api/system.drawing.drawing2d.matrix?view=windowsdesktop-10.0&viewFallbackFrom=net-8.0
        return new Matrix(1, 0, 0, 1, dx, dy);
    }

    public Matrix CreateRotation(float degrees, PointF pivot)
    {
        var radians = degrees * Math.PI / 180;
        var cos = (float)Math.Cos(radians);
        var sin = (float)Math.Sin(radians);

        using var rotation = new Matrix(cos, sin, -sin, cos, 0, 0);
        using var back = CreateTranslation(pivot.X, pivot.Y);
        
        var result = CreateTranslation(-pivot.X, -pivot.Y);
        result.Multiply(rotation, MatrixOrder.Append);
        result.Multiply(back, MatrixOrder.Append);

        return result;
    }

    public Matrix CreateScaling(float sx, float sy, PointF pivot)
    {
        using var scaling = new Matrix(sx, 0, 0, sy, 0, 0);
        using var back = CreateTranslation(pivot.X, pivot.Y);

        var result = CreateTranslation(-pivot.X, -pivot.Y);
        result.Multiply(scaling, MatrixOrder.Append);
        result.Multiply(back, MatrixOrder.Append);

        return result;
    }

    // Центр — среднее координат вершин
    public PointF GetCenter(PolygonShape polygon)
    {
        if (polygon.Vertices.Count == 0)
            throw new ArgumentException("У пустого полигона нет центра.", nameof(polygon));

        double x = 0, y = 0;
        foreach (var point in polygon.Vertices)
        {
            x += point.X;
            y += point.Y;
        }
        return new PointF((float)(x / polygon.Vertices.Count), (float)(y / polygon.Vertices.Count));
    }

    public void Apply(PolygonShape polygon, Matrix transform)
    {
        if (polygon.Vertices.Count == 0) 
            return;

        var points = polygon.Vertices.ToArray();
        transform.TransformPoints(points);
        if (points.Any(point => !float.IsFinite(point.X) || !float.IsFinite(point.Y)))
            throw new ArgumentException("Слишком большие параметры преобразования.");

        // тут переприсвоим, а в форме оно уж само перерисуется
        for (var i = 0; i < points.Length; i++)
            polygon.Vertices[i] = points[i];
    }
}
