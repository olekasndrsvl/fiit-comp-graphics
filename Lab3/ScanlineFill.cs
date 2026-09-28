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
        if (image is null) throw new ArgumentNullException(nameof(image));
        if (pattern is null) throw new ArgumentNullException(nameof(pattern));

        if (seed.X < 0 || seed.Y < 0 || seed.X >= image.Width || seed.Y >= image.Height)
            return;

        Color target = image.GetPixel(seed.X, seed.Y);

        /*
        Маска: был ли пиксель уже обработан. Без неё нельзя — цвет после заливки
        может случайно совпасть с target, и получим бесконечную рекурсию.
        */
        bool[,] visited = new bool[image.Width, image.Height];

        FillSeriesPattern(image, pattern, seed, seed.X, seed.Y, target, visited);
    }

    private static void FillSeriesPattern(Bitmap image, Bitmap pattern, Point seed, int x, int y, Color target, bool[,] visited)
    {
        if (x < 0 || x >= image.Width || y < 0 || y >= image.Height)
            return;

        if (visited[x, y])
            return;

        if (image.GetPixel(x, y).ToArgb() != target.ToArgb())
            return;

        int left = x;
        while (left - 1 >= 0 && !visited[left - 1, y] &&
               image.GetPixel(left - 1, y).ToArgb() == target.ToArgb())
        {
            left--;
        }

        
        int right = x;
        while (right + 1 < image.Width && !visited[right + 1, y] &&
               image.GetPixel(right + 1, y).ToArgb() == target.ToArgb())
        {
            right++;
        }

        for (int i = left; i <= right; i++)
        {
            image.SetPixel(i, y, GetPatternPixel(pattern, seed, i, y));
            visited[i, y] = true;
        }

        // Серии сверху
        if (y > 0)
        {
            int i = left;
            while (i <= right)
            {
                if (!visited[i, y - 1] &&
                    image.GetPixel(i, y - 1).ToArgb() == target.ToArgb())
                {
                    FillSeriesPattern(image, pattern, seed, i, y - 1, target, visited);

                    // Пропускаем уже обработанную серию
                    while (i <= right && (visited[i, y - 1] ||
                            image.GetPixel(i, y - 1).ToArgb() != target.ToArgb()))
                    {
                        i++;
                    }
                }
                else i++;
            }
        }

        // Серии снизу
        if (y < image.Height - 1)
        {
            int i = left;
            while (i <= right)
            {
                if (!visited[i, y + 1] && image.GetPixel(i, y + 1).ToArgb() == target.ToArgb())
                {
                    FillSeriesPattern(image, pattern, seed, i, y + 1, target, visited);

                    while (i <= right && (visited[i, y + 1] ||
                            image.GetPixel(i, y + 1).ToArgb() != target.ToArgb()))
                    {
                        i++;
                    }
                }
                else i++;
            }
        }
    }

    /*
    Метод берёт координаты относительно точки клика, заворачивает их в размеры 
    текстуры, и возвращает цвет соответствующего пикселя текстуры.
    */
    private static Color GetPatternPixel(Bitmap pattern, Point seed, int x, int y)
    {
        int dx = x - seed.X;
        int dy = y - seed.Y;

        int px = ((dx % pattern.Width) + pattern.Width) % pattern.Width;
        int py = ((dy % pattern.Height) + pattern.Height) % pattern.Height;

        return pattern.GetPixel(px, py);
    }
}
