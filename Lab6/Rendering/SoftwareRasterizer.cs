using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Lab6.Core.Materials;

namespace Lab6.Rendering;

internal readonly record struct RasterVertex(
    float X,
    float Y,
    float Z,
    double InverseW,
    double UOverW,
    double VOverW,
    bool HasTextureCoordinates);

public sealed class SoftwareRasterizer
{
    private const float MinimumTriangleArea = 1e-6f;
    private const double MinimumInterpolatedInverseW = 1e-12;

    private readonly ZBuffer depthBuffer = new();
    private int[] colorBuffer = [];
    private int width;
    private int height;
    private bool depthTestEnabled;

    public void Initialize(Size viewport, bool depthTestEnabled)
    {
        width = viewport.Width;
        height = viewport.Height;
        this.depthTestEnabled = depthTestEnabled;

        var bufferSize = width * height;
        if (colorBuffer.Length != bufferSize)
            colorBuffer = new int[bufferSize];

        Array.Fill(colorBuffer, Color.White.ToArgb());
        depthBuffer.Initialize(viewport);
    }

    internal void RasterizePolygon(
        IReadOnlyList<RasterVertex> vertices,
        Material material,
        IFragmentShader shader)
    {
        if (vertices.Count < 3)
            return;

        for (var i = 1; i < vertices.Count - 1; i++)
            RasterizeTriangle(vertices[0], vertices[i], vertices[i + 1], material, shader);
    }

    private void RasterizeTriangle(
        RasterVertex a,
        RasterVertex b,
        RasterVertex c,
        Material material,
        IFragmentShader shader)
    {
        var area =
            (b.X - a.X) * (c.Y - a.Y) -
            (b.Y - a.Y) * (c.X - a.X);

        if (Math.Abs(area) < MinimumTriangleArea)
            return;

        var minX = Math.Max(0, (int)Math.Floor(Math.Min(a.X, Math.Min(b.X, c.X))));
        var maxX = Math.Min(width - 1, (int)Math.Ceiling(Math.Max(a.X, Math.Max(b.X, c.X))));
        var minY = Math.Max(0, (int)Math.Floor(Math.Min(a.Y, Math.Min(b.Y, c.Y))));
        var maxY = Math.Min(height - 1, (int)Math.Ceiling(Math.Max(a.Y, Math.Max(b.Y, c.Y))));

        var inverseArea = 1f / area;
        var w1StepX = -(c.Y - b.Y) * inverseArea;
        var w2StepX = -(a.Y - c.Y) * inverseArea;
        var w3StepX = -(b.Y - a.Y) * inverseArea;
        var w1StepY = (c.X - b.X) * inverseArea;
        var w2StepY = (a.X - c.X) * inverseArea;
        var w3StepY = (b.X - a.X) * inverseArea;
        var hasTextureCoordinates =
            a.HasTextureCoordinates && b.HasTextureCoordinates && c.HasTextureCoordinates;

        for (var y = minY; y <= maxY; y++)
        {
            var pixelY = y + 0.5f;
            var pixelX = minX + 0.5f;
            var w1 =
                ((b.X - pixelX) * (c.Y - pixelY) -
                 (b.Y - pixelY) * (c.X - pixelX)) * inverseArea;
            var w2 =
                ((c.X - pixelX) * (a.Y - pixelY) -
                 (c.Y - pixelY) * (a.X - pixelX)) * inverseArea;
            var w3 = 1f - w1 - w2;

            for (var x = minX; x <= maxX; x++)
            {
                if (w1 >= 0 && w2 >= 0 && w3 >= 0)
                    DrawFragment(x, y, w1, w2, w3, a, b, c,
                        hasTextureCoordinates, material, shader);

                w1 += w1StepX;
                w2 += w2StepX;
                w3 += w3StepX;
            }
        }
    }

    private void DrawFragment(
        int x,
        int y,
        float w1,
        float w2,
        float w3,
        RasterVertex a,
        RasterVertex b,
        RasterVertex c,
        bool hasTextureCoordinates,
        Material material,
        IFragmentShader shader)
    {
        var depth = w1 * a.Z + w2 * b.Z + w3 * c.Z;
        if (depthTestEnabled && !depthBuffer.TestAndSet(x, y, depth))
            return;

        double u = 0;
        double v = 0;

        if (hasTextureCoordinates)
        {
            var inverseW = w1 * a.InverseW + w2 * b.InverseW + w3 * c.InverseW;
            if (Math.Abs(inverseW) < MinimumInterpolatedInverseW)
                return;

            u = (w1 * a.UOverW + w2 * b.UOverW + w3 * c.UOverW) / inverseW;
            v = (w1 * a.VOverW + w2 * b.VOverW + w3 * c.VOverW) / inverseW;
        }

        var fragment = new Fragment(depth, u, v, hasTextureCoordinates);
        colorBuffer[y * width + x] = shader.Shade(fragment, material);
    }

    public void Draw(Graphics graphics)
    {
        if (colorBuffer.Length == 0)
            return;

        using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        var bounds = new Rectangle(0, 0, width, height);
        var data = bitmap.LockBits(bounds, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

        try
        {
            if (data.Stride == width * sizeof(int))
            {
                Marshal.Copy(colorBuffer, 0, data.Scan0, colorBuffer.Length);
            }
            else
            {
                for (var y = 0; y < height; y++)
                {
                    var row = IntPtr.Add(data.Scan0, y * data.Stride);
                    Marshal.Copy(colorBuffer, y * width, row, width);
                }
            }
        }
        finally
        {
            bitmap.UnlockBits(data);
        }

        graphics.DrawImageUnscaled(bitmap, 0, 0);
    }
}
