using FastBitmap;
using System.Drawing.Imaging;

namespace Lab2;

public partial class Form1
{
    private void SolveHsv(Bitmap source, int hueOffset, int saturationOffset, int valueOffset)
    {
        var result = source.Select(color =>
        {
            RgbToHsv(color, out double h, out double s, out double v);

            // Применяем смещения
            h += hueOffset;
            h = ((h % 360) + 360) % 360;

            s = Math.Clamp(s + saturationOffset / 100.0, 0.0, 1.0);
            v = Math.Clamp(v + valueOffset / 100.0, 0.0, 1.0);

            var (r, g, b) = HsvToRgb(h, s, v);
            return Color.FromArgb(color.A, r, g, b);
        });
        SetHsvResult(result);
    }

    private static void RgbToHsv(Color color, out double hue, out double saturation, out double value)
    {
        var (r, g, b) = (color.R / 255.0, color.G / 255.0, color.B / 255.0);

        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double delta = max - min;

        value = max; // Яркость
        saturation = (max == 0) ? 0 : delta / max; // Насыщенность - доля цветности от яркости, или 0(черный) по умолчанию

        // Тон разбираем по основанию конуса, показано на рисунке
        if (delta == 0)
        {
            hue = 0;
            return;
        }
        else if (max == r)
            hue = 60 * (((g - b) / delta) % 6); // именно для сектора 0 - 60
        else if (max == g)
            hue = 60 * ((b - r) / delta + 2);
        else hue = 60 * ((r - g) / delta + 4);

        if (hue < 0) hue += 360;
    }

    private static (byte R, byte G, byte B) HsvToRgb(double hue, double saturation, double value)
    {
        hue = ((hue % 360) + 360) % 360; // Приводим к виду [0, 360), дважды берем модуль из-за реализации % в c#

        double chroma = value * saturation; // чистота цвета, то же, что и delta
        double x = chroma * (1 - Math.Abs((hue / 60.0) % 2 - 1));
        double m = value - chroma; // серая добавка(минимум)

        double r1, g1, b1; // Определение сектора

        if (hue < 60) { r1 = chroma; g1 = x; b1 = 0; }
        else if (hue < 120) { r1 = x; g1 = chroma; b1 = 0; }
        else if (hue < 180) { r1 = 0; g1 = chroma; b1 = x; }
        else if (hue < 240) { r1 = 0; g1 = x; b1 = chroma; }
        else if (hue < 300) { r1 = x; g1 = 0; b1 = chroma; }
        else { r1 = chroma; g1 = 0; b1 = x; }

        byte r = (byte)Math.Clamp(Math.Round((r1 + m) * 255), 0, 255);
        byte g = (byte)Math.Clamp(Math.Round((g1 + m) * 255), 0, 255);
        byte b = (byte)Math.Clamp(Math.Round((b1 + m) * 255), 0, 255);
        return (r, g, b);
    }

    private void SaveHsvResult(Image result, string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var format = ext switch
        {
            ".png" => ImageFormat.Png,
            ".jpg" => ImageFormat.Jpeg,
            ".jpeg" => ImageFormat.Jpeg,
            ".bmp" => ImageFormat.Bmp,
            ".gif" => ImageFormat.Gif,
            ".tif" => ImageFormat.Tiff,
            ".tiff" => ImageFormat.Tiff,
            _ => ImageFormat.Png,
        };

        using var copy = new Bitmap(result.Width, result.Height);
        using (var g = Graphics.FromImage(copy))
        {
            g.DrawImage(result, 0, 0, result.Width, result.Height);
        }
        copy.Save(fileName, format);
    }
}
