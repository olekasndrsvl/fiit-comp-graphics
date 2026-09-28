namespace Lab3;

public static class ScanlineFill
{
    public static void FillColor(Bitmap image, Point seed, Color color)
    {
        // TODO 1а: найти горизонтальную серию цвета начальной точки и закрасить её.
        // TODO: рекурсивно обработать соседние серии сверху и снизу (4-связность).
        // TODO: учесть границы изображения, отверстия и совпадение исходного/нового цвета.
        throw new NotImplementedException("TODO 1а: рекурсивная заливка сериями пикселов.");
    }

    public static void FillPattern(Bitmap image, Point seed, Bitmap pattern)
    {
        // TODO 1б: рекурсивная заливка сериями с отдельной маской посещённых пикселов.
        // TODO: привязать начало рисунка к seed; брать координаты по модулю размеров
        // рисунка (с учётом отрицательных смещений). Маленький рисунок повторяется,
        // большой обрезается областью. Без масштабирования; отверстия не закрашивать.
        throw new NotImplementedException("TODO 1б: рекурсивная заливка рисунком.");
    }
}
