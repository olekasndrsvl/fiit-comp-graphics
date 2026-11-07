using Lab6.Core.Geometry;
using Lab6.Core.Mathematics;
using Lab6.Core.Scene;
using MathNet.Numerics.LinearAlgebra;

namespace Lab6.Rendering;

public sealed class WireframeRenderer
{
    private const double DefaultVisibleRange = 4;
    private const double ViewportPadding = 20;
    private const double AxisLengthInPixels = 50;
    private SceneObject? framedItem;

    public Camera Camera { get; } = new();

    public List<(PointF Start, PointF End)> Project(
        SceneObject item,
        Size viewport,
        Matrix<double> view,
        Matrix<double> projection,
        bool perspective)
    {
        var transform = item.Model * view * projection;
        var vertices = item.Geometry.Positions.Select(point => Transform(point, transform)).ToArray();
        var frame = perspective
            ? ScreenFrame.Empty
            : GetAxonometricFrame(item.Geometry.Positions, viewport, view * projection);
        var segments = new List<(PointF Start, PointF End)>();

        foreach (var (start, end) in item.Geometry.Edges)
        {
            var a = vertices[start];
            var b = vertices[end];
            segments.Add((
                ToScreen(a, viewport, perspective, frame),
                ToScreen(b, viewport, perspective, frame)));
        }

        return segments;
    }

    private static ScreenFrame GetAxonometricFrame(
        IReadOnlyList<Point3D> positions,
        Size viewport,
        Matrix<double> transform)
    {
        if (positions.Count == 0)
            return ScreenFrame.Empty;

        var points = positions.Select(point => Transform(point, transform)).ToArray();
        var minX = points.Min(point => point.X);
        var maxX = points.Max(point => point.X);
        var minY = points.Min(point => point.Y);
        var maxY = points.Max(point => point.Y);
        var width = maxX - minX;
        var height = maxY - minY;

        var scale = Math.Min(viewport.Width, viewport.Height) / DefaultVisibleRange;
        var availableWidth = Math.Max(1, viewport.Width - 2 * ViewportPadding);
        var availableHeight = Math.Max(1, viewport.Height - 2 * ViewportPadding);

        if (width > 0)
            scale = Math.Min(scale, availableWidth / width);
        if (height > 0)
            scale = Math.Min(scale, availableHeight / height);

        return new ScreenFrame(scale, (minX + maxX) / 2, (minY + maxY) / 2);
    }

    private static ClipPoint Transform(Point3D point, Matrix<double> matrix)
    {
        var position = Vector<double>.Build.DenseOfArray([point.X, point.Y, point.Z, 1]);
        var result = position * matrix;
        return new ClipPoint(result[0], result[1], result[2], result[3]);
    }

    private static PointF ToScreen(ClipPoint point, Size viewport, bool perspective, ScreenFrame frame)
    {
        var x = point.X / point.W;
        var y = point.Y / point.W;
        var screenX = perspective
            ? viewport.Width / 2d + x * viewport.Width / 2d
            : viewport.Width / 2d + (x - frame.CenterX) * frame.Scale;
        var screenY = perspective
            ? viewport.Height / 2d - y * viewport.Height / 2d
            : viewport.Height / 2d - (y - frame.CenterY) * frame.Scale;
        return new PointF((float)screenX, (float)screenY);
    }

    public void DrawEdges(Graphics graphics, List<(PointF Start, PointF End)> segments, Pen pen)
    {
        foreach (var (start, end) in segments)
            graphics.DrawLine(pen, start, end);
    }

    public void DrawAxes(Graphics graphics, Size viewport, Matrix<double> view, Matrix<double> projection, bool perspective)
    {
        var transform = view * projection;
        var frame = perspective ? ScreenFrame.Empty : new ScreenFrame(AxisLengthInPixels / AxesGeometry.Length, 0, 0);

        foreach (var (pa, pb, color) in AxesGeometry.Segments)
        {
            var a = Transform(pa, transform);
            var b = Transform(pb, transform);

            using var pen = new Pen(color, 2f);
            graphics.DrawLine(pen, ToScreen(a, viewport, perspective, frame), ToScreen(b, viewport, perspective, frame));
        }
    }

    public void Render(Graphics graphics, Size viewport, SceneObject item, bool perspective, bool showAxes = false)
    {
        var aspect = (double)viewport.Width / viewport.Height;
        if (perspective && !ReferenceEquals(framedItem, item))
        {
            Camera.FitTo(item.Geometry.Positions, aspect);
            framedItem = item;
        }

        var view = perspective ? Camera.GetViewMatrix() : TransformMatrix.Identity;
        var projection = perspective ? Projections.Perspective(aspect) : Projections.Axonometric();

        if (showAxes)
            DrawAxes(graphics, viewport, view, projection, perspective);

        var segments = Project(item, viewport, view, projection, perspective);
        using var pen = new Pen(Color.Black);
        DrawEdges(graphics, segments, pen);
    }

    private readonly record struct ClipPoint(double X, double Y, double Z, double W);

    private readonly record struct ScreenFrame(double Scale, double CenterX, double CenterY)
    {
        public static ScreenFrame Empty => new(0, 0, 0);
    }
}
