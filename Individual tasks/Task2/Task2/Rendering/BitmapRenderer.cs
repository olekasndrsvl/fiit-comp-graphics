using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Task2;

internal static class BitmapRenderer
{
    public static Bitmap Render(
        Scene scene,
        Camera camera,
        int width,
        int height,
        IProgress<int>? progress,
        CancellationToken cancellationToken)
    {
        var pixels = new int[width * height];
        var completedRows = 0;

        Parallel.For(0, height, new ParallelOptions { CancellationToken = cancellationToken },
            y =>
            {
                for (var x = 0; x < width; x++)
                {
                    var ray = camera.GetRay(
                        (x + 0.5f) / width,
                        (y + 0.5f) / height);
                    var color = RayTracer.Trace(scene, ray);
                    pixels[y * width + x] = ToArgb(color);
                }

                var rows = Interlocked.Increment(ref completedRows);
                if (rows == height || rows % 8 == 0)
                    progress?.Report(rows * 100 / height);
            });

        cancellationToken.ThrowIfCancellationRequested();
        return CreateBitmap(width, height, pixels);
    }

    private static int ToArgb(ColorRgb linearColor)
    {
        var exposed = linearColor * 1.22f;
        var mapped = new ColorRgb(
            exposed.R / (1f + exposed.R),
            exposed.G / (1f + exposed.G),
            exposed.B / (1f + exposed.B));
        var gamma = new ColorRgb(
            MathF.Pow(Math.Clamp(mapped.R, 0f, 1f), 1f / 2.2f),
            MathF.Pow(Math.Clamp(mapped.G, 0f, 1f), 1f / 2.2f),
            MathF.Pow(Math.Clamp(mapped.B, 0f, 1f), 1f / 2.2f));

        var red = (int)(gamma.R * 255f + 0.5f);
        var green = (int)(gamma.G * 255f + 0.5f);
        var blue = (int)(gamma.B * 255f + 0.5f);
        return unchecked((int)0xFF000000 | red << 16 | green << 8 | blue);
    }

    private static Bitmap CreateBitmap(int width, int height, int[] pixels)
    {
        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        var bounds = new Rectangle(0, 0, width, height);
        var data = bitmap.LockBits(bounds, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

        try
        {
            Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
        }
        finally
        {
            bitmap.UnlockBits(data);
        }

        return bitmap;
    }
}
