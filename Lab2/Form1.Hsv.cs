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

        value = max; 
        saturation = (max == 0) ? 0 : delta / max; 

        if (delta == 0)
        {
            hue = 0;
            return; 
        }

        if (max == r) 
        {
            if (g >= b) hue = 60 * ((g - b) / delta) + 0;
            else hue = 60 * ((g - b) / delta) + 360;
        }
        else if (max == g) hue = 60 * ((b - r) / delta) + 120;

        else hue = 60 * ((r - g) / delta) + 240;
    }

    private static (byte R, byte G, byte B) HsvToRgb(double hue, double saturation, double value)
    {
        hue = ((hue % 360) + 360) % 360;
        // Определяем сектор, f принимает значения от 0 до 1 (степень перехода от одного цвета к другому)
        int hi = (int)Math.Floor(hue / 60.0) % 6;
        double f = (hue / 60.0) - Math.Floor(hue / 60.0);

        double p = value * (1 - saturation); // самый темный
        double q = value * (1 - f * saturation); // ближе к концу сектора
        double t = value * (1 - (1 - f) * saturation); // ближе к началу

        double r = 0, g = 0, b = 0;

        switch (hi)
        {
            case 0: 
                r = value; g = t; b = p;
                break;
            case 1: 
                r = q; g = value; b = p;
                break;
            case 2: 
                r = p; g = value; b = t;
                break;
            case 3: 
                r = p; g = q; b = value;
                break;
            case 4: 
                r = t; g = p; b = value;
                break;
            case 5: 
                r = value; g = p; b = q;
                break;
        }

        byte rByte = (byte)Math.Clamp(Math.Round(r * 255), 0, 255);
        byte gByte = (byte)Math.Clamp(Math.Round(g * 255), 0, 255);
        byte bByte = (byte)Math.Clamp(Math.Round(b * 255), 0, 255);

        return (rByte, gByte, bByte);
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
