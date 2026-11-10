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

    private float minDepth;
    private float maxDepth;
    private float[,]? depthBuffer;
    private Color[,]? colorBuffer;

    private readonly struct ScreenPoint
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public ScreenPoint(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    private void InitializeBuffers(Size viewport)
    {
        depthBuffer = new float[viewport.Width, viewport.Height];
        colorBuffer = new Color[viewport.Width, viewport.Height];

        for (int x = 0; x < viewport.Width; x++)
        {
            for (int y = 0; y < viewport.Height; y++)
            {
                depthBuffer[x, y] = float.PositiveInfinity;
                colorBuffer[x, y] = Color.White;
            }
        }
    }

    private void CalculateDepthRange(IReadOnlyList<SceneObject> items, Matrix<double> view, Matrix<double> projection)
    {
        minDepth = float.PositiveInfinity;
        maxDepth = float.NegativeInfinity;

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
        {
            minDepth = 0;
            maxDepth = 1;
        }
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
        var vertices = item.Geometry.Positions.Select(point => Transform(point, transform)).ToArray();
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

    private static ScreenPoint ToScreenPoint(ClipPoint point, Size viewport, bool perspective, ScreenFrame frame)
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

        return new ScreenPoint((float)screenX, (float)screenY, (float)depth);
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

    private Color GetDepthColor(float depth, bool perspective)
    {
        float normalizedDepth;

        if (perspective)
        {
            normalizedDepth = depth;
        }
        else
        {
            float range = maxDepth - minDepth;

            normalizedDepth = range > 1e-6f
                ? (depth - minDepth) / range
                : 0.5f;
        }

        float intensity = 1f - Math.Clamp(normalizedDepth, 0f, 1f);

        int channel = (int)(25 + intensity * 230);

        return Color.FromArgb(channel, channel, channel);
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
        if (items.Count == 0)
            return;

        if (zBufferEnabled)
        {
            InitializeBuffers(viewport);
        }

        var aspect = (double)viewport.Width / viewport.Height;
        var cameraTarget = selectedItem ?? items[0];
        if (perspective && !ReferenceEquals(framedItem, cameraTarget))
        {
            Camera.FitTo(WorldPositions(cameraTarget), aspect);
            framedItem = cameraTarget;
        }

        var view = perspective ? Camera.GetViewMatrix() : TransformMatrix.Identity;
        var projection = perspective ? Projections.Perspective(aspect) : Projections.Axonometric();
        var frame = perspective
            ? ScreenFrame.Empty
            : GetAxonometricFrame(items, viewport, view * projection);
        
        if (zBufferEnabled && !perspective)
        {
            CalculateDepthRange(items, view, projection);
        }

        using var pen = new Pen(Color.Black);
        foreach (var item in items)
        {
            if (zBufferEnabled)
            {
                var transform = item.Model * view * projection;
                RasterizeFaces(item, transform, viewport, perspective, frame);
            }
            else
            {
                var segments = Project(item, viewport, view, projection, perspective, frame);
                DrawEdges(graphics, segments, pen);
            }
        }

        if (zBufferEnabled)
        {
            DrawColorBuffer(graphics, viewport);
        }

        if (showAxes)
            DrawAxes(graphics, viewport, view, projection, perspective);
    }

    private readonly record struct ClipPoint(double X, double Y, double Z, double W);

    public readonly record struct ScreenFrame(double Scale, double CenterX, double CenterY)
    {
        public static ScreenFrame Empty => new(0, 0, 0);
    }

    private void RasterizeTriangle(ScreenPoint a, ScreenPoint b, ScreenPoint c, Color color, Size viewport, bool perspective)
    {
        float area =
            (b.X - a.X) * (c.Y - a.Y) -
            (b.Y - a.Y) * (c.X - a.X);

        if (Math.Abs(area) < 1e-6f)
            return;

        int minX = Math.Max(0, (int)Math.Floor(Math.Min(a.X, Math.Min(b.X, c.X))));
        int maxX = Math.Min(viewport.Width - 1, (int)Math.Ceiling(Math.Max(a.X, Math.Max(b.X, c.X))));

        int minY = Math.Max(0, (int)Math.Floor(Math.Min(a.Y, Math.Min(b.Y, c.Y))));
        int maxY = Math.Min(viewport.Height - 1, (int)Math.Ceiling(Math.Max(a.Y, Math.Max(b.Y, c.Y))));

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                float px = x + 0.5f;
                float py = y + 0.5f;

                float w1 =
                    ((b.X - px) * (c.Y - py) -
                     (b.Y - py) * (c.X - px)) / area;

                float w2 =
                    ((c.X - px) * (a.Y - py) -
                     (c.Y - py) * (a.X - px)) / area;

                float w3 = 1f - w1 - w2;

                if (w1 < 0 || w2 < 0 || w3 < 0)
                    continue;

                float z = w1 * a.Z + w2 * b.Z + w3 * c.Z;

                if (z < depthBuffer![x, y])
                {
                    depthBuffer[x, y] = z;
                    colorBuffer![x, y] = GetDepthColor(z, perspective);
                }
            }
        }
    }

    private void RasterizePolygon(IReadOnlyList<ScreenPoint> vertices, Color color, Size viewport, bool perspective)
    {
        if (vertices.Count < 3)
            return;

        for (int i = 1; i < vertices.Count - 1; i++)
        {
            RasterizeTriangle(
                vertices[0],
                vertices[i],
                vertices[i + 1],
                color,
                viewport,
                perspective);
        }   
    }

    private void RasterizeFaces(SceneObject item, Matrix<double> transform, Size viewport, bool perspective, ScreenFrame frame)
    {
        foreach (var face in item.Geometry.Faces)
        {
            var vertices = face.Vertices
                .Select(vertex =>
                {
                    var point = item.Geometry.Positions[vertex.PositionIndex];
                    var clipPoint = Transform(point, transform);

                    return ToScreenPoint(clipPoint, viewport, perspective, frame);
                })
                .ToList();

            RasterizePolygon(vertices, Color.LightGray, viewport, perspective);
        }
    }

    private void DrawColorBuffer(Graphics graphics, Size viewport)
    {
        using var bitmap = new Bitmap(viewport.Width, viewport.Height);

        for (int x = 0; x < viewport.Width; x++)
        {
            for (int y = 0; y < viewport.Height; y++)
            {
                bitmap.SetPixel(x, y, colorBuffer![x, y]);
            }
        }

        graphics.DrawImageUnscaled(bitmap, 0, 0);
    }
}
