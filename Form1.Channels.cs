using FastBitmap;

namespace Lab2;

public partial class Form1
{
    private void SolveChannels(Bitmap source)
    {
        // TODO: выделить R, G, B и подсчитать три гистограммы int[256].
        // source — исходник, его не изменяем и не освобождаем.
        //
        // Пример вывода после вычислений:
        // red, green, blue — три Bitmap с выделенными каналами.
        // redHistogram, greenHistogram, blueHistogram — три массива int[256].
        // Например, redHistogram[128] — число пикселей исходника со значением R = 128.
        //
        // SetChannelResults(red, green, blue, redHistogram, greenHistogram, blueHistogram);
        //
        // Метод показывает все три изображения и обновляет их гистограммы.
        // Выводить после выхода из using с FastBitmap, когда изображения разблокированы.
        // Переданные Bitmap не освобождать, пока они отображаются в PictureBox.
    }
}
