using FastBitmap;

namespace Lab2;

public partial class Form1
{
    private void SolveHsv(Bitmap source, int hueOffset, int saturationOffset, int valueOffset)
    {
        // TODO: RGB -> HSV, применить смещения, HSV -> RGB.
        // H: -180..180 градусов; S и V: -100..100.
        // Каждый расчёт начинается с source, а не с предыдущего результата.
        // source не изменяем и не освобождаем.
        //
        // Пример вывода после вычислений:
        // result — Bitmap с готовыми RGB-пикселями после обратного преобразования из HSV.
        //
        // SetHsvResult(result);
        //
        // Метод показывает изображение и включает кнопку «Сохранить».
        // Вызывать после выхода из using с FastBitmap, когда result разблокирован.
        // result не освобождать, пока он отображается в PictureBox.
    }

    private void SaveHsvResult(Image result, string fileName)
    {
        // TODO: сохранить готовое RGB-изображение result в fileName.
        // Формат PNG/JPEG/BMP выбрать по расширению fileName.
        // result принадлежит области просмотра: здесь его не освобождаем.
    }
}
