using System.Drawing.Imaging;

namespace FastBitmap;

public unsafe class FastBitmap : IDisposable
{
    public readonly int Width;
    public readonly int Height;
    public int Count => Height * Width;

    private readonly Bitmap source;
    private readonly BitmapData bitmapData;
    private readonly int bytesPerPixel;
    private readonly int stride;
    private readonly byte* scan0;

    private const PixelFormat TargetPixelFormat = PixelFormat.Format32bppArgb;

    public FastBitmap(Bitmap bitmap)
    {
        Width = bitmap.Width;
        Height = bitmap.Height;
        source = bitmap;
        bitmapData = bitmap.LockBits(
            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadWrite,
            TargetPixelFormat);
        stride = bitmapData.Stride;
        bytesPerPixel = Image.GetPixelFormatSize(TargetPixelFormat) / 8;
        scan0 = (byte*)bitmapData.Scan0.ToPointer();
    }

    private byte* PixelOffset(Point point) =>
        scan0 + point.Y * stride + point.X * bytesPerPixel;

    public void SetPixel(Point point, Color color)
    {
        var data = PixelOffset(point);
        data[Channel.A] = color.A;
        data[Channel.R] = color.R;
        data[Channel.G] = color.G;
        data[Channel.B] = color.B;
    }

    public Color GetPixel(Point point)
    {
        var data = PixelOffset(point);
        return Color.FromArgb(
            data[Channel.A],
            data[Channel.R],
            data[Channel.G],
            data[Channel.B]);
    }

    public Color this[int x, int y]
    {
        get => GetPixel(new Point(x, y));
        set => SetPixel(new Point(x, y), value);
    }

    public void Dispose() => source.UnlockBits(bitmapData);

    private static class Channel
    {
        public const int A = 3;
        public const int R = 2;
        public const int G = 1;
        public const int B = 0;
    }
}

public static class FastBitmapTools
{
    public static void ForEach(this Bitmap source, Action<Color> action)
    {
        using var fastSource = new FastBitmap(source);
        fastSource.ForEach(action);
    }

    public static void ForEach(this FastBitmap source, Action<Color> action)
    {
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
            action(source[x, y]);
    }

    public static Bitmap Select(this Bitmap source, Func<Color, Color> transform)
    {
        using var fastSource = new FastBitmap(source);
        return fastSource.Select(transform);
    }

    public static Bitmap Select(this FastBitmap source, Func<Color, Color> transform)
    {
        var result = new Bitmap(source.Width, source.Height);
        using var fastResult = new FastBitmap(result);

        for (var y = 0; y < fastResult.Height; y++)
        for (var x = 0; x < fastResult.Width; x++)
            fastResult[x, y] = transform(source[x, y]);

        return result;
    }
}
