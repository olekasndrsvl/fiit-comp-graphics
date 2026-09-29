using System.Drawing;
using FastBitmap;

namespace Lab3;

public static class LineRasterizer
{
    public static void DrawBresenham(
        Bitmap image,
        Point start,
        Point end,
        Color color)
    {
        using var bitmap = new FastBitmap.FastBitmap(image);

        int x = start.X;
        int y = start.Y;
        int dx = Math.Abs(end.X - start.X);
        int dy = Math.Abs(end.Y - start.Y);

        int sx = start.X < end.X ? 1 : -1;
        int sy = start.Y < end.Y ? 1 : -1;

        int error = dx - dy;

        while (true)
        {
            SetPixel(bitmap, x, y, color);

            if (x == end.X && y == end.Y)
                break;

            int error2 = 2 * error;

            if (error2 > -dy)
            {
                error -= dy;
                x += sx;
            }

            if (error2 < dx)
            {
                error += dx;
                y += sy;
            }
        }
    }

    public static void DrawWu(
        Bitmap image,
        Point start,
        Point end,
        Color color)
    {
        using var bitmap = new FastBitmap.FastBitmap(image);

        int x0 = start.X;
        int y0 = start.Y;
        int x1 = end.X;
        int y1 = end.Y;

        if (x0 == x1 && y0 == y1)
        {
            SetPixel(bitmap, x0, y0, color);
            return;
        }

        bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);

        if (steep)
        {
            (x0, y0) = (y0, x0);
            (x1, y1) = (y1, x1);
        }

        if (x0 > x1)
        {
            (x0, x1) = (x1, x0);
            (y0, y1) = (y1, y0);
        }

        float dx = x1 - x0;
        float dy = y1 - y0;
        float gradient = dx == 0 ? 1 : dy / dx;

        // Первая конечная точка
        int xEnd = x0;
        float yEnd = y0 + gradient * (xEnd - x0);
        float xGap = Rfpart(x0 + 0.5f);
        int yPixel = (int)Math.Floor(yEnd);

        Plot(
            bitmap,
            steep,
            xEnd,
            yPixel,
            Rfpart(yEnd) * xGap,
            color);

        Plot(
            bitmap,
            steep,
            xEnd,
            yPixel + 1,
            Fpart(yEnd) * xGap,
            color);

        float intery = yEnd + gradient;

        // Последняя конечная точка
        xEnd = x1;
        yEnd = y1 + gradient * (xEnd - x1);
        xGap = Fpart(x1 + 0.5f);
        yPixel = (int)Math.Floor(yEnd);

        // Основная часть линии
        for (int x = x0 + 1; x < x1; x++)
        {
            yPixel = (int)Math.Floor(intery);

            Plot(
                bitmap,
                steep,
                x,
                yPixel,
                Rfpart(intery),
                color);

            Plot(
                bitmap,
                steep,
                x,
                yPixel + 1,
                Fpart(intery),
                color);

            intery += gradient;
        }

        // Последняя конечная точка
        Plot(
            bitmap,
            steep,
            xEnd,
            yPixel,
            Rfpart(yEnd) * xGap,
            color);

        Plot(
            bitmap,
            steep,
            xEnd,
            yPixel + 1,
            Fpart(yEnd) * xGap,
            color);
    }

    private static float Fpart(float value)
    {
        return value - MathF.Floor(value);
    }

    private static float Rfpart(float value)
    {
        return 1f - Fpart(value);
    }

    private static void SetPixel(
        FastBitmap.FastBitmap bitmap,
        int x,
        int y,
        Color color)
    {
        if ((uint)x >= (uint)bitmap.Width ||
            (uint)y >= (uint)bitmap.Height)
        {
            return;
        }

        bitmap.SetPixel(new Point(x, y), color);
    }

    private static void Plot(
        FastBitmap.FastBitmap bitmap,
        bool steep,
        int x,
        int y,
        float brightness,
        Color color)
    {
        if (brightness <= 0)
            return;

        int pixelX = steep ? y : x;
        int pixelY = steep ? x : y;

        if ((uint)pixelX >= (uint)bitmap.Width ||
            (uint)pixelY >= (uint)bitmap.Height)
        {
            return;
        }

        var point = new Point(pixelX, pixelY);
        Color background = bitmap.GetPixel(point);

        float alpha = brightness * color.A / 255f;

        byte r = (byte)(color.R * alpha + background.R * (1f - alpha));
        byte g = (byte)(color.G * alpha + background.G * (1f - alpha));
        byte b = (byte)(color.B * alpha + background.B * (1f - alpha));

        bitmap.SetPixel(
            point,
            Color.FromArgb(255, r, g, b));
    }
}