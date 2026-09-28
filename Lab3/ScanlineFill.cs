namespace Lab3;

public static class ScanlineFill
{
    public static void FillColor(Bitmap image, Point seed, Color color)
    {
        if(image is null) throw new ArgumentNullException(nameof(image));

        if (seed.X < 0 || seed.Y < 0 || seed.X >= image.Width || seed.Y >= image.Height)
            return;

        Color target = image.GetPixel(seed.X, seed.Y);

        // Если уже залито этим цветом — выходим, иначе будет бесконечная рекурсия.
        if (target.ToArgb() == color.ToArgb())
            return;

        FillSeries(image, seed.X, seed.Y, target, color);
    }

    private static void FillSeries(Bitmap image, int x, int y, Color target, Color color)
    {
        if (x < 0 || x >= image.Width || y < 0 || y >= image.Height)
            return;

        if (image.GetPixel(x, y).ToArgb() != target.ToArgb())
            return;

        int left = x;
        // Пока другой цвет и не дошли до границы
        while(left - 1 >= 0 && image.GetPixel(left - 1, y).ToArgb() == target.ToArgb())
        {
            left--;
        }

        int right = x;
        while(right + 1 < image.Width && image.GetPixel(right + 1,y).ToArgb() == target.ToArgb())
        {
            right++;
        }

        // Закрашиваем полученную линию
        for (int i = left; i <= right; i++) {
            image.SetPixel(i, y, color);
        }

        // Обработка линий сверху точки
        if(y > 0)
        {
            int i = left;
            while(i <= right)
            {
                if (image.GetPixel(i, y - 1).ToArgb() == target.ToArgb())
                {
                    FillSeries(image, i, y - 1, target, color);

                    // Идем таким макаром до соседней границы по закрашенной линии
                    while (i <= right && image.GetPixel(i, y - 1).ToArgb() != target.ToArgb())
                    {
                        i++;
                    }
                }
                else i++;
            }
        }

        if(y < image.Height - 1)
        {
            int i = left;
            while (i <= right)
            {
                if (image.GetPixel(i, y + 1).ToArgb() == target.ToArgb())
                {
                    FillSeries(image, i, y + 1, target, color);

                    while (i <= right &&
                           image.GetPixel(i, y + 1).ToArgb() != target.ToArgb())
                    {
                        i++;
                    }
                }
                else i++;       
            }
        }
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
