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
    private double viewYaw;
    private double viewPitch;
    private double objectRotationAngle;

    public Camera Camera { get; } = new();
    public bool BackfaceCulling { get; set; }

    public double ObjectRotationAngle
    {
        get => objectRotationAngle;
        set => objectRotationAngle = value;
    }

    public void RotateView(double deltaYaw, double deltaPitch)
    {
        viewYaw += deltaYaw;
        viewPitch = Math.Clamp(viewPitch + deltaPitch,
            -Math.PI / 2 + 0.01, Math.PI / 2 - 0.01);
    }

    public void ResetViewRotation()
    {
        viewYaw = 0;
        viewPitch = 0;
    }

    private Matrix<double> BuildViewRotation()
    {
        if (Math.Abs(viewYaw) < 1e-9 && Math.Abs(viewPitch) < 1e-9)
            return TransformMatrix.Identity;
        return TransformMatrix.Rotation(0, 1, 0, viewYaw)
             * TransformMatrix.Rotation(1, 0, 0, viewPitch);
    }

    private Matrix<double> GetObjectRotation(SceneObject item)
    {
        if (Math.Abs(objectRotationAngle) < 1e-9)
            return TransformMatrix.Identity;
        var rotation = TransformMatrix.Rotation(0, 1, 0, objectRotationAngle);
        try { return TransformMatrix.Around(item.Center, rotation); }
        catch (NotImplementedException) { return rotation; }
    }

    public List<(PointF Start, PointF End)> Project(
        SceneObject item,
        Size viewport,
        Matrix<double> view,
        Matrix<double> projection,
        bool perspective,
        ScreenFrame frame)
    {
        var objectRotation = GetObjectRotation(item);
        var modelView = item.Model * objectRotation * view;
        var transform = modelView * projection;

        var viewPositions = item.Geometry.Positions
            .Select(p => TransformToPoint3D(p, modelView))
            .ToArray();

        var clipVertices = item.Geometry.Positions
            .Select(point => Transform(point, transform))
            .ToArray();

        HashSet<(int, int)>? visibleEdges = null;
        if (BackfaceCulling)
        {
            visibleEdges = BackfaceCuller.ComputeVisibleEdges(item, viewPositions, perspective);
        }

        var segments = new List<(PointF Start, PointF End)>();
        foreach (var (start, end) in item.Geometry.Edges)
        {
            if (visibleEdges is not null)
            {
                var key = start < end ? (start, end) : (end, start);
                if (!visibleEdges.Contains(key)) continue;
            }

            segments.Add((
                ToScreen(clipVertices[start], viewport, perspective, frame),
                ToScreen(clipVertices[end], viewport, perspective, frame)));
        }

        return segments;
    }

    private static Point3D TransformToPoint3D(Point3D point, Matrix<double> matrix)
    {
        var position = Vector<double>.Build.DenseOfArray([point.X, point.Y, point.Z, 1]);
        var result = position * matrix;
        return new Point3D(result[0], result[1], result[2]);
    }

    private ScreenFrame GetAxonometricFrame(
        IReadOnlyList<SceneObject> items,
        Size viewport,
        Matrix<double> viewProjection)
    {
        var points = items
            .SelectMany(item =>
            {
                var rotation = GetObjectRotation(item);
                return item.Geometry.Positions.Select(point =>
                    Transform(point, item.Model * rotation * viewProjection));
            })
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

        if (width > 0) scale = Math.Min(scale, availableWidth / width);
        if (height > 0) scale = Math.Min(scale, availableHeight / height);

        return new ScreenFrame(scale, (minX + maxX) / 2, (minY + maxY) / 2);
    }

    private static IReadOnlyList<Point3D> WorldPositions(SceneObject item) =>
        item.Geometry.Positions
            .Select(point => TransformMatrix.Transform(item.Model, point))
            .ToArray();

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

    private static void DrawEdges(Graphics graphics, List<(PointF Start, PointF End)> segments, Pen pen)
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

    public void Render(
        Graphics graphics,
        Size viewport,
        IReadOnlyList<SceneObject> items,
        SceneObject? selectedItem,
        bool perspective,
        bool showAxes = false)
    {
        if (items.Count == 0)
            return;

        var aspect = (double)viewport.Width / viewport.Height;
        var cameraTarget = selectedItem ?? items[0];
        if (perspective && !ReferenceEquals(framedItem, cameraTarget))
        {
            Camera.FitTo(WorldPositions(cameraTarget), aspect);
            framedItem = cameraTarget;
        }

        var rotation = BuildViewRotation();
        var view = perspective ? rotation * Camera.GetViewMatrix() : rotation;
        var projection = perspective ? Projections.Perspective(aspect) : Projections.Axonometric();
        var frame = perspective
            ? ScreenFrame.Empty
            : GetAxonometricFrame(items, viewport, view * projection);

        if (showAxes)
            DrawAxes(graphics, viewport, view, projection, perspective);

        using var pen = new Pen(Color.Black);
        foreach (var item in items)
        {
            var segments = Project(item, viewport, view, projection, perspective, frame);
            DrawEdges(graphics, segments, pen);
        }
    }

    private readonly record struct ClipPoint(double X, double Y, double Z, double W);

    public readonly record struct ScreenFrame(double Scale, double CenterX, double CenterY)
    {
        public static ScreenFrame Empty => new(0, 0, 0);
    }
}