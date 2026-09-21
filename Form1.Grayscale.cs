using FastBitmap;

namespace Lab2;

public partial class Form1
{
    private void SolveGrayscale(Bitmap source, HistogramView firstHistogramView, HistogramView secondHistogramView)
    {
        // TODO: две формулы оттенков серого, разность и две гистограммы.
        // source — исходник, его не изменяем и не освобождаем.
        // Для обработки доступны source.Select(...), source.ForEach(...)
        // и new FastBitmap.FastBitmap(source) внутри using.
        //
        // Пример вывода после вычислений:
        // first и second — Bitmap с результатами двух формул, difference — Bitmap разности.
        // firstHistogram и secondHistogram — int[256]: индекс = интенсивность, значение = число пикселей.
        //
        // SetGrayscaleResults(first, second, difference);
        // firstHistogramView.SetValues(firstHistogram);
        // secondHistogramView.SetValues(secondHistogram);
        //
        // Порядок важен: SetGrayscaleResults без массивов сначала очищает гистограммы.
        // Выводить после выхода из using с FastBitmap, когда изображения разблокированы.
        // Переданные Bitmap не освобождать, пока они отображаются в PictureBox.
    }
}
