using PixelBuffer = FastBitmap.FastBitmap;

namespace Lab3;

public static class BoundaryTracer
{
    public static IReadOnlyList<Point> Trace(PixelBuffer image, Point start, Color boundaryColor)
    {
        // TODO 1в: проверить начальную точку и обойти связную границу заданного цвета.
        // TODO: записывать точки в порядке обхода; определить условие завершения,
        // обработать одиночный пиксел, незамкнутую границу и края изображения.
        throw new NotImplementedException("TODO 1в: обход границы с сохранением порядка точек.");
    }

    public static void DrawBoundary(PixelBuffer image, IReadOnlyList<Point> boundary, Color color)
    {
        // TODO 1в: прорисовать полученный список точек поверх исходного изображения.
        throw new NotImplementedException("TODO 1в: отображение найденной границы.");
    }
}
