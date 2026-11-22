using System.Drawing.Imaging;
using System.Numerics;
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

    private static int ToArgb(Vector3 linearColor)
    {
        var exposed = linearColor * 1.22f;
        var mapped = new Vector3(
            exposed.X / (1f + exposed.X),
            exposed.Y / (1f + exposed.Y),
            exposed.Z / (1f + exposed.Z));
        var gamma = new Vector3(
            MathF.Pow(Math.Clamp(mapped.X, 0f, 1f), 1f / 2.2f),
            MathF.Pow(Math.Clamp(mapped.Y, 0f, 1f), 1f / 2.2f),
            MathF.Pow(Math.Clamp(mapped.Z, 0f, 1f), 1f / 2.2f));

        var red = (int)(gamma.X * 255f + 0.5f);
        var green = (int)(gamma.Y * 255f + 0.5f);
        var blue = (int)(gamma.Z * 255f + 0.5f);
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
