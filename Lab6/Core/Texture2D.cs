using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Lab6.Core.Materials;

public sealed class Texture2D
{
    private readonly int[] pixels;

    public int Width { get; }
    public int Height { get; }
    public string? SourcePath { get; }

    private Texture2D(int width, int height, int[] pixels, string? sourcePath)
    {
        Width = width;
        Height = height;
        this.pixels = pixels;
        SourcePath = sourcePath;
    }

    public static Texture2D Load(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        using var source = new Bitmap(filePath);
        using var converted = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(converted))
            graphics.DrawImageUnscaled(source, 0, 0);

        return new Texture2D(
            converted.Width,
            converted.Height,
            ReadPixels(converted),
            Path.GetFullPath(filePath));
    }

    public Color Sample(double u, double v) => Color.FromArgb(SampleArgb(u, v));

    public int SampleArgb(double u, double v)
    {
        var normalizedU = Math.Clamp(u, 0, 1);
        var normalizedV = Math.Clamp(v, 0, 1);

        var x = (int)Math.Round(normalizedU * (Width - 1));
        var y = (int)Math.Round((1 - normalizedV) * (Height - 1));

        return pixels[y * Width + x];
    }

    private static int[] ReadPixels(Bitmap bitmap)
    {
        var pixels = new int[bitmap.Width * bitmap.Height];
        var bounds = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var data = bitmap.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

        try
        {
            for (var y = 0; y < bitmap.Height; y++)
            {
                var sourceRow = IntPtr.Add(data.Scan0, y * data.Stride);
                Marshal.Copy(sourceRow, pixels, y * bitmap.Width, bitmap.Width);
            }
        }
        finally
        {
            bitmap.UnlockBits(data);
        }

        return pixels;
    }
}
