using FastBitmap;

namespace Lab2;

public partial class Form1
{
    private void SolveChannels(Bitmap source)
    {
        var image = new FastBitmap.FastBitmap(source);

        // Гистограммы значений каждого цветового канала
        int[] redHistogram = new int[256];
        int[] greenHistogram = new int[256];
        int[] blueHistogram = new int[256];

        // Изображение только с красным каналом
        Bitmap red = image.Select(color =>
        {
            redHistogram[color.R]++;
            return Color.FromArgb(color.A, color.R, 0, 0);
        });

        // Изображение только с зелёным каналом
        Bitmap green = image.Select(color =>
        {
            greenHistogram[color.G]++;
            return Color.FromArgb(color.A, 0, color.G, 0);
        });

        // Изображение только с синим каналом
        Bitmap blue = image.Select(color =>
        {
            blueHistogram[color.B]++;
            return Color.FromArgb(color.A, 0, 0, color.B);
        });

        // Освобождаем исходное изображение
        image.Dispose();

        // Передаём изображения каналов и их гистограммы
        SetChannelResults(
            red,
            green,
            blue,
            redHistogram,
            greenHistogram,
            blueHistogram);
    }
}