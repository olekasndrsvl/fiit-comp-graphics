using Lab6.Core.Geometry;
using Lab6.Core.Scene;
using MathNet.Numerics.LinearAlgebra;

namespace Lab6.Rendering;

public sealed class WireframeRenderer
{
    public void Render(Graphics graphics, Size viewport, SceneObject item, bool perspective) // viewport - размеры экрана
    {
        var aspect = (double)viewport.Width / viewport.Height;
        var projection = perspective ? Projections.Perspective(aspect) : Projections.Axonometric();
        var segments = Project(item, viewport, projection, perspective);
        using var pen = new Pen(Color.Black);
        DrawEdges(graphics, segments, pen);
    }

    public List<(PointF Start, PointF End)> Project(SceneObject item, Size viewport, Matrix<double> projection, bool perspective)
    {
        var view = Matrix<double>.Build.DenseIdentity(4); // для развития

        if (perspective) // хардкод для аксонометрической
            view[3, 2] = 5; // Камера в точке (0, 0, -5) смотрит вдоль +Z.

        var transform = item.Model * view * projection;
        var vertices = item.Geometry.Positions.Select(point => Transform(point, transform)).ToArray();

        var scale = perspective ? 0d : Math.Min(viewport.Width, viewport.Height) / 4d; // хардкод для перспективной
        var segments = new List<(PointF Start, PointF End)>();

        foreach (var (start, end) in item.Geometry.Edges)
        {
            var a = vertices[start];
            var b = vertices[end];
            segments.Add((ToScreen(a), ToScreen(b)));
        }

        return segments;

        PointF ToScreen(ClipPoint point)
        {
            var x = point.X / point.W;
            var y = point.Y / point.W;
            var scaleX = perspective ? viewport.Width / 2d : scale;
            var scaleY = perspective ? viewport.Height / 2d : scale;
            return new PointF((float)(viewport.Width / 2d + x * scaleX), (float)(viewport.Height / 2d - y * scaleY));
        }
    }

    private static ClipPoint Transform(Point3D point, Matrix<double> matrix)
    {
        var position = Vector<double>.Build.DenseOfArray([point.X, point.Y, point.Z, 1]);
        var result = position * matrix;
        return new ClipPoint(result[0], result[1], result[2], result[3]);
    }

    private readonly record struct ClipPoint(double X, double Y, double Z, double W);

    public void DrawEdges(Graphics graphics, List<(PointF Start, PointF End)> segments, Pen pen)
    {
        foreach (var (start, end) in segments)
            graphics.DrawLine(pen, start, end);
    }
}
