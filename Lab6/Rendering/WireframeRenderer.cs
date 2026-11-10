
using System.Drawing;
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

    private readonly ZBuffer zBuffer = new();

    public Camera Camera { get; } = new();

    private readonly record struct ClipPoint(double X, double Y, double Z, double W);

    public readonly record struct ScreenFrame(double Scale, double CenterX, double CenterY)
    {
        public static ScreenFrame Empty => new(0, 0, 0);
    }

    private (float Min, float Max) CalculateDepthRange(
        IReadOnlyList<SceneObject> items,
        Matrix<double> view,
        Matrix<double> projection)
    {
        float minDepth = float.PositiveInfinity;
        float maxDepth = float.NegativeInfinity;

        foreach (var item in items)
        {
            var transform = item.Model * view * projection;

            foreach (var point in item.Geometry.Positions)
            {
                var clipPoint = Transform(point, transform);
                float depth = (float)clipPoint.Z;

                minDepth = Math.Min(minDepth, depth);
                maxDepth = Math.Max(maxDepth, depth);
            }
        }

        if (float.IsPositiveInfinity(minDepth))
            return (0, 1);

        return (minDepth, maxDepth);
    }

    public List<(PointF Start, PointF End)> Project(
        SceneObject item,
        Size viewport,
        Matrix<double> view,
        Matrix<double> projection,
        bool perspective,
        ScreenFrame frame)
    {
        var transform = item.Model * view * projection;

        var vertices = item.Geometry.Positions
            .Select(point => Transform(point, transform))
            .ToArray();

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
        IReadOnlyList<SceneObject> items,
        Size viewport,
        Matrix<double> viewProjection)
    {
        var points = items
            .SelectMany(item => item.Geometry.Positions.Select(point =>
                Transform(point, item.Model * viewProjection)))
            .ToArray();

        if (points.Length == 0)
            return ScreenFrame.Empty;

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

        return new ScreenFrame(
            scale,
            (minX + maxX) / 2,
            (minY + maxY) / 2);
    }

    private static IReadOnlyList<Point3D> WorldPositions(SceneObject item) =>
        item.Geometry.Positions
            .Select(point => TransformMatrix.Transform(item.Model, point))
            .ToArray();

    private static ClipPoint Transform(Point3D point, Matrix<double> matrix)
    {
        var position = Vector<double>.Build.DenseOfArray(
            [point.X, point.Y, point.Z, 1]);

        var result = position * matrix;

        return new ClipPoint(
            result[0],
            result[1],
            result[2],
            result[3]);
    }

    private static PointF ToScreen(
        ClipPoint point,
        Size viewport,
        bool perspective,
        ScreenFrame frame)
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

    private static ScreenPoint ToScreenPoint(
        ClipPoint point,
        Size viewport,
        bool perspective,
        ScreenFrame frame)
    {
        var x = point.X / point.W;
        var y = point.Y / point.W;

        var screenX = perspective
            ? viewport.Width / 2d + x * viewport.Width / 2d
            : viewport.Width / 2d + (x - frame.CenterX) * frame.Scale;

        var screenY = perspective
            ? viewport.Height / 2d - y * viewport.Height / 2d
            : viewport.Height / 2d - (y - frame.CenterY) * frame.Scale;

        var depth = perspective
            ? point.Z / point.W
            : point.Z;

        return new ScreenPoint(
            (float)screenX,
            (float)screenY,
            (float)depth);
    }

    private static void DrawEdges(
        Graphics graphics,
        List<(PointF Start, PointF End)> segments,
        Pen pen)
    {
        foreach (var (start, end) in segments)
            graphics.DrawLine(pen, start, end);
    }

    private static void DrawAxes(
        Graphics graphics,
        Size viewport,
        Matrix<double> view,
        Matrix<double> projection,
        bool perspective)
    {
        var transform = view * projection;

        var frame = perspective
            ? ScreenFrame.Empty
            : new ScreenFrame(AxisLengthInPixels / AxesGeometry.Length, 0, 0);

        foreach (var (pa, pb, color) in AxesGeometry.Segments)
        {
            var a = Transform(pa, transform);
            var b = Transform(pb, transform);

            using var pen = new Pen(color, 2f);

            graphics.DrawLine(
                pen,
                ToScreen(a, viewport, perspective, frame),
                ToScreen(b, viewport, perspective, frame));
        }
    }

    private void RasterizeFaces(
        SceneObject item,
        Matrix<double> transform,
        Size viewport,
        bool perspective,
        ScreenFrame frame)
    {
        foreach (var face in item.Geometry.Faces)
        {
            var vertices = face.Vertices
                .Select(vertex =>
                {
                    var point = item.Geometry.Positions[vertex.PositionIndex];
                    var clipPoint = Transform(point, transform);

                    return ToScreenPoint(
                        clipPoint,
                        viewport,
                        perspective,
                        frame);
                })
                .ToList();

            zBuffer.RasterizePolygon(vertices, viewport, perspective);
        }
    }

    public void Render(
        Graphics graphics,
        Size viewport,
        IReadOnlyList<SceneObject> items,
        SceneObject? selectedItem,
        bool perspective,
        bool showAxes = false,
        bool zBufferEnabled = false)
    {
        if (items.Count == 0 || viewport.Width <= 0 || viewport.Height <= 0)
            return;

        var aspect = (double)viewport.Width / viewport.Height;

        var cameraTarget = selectedItem ?? items[0];

        if (perspective && !ReferenceEquals(framedItem, cameraTarget))
        {
            Camera.FitTo(WorldPositions(cameraTarget), aspect);
            framedItem = cameraTarget;
        }

        var view = perspective
            ? Camera.GetViewMatrix()
            : TransformMatrix.Identity;

        var projection = perspective
            ? Projections.Perspective(aspect)
            : Projections.Axonometric();

        var frame = perspective
            ? ScreenFrame.Empty
            : GetAxonometricFrame(items, viewport, view * projection);

        if (zBufferEnabled)
        {
            var depthRange = perspective
                ? (Min: 0f, Max: 1f)
                : CalculateDepthRange(items, view, projection);

            zBuffer.Initialize(
                viewport,
                perspective,
                depthRange.Min,
                depthRange.Max);
        }

        using var pen = new Pen(Color.Black);

        foreach (var item in items)
        {
            if (zBufferEnabled)
            {
                var transform = item.Model * view * projection;

                RasterizeFaces(
                    item,
                    transform,
                    viewport,
                    perspective,
                    frame);
            }
            else
            {
                var segments = Project(
                    item,
                    viewport,
                    view,
                    projection,
                    perspective,
                    frame);

                DrawEdges(graphics, segments, pen);
            }
        }

        if (zBufferEnabled)
            zBuffer.Draw(graphics, viewport);

        if (showAxes)
            DrawAxes(graphics, viewport, view, projection, perspective);
    }
}
