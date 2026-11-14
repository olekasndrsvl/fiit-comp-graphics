using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Lab6.Rendering;

internal readonly record struct ScreenPoint(float X, float Y, float Z);

public sealed class SoftwareRasterizer
{
    private readonly ZBuffer depthBuffer = new();
    private int[] colorBuffer = [];
    private int width;
    private int height;
    private float minDepth;
    private float maxDepth;

    public void Initialize(Size viewport, float minDepth, float maxDepth)
    {
        width = viewport.Width;
        height = viewport.Height;
        this.minDepth = minDepth;
        this.maxDepth = maxDepth;

        var bufferSize = width * height;
        if (colorBuffer.Length != bufferSize)
            colorBuffer = new int[bufferSize];

        Array.Fill(colorBuffer, Color.White.ToArgb());
        depthBuffer.Initialize(viewport);
    }

    internal void RasterizePolygon(IReadOnlyList<ScreenPoint> vertices)
    {
        if (vertices.Count < 3)
            return;

        for (var i = 1; i < vertices.Count - 1; i++)
            RasterizeTriangle(vertices[0], vertices[i], vertices[i + 1]);
    }

    private void RasterizeTriangle(ScreenPoint a, ScreenPoint b, ScreenPoint c)
    {
        var area =
            (b.X - a.X) * (c.Y - a.Y) -
            (b.Y - a.Y) * (c.X - a.X);

        if (Math.Abs(area) < 1e-6f)
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
                {
                    var depth = w1 * a.Z + w2 * b.Z + w3 * c.Z;
                    if (depthBuffer.TestAndSet(x, y, depth))
                        colorBuffer[y * width + x] = GetDepthColor(depth);
                }

                w1 += w1StepX;
                w2 += w2StepX;
                w3 += w3StepX;
            }
        }
    }

    private int GetDepthColor(float depth)
    {
        var range = maxDepth - minDepth;
        var normalizedDepth = range > 1e-6f
            ? (depth - minDepth) / range
            : 0.5f;
        var intensity = 1f - Math.Clamp(normalizedDepth, 0f, 1f);
        var channel = (int)(25 + intensity * 230);
        return Color.FromArgb(channel, channel, channel).ToArgb();
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
