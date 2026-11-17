using System.Drawing;
using Lab6.Core.Geometry;
using Lab6.Core.Lighting;                                      
using Lab6.Core.Mathematics;
using Lab6.Core.Scene;
using MathNet.Numerics.LinearAlgebra;

namespace Lab6.Rendering;

public sealed class WireframeRenderer
{
    private const double DefaultVisibleRange = 4;
    private const double ViewportPadding = 20;
    private const double AxisLengthInPixels = 50;
    private const double MinimumProjectionW = 1e-6;

    private SceneObject? framedItem;

    private readonly SoftwareRasterizer rasterizer = new();
    private readonly UnlitTextureShader textureShader = new();
    private readonly GouraudFragmentShader gouraudShader = new();  
    private readonly PhongFragmentShader phongShader = new();      
    private double viewYaw;
    private double viewPitch;
    private double objectRotationAngle;

    public Camera Camera { get; } = new();

    private readonly record struct ClipPoint(double X, double Y, double Z, double W);

    public readonly record struct ScreenFrame(double Scale, double CenterX, double CenterY)
    {
        public static ScreenFrame Empty => new(0, 0, 0);
    }

    private (float Min, float Max) CalculateDepthRange(
    IReadOnlyList<SceneObject> items,
    Matrix<double> view,
    Matrix<double> projection,
    bool perspective)
    {
        float minDepth = float.PositiveInfinity;
        float maxDepth = float.NegativeInfinity;

        foreach (var item in items)
        {
            var transform = item.Model * GetObjectRotation(item) * view * projection;

            foreach (var point in item.Geometry.Positions)
            {
                var clipPoint = Transform(point, transform);

                if (Math.Abs(clipPoint.W) < MinimumProjectionW)
                    continue;

                float depth = perspective
                    ? (float)(clipPoint.Z / clipPoint.W)
                    : (float)clipPoint.Z;

                minDepth = Math.Min(minDepth, depth);
                maxDepth = Math.Max(maxDepth, depth);
            }
        }

        if (float.IsPositiveInfinity(minDepth))
            return (0, 1);

        return (minDepth, maxDepth);
    }
    public bool BackfaceCulling { get; set; }

    public PointF? LightScreenPosition { get; private set; }      

    public double WorldPerPixel { get; private set; } = 0.01;     

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
            var viewDirection = perspective
                ? new Normal(0, 0, 1)
                : new Normal(projection[0, 2], projection[1, 2], projection[2, 2]);

            visibleEdges = BackfaceCuller.ComputeVisibleEdges(
                item,
                viewPositions,
                perspective,
                viewDirection,
                new Point3D(0, 0, 0));
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

    private static bool TryToRasterVertex(                   
        ClipPoint point,
        Size viewport,
        bool perspective,
        ScreenFrame frame,
        TextureCoordinate? textureCoordinate,
        VertexColor? color,                                 
        WorldVertex? worldVertex,                               
        out RasterVertex vertex)
    {
        vertex = default;

        if (perspective && point.W <= MinimumProjectionW)
            return false;

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

        if (!double.IsFinite(screenX) || !double.IsFinite(screenY) || !double.IsFinite(depth))
            return false;

        var inverseW = 1 / point.W;

        var attributes = VertexAttributes.None;                

        double u = 0, v = 0;                                    
        if (textureCoordinate is { } texture)                     
        {
            attributes |= VertexAttributes.Texture;
            u = texture.U;
            v = texture.V ?? 0;
        }

        double red = 0, green = 0, blue = 0;                       
        if (color is { } vertexColor)
        {
            attributes |= VertexAttributes.Color;
            red = vertexColor.R;
            green = vertexColor.G;
            blue = vertexColor.B;
        }

        double worldX = 0, worldY = 0, worldZ = 0;                
        double normalX = 0, normalY = 0, normalZ = 0;
        if (worldVertex is { } world)
        {
            attributes |= VertexAttributes.WorldSpace;
            worldX = world.Position.X;
            worldY = world.Position.Y;
            worldZ = world.Position.Z;
            normalX = world.WorldNormal.X;
            normalY = world.WorldNormal.Y;
            normalZ = world.WorldNormal.Z;
        }

        vertex = new RasterVertex(
            (float)screenX,
            (float)screenY,
            (float)depth,
            inverseW,
            u * inverseW,
            v * inverseW,
            red * inverseW,                                  
            green * inverseW,
            blue * inverseW,
            worldX * inverseW,
            worldY * inverseW,
            worldZ * inverseW,
            normalX * inverseW,
            normalY * inverseW,
            normalZ * inverseW,
            attributes);                                         

        return true;
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

    private readonly record struct VertexStageResult(
        VertexColor[]? Colors,
        WorldVertex[]? WorldVertices)
    {
        public static VertexStageResult None => new(null, null);
    }

    private static VertexStageResult RunVertexStage(
        SceneObject item,
        Matrix<double> world,
        ShadingMode shadingMode,
        Light? light)
    {
        if (light is null || shadingMode == ShadingMode.None)
            return VertexStageResult.None;

        var worldVertices = VertexTransform.Transform(item, world);

        if (shadingMode == ShadingMode.Phong)
            return new VertexStageResult(null, worldVertices);

        var baseColor = VertexColor.FromColor(item.Material.Color);
        var colors = new VertexColor[worldVertices.Length];

        for (var i = 0; i < colors.Length; i++)
        {
            var vertex = worldVertices[i];
            colors[i] = LightingModel.ShadeLambert(
                baseColor, vertex.WorldNormal, vertex.Position, light);
        }

        return new VertexStageResult(colors, null);
    }

    private static double Distance(Point3D a, Point3D b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        var dz = a.Z - b.Z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    private static T? ValueAt<T>(T[]? values, int index) where T : struct =>
        values is not null && index >= 0 && index < values.Length ? values[index] : null;

    private void RasterizeFaces(
        SceneObject item,
        Matrix<double> modelView,
        Matrix<double> transform,
        Size viewport,
        bool perspective,
        Normal viewDirection,
        ScreenFrame frame,
        IFragmentShader shader,
        VertexStageResult stage)
    {
        var viewPositions = BackfaceCulling
            ? item.Geometry.Positions
                .Select(point => TransformToPoint3D(point, modelView))
                .ToArray()
            : null;
        var cameraInViewSpace = new Point3D(0, 0, 0);

        foreach (var face in item.Geometry.Faces)
        {
            if (viewPositions is not null &&
                !BackfaceCuller.IsFaceVisible(
                    item.Geometry, face, viewPositions, perspective, viewDirection, cameraInViewSpace))
            {
                continue;
            }

            var vertices = new List<RasterVertex>(face.Vertices.Count);
            var isVisible = true;

            foreach (var faceVertex in face.Vertices)
            {
                var index = faceVertex.PositionIndex;
                var point = item.Geometry.Positions[index];
                var clipPoint = Transform(point, transform);
                TextureCoordinate? textureCoordinate = faceVertex.TextureCoordinateIndex is int textureIndex
                    ? item.Geometry.TextureCoordinates[textureIndex]
                    : null;

                if (!TryToRasterVertex(
                        clipPoint,
                        viewport,
                        perspective,
                        frame,
                        textureCoordinate,
                        ValueAt(stage.Colors, index),
                        ValueAt(stage.WorldVertices, index),
                        out var vertex))
                {
                    isVisible = false;
                    break;
                }

                vertices.Add(vertex);
            }

            if (isVisible)
                rasterizer.RasterizePolygon(vertices, item.Material, shader);
        }
    }

    public void Render(Graphics graphics, Size viewport, IReadOnlyList<SceneObject> items, SceneObject? selectedItem, bool perspective,
        bool showAxes = false, bool zBufferEnabled = false, ShadingMode shadingMode = ShadingMode.None, Light? light = null)
    {
        if (items.Count == 0 || viewport.Width <= 0 || viewport.Height <= 0)
            return;

        var lighting = shadingMode != ShadingMode.None && light is not null;
        var aspect = (double)viewport.Width / viewport.Height;

        var cameraTarget = selectedItem ?? items[0];

        if (perspective && !ReferenceEquals(framedItem, cameraTarget))
        {
            Camera.FitTo(WorldPositions(cameraTarget), aspect);
            framedItem = cameraTarget;
        }

        var view = Camera.GetViewMatrix();
        var rotation = BuildViewRotation();
        var projection = perspective ? Projections.Perspective(aspect) : Projections.Axonometric();
        var frame = perspective
            ? ScreenFrame.Empty
            : GetAxonometricFrame(items, viewport, view * projection);
        if (!perspective)
            frame = frame with { Scale = frame.Scale * Camera.OrthographicZoom };


        // Масштаб для перетаскивания маркера света: сколько мировых единиц в пикселе.
        WorldPerPixel = perspective                             
            ? 2 * Math.Tan(Camera.FieldOfView / 2) * Distance(Camera.Position, Camera.Target) /
              Math.Max(1, viewport.Height)
            : 1 / Math.Max(frame.Scale, 1e-6);

        var filledRendering = zBufferEnabled || lighting ||      
                              items.Any(item => item.Material.Texture is not null);
        var depthRange = (Min: 0f, Max: 1f);

        if (zBufferEnabled)
        {
            depthRange = CalculateDepthRange(
                items,
                view,
                projection,
                perspective);
        }

        if (filledRendering)
            rasterizer.Initialize(viewport, zBufferEnabled);

        var viewDirection = perspective                            
            ? new Normal(0, 0, 1)
            : new Normal(projection[0, 2], projection[1, 2], projection[2, 2]);

        LightScreenPosition = light is not null &&                 
                              LightMarker.TryGetScreenPosition(
                                  viewport, view, projection, perspective, frame, light.Position,
                                  out var lightScreen, out _)
            ? lightScreen
            : null;

        using var pen = new Pen(Color.Black);

        foreach (var item in items)
        {
            if (!filledRendering)                              
            {
                var segments = Project(
                    item,
                    viewport,
                    view,
                    projection,
                    perspective,
                    frame);

                DrawEdges(graphics, segments, pen);
                continue;
            }

            var world = item.Model * GetObjectRotation(item);        
            var modelView = world * view;
            var transform = modelView * projection;

            var stage = lighting                                     
                ? RunVertexStage(item, world, shadingMode, light)
                : VertexStageResult.None;

            // Точка расширения пункта 1.2 — здесь подключается фрагментный шейдер Фонга.
            IFragmentShader shader = shadingMode switch           
            {
                ShadingMode.Gouraud when stage.Colors is not null => gouraudShader,
                ShadingMode.Phong => phongShader,
                _ => item.Material.Texture is not null
                    ? textureShader
                    : zBufferEnabled
                        ? new DepthShader(depthRange.Min, depthRange.Max)
                        : textureShader
            };

            RasterizeFaces(item, modelView, transform, viewport, perspective, viewDirection, frame, shader, stage);
        }

        if (filledRendering)
            rasterizer.Draw(graphics);

        if (showAxes)
            DrawAxes(graphics, viewport, view, projection, perspective);

        if (light is not null)                                   
        {
            LightMarker.Draw(
                graphics,
                viewport,
                view,
                projection,
                perspective,
                frame,
                light.Position,
                light.Color);
        }
    }
}
