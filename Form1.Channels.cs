using FastBitmap;

namespace Lab2;

public partial class Form1
{
    private void SolveChannels(Bitmap source)
    {
        var red = new Bitmap(source.Width, source.Height);
        var green = new Bitmap(source.Width, source.Height);
        var blue = new Bitmap(source.Width, source.Height);

        var redHistogram = new int[256];
        var greenHistogram = new int[256];
        var blueHistogram = new int[256];

        using (var fastSource = new FastBitmap.FastBitmap(source))
        using (var fastRed = new FastBitmap.FastBitmap(red))
        using (var fastGreen = new FastBitmap.FastBitmap(green))
        using (var fastBlue = new FastBitmap.FastBitmap(blue))
        {
            for (var y = 0; y < source.Height; y++)
                for (var x = 0; x < source.Width; x++)
                {
                    var color = fastSource[x, y];

                    redHistogram[color.R]++;
                    greenHistogram[color.G]++;
                    blueHistogram[color.B]++;

                    fastRed[x, y] = Color.FromArgb(color.A, color.R, 0, 0);
                    fastGreen[x, y] = Color.FromArgb(color.A, 0, color.G, 0);
                    fastBlue[x, y] = Color.FromArgb(color.A, 0, 0, color.B);
                }
        }


        SetChannelResults(red, green, blue,
                          redHistogram, greenHistogram, blueHistogram);
    }
}