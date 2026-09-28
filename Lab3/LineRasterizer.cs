namespace Lab3;

public static class LineRasterizer
{
    public static void DrawBresenham(Bitmap image, Point start, Point end, Color color)
    {
        // TODO 2: целочисленный алгоритм Брезенхема для всех октантов.
        // TODO: учесть совпадающие точки, вертикаль/горизонталь и границы холста.
        throw new NotImplementedException("TODO 2: целочисленный алгоритм Брезенхема.");
    }

    public static void DrawWu(Bitmap image, Point start, Point end, Color color)
    {
        // TODO 2: алгоритм Ву с покрытием соседних пикселов и смешиванием с фоном.
        // TODO: учесть крутые отрезки, порядок концов, совпадающие точки и края.
        throw new NotImplementedException("TODO 2: алгоритм Ву.");
    }
}
