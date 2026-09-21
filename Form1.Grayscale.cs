using FastBitmap;

namespace Lab2;
using FastBitmap;
public partial class Form1
{
    private void SolveGrayscale(Bitmap source, HistogramView firstHistogramView, HistogramView secondHistogramView)
    {
        var image = new FastBitmap(source);
        
        int[] firstHistogram = new int[256];
        int[] secondHistogram = new int[256];
        
        Bitmap gray1 = image.Select(color =>
        {
            int intensity = (int)(0.299 * color.R + 0.587 * color.G + 0.114 * color.B);
            firstHistogram[intensity]++;
            return Color.FromArgb(color.A, intensity, intensity, intensity);
        });
        
        Bitmap gray2 = image.Select(color =>
        {
            int intensity = (int)(0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B);
            secondHistogram[intensity]++;
            return Color.FromArgb(color.A, intensity, intensity, intensity);
        });
        Bitmap grayDiff = image.Select(color =>
        {
            double first = 0.299 * color.R + 0.587 * color.G + 0.114 * color.B;
            double second = 0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B;
            int intensity = (int)Math.Abs(first - second);
            return Color.FromArgb(color.A, intensity, intensity, intensity);
        });
        
        image.Dispose();
        
        grayscaleFirstPreview.Image = gray1;
        grayscaleSecondPreview.Image = gray2;
        differencePreview.Image = grayDiff;
       
       
        firstHistogramView.SetValues(firstHistogram);
        secondHistogramView.SetValues(secondHistogram);
        
    }
}
