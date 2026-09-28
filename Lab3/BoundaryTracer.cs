using System.Drawing;
using PixelBuffer = FastBitmap.FastBitmap;

namespace Lab3;

public static class BoundaryTracer
{
    private static readonly Point[] Directions =
    [
        new(1, 0),    // 0 →
        new(1, -1),   // 1 ↗
        new(0, -1),   // 2 ↑
        new(-1, -1),  // 3 ↖
        new(-1, 0),   // 4 ←
        new(-1, 1),   // 5 ↙
        new(0, 1),    // 6 ↓
        new(1, 1)     // 7 ↘
    ];

    public static IReadOnlyList<Point> Trace(PixelBuffer image, Point start, Color boundaryColor)
    {
        var boundary = new List<Point>();

        if (!Inside(image, start)) 
            return boundary;

        if (!IsBoundary(image, start, boundaryColor)) 
            return boundary;

        boundary.Add(start);

        const int firstDirection = 6; // Стартовое движение - вниз

        var first = FindNext(image, start, firstDirection, boundaryColor);
        if (first is null) return boundary;

        var current = first.Value.point; 
        var direction = first.Value.direction;
        boundary.Add(current);

        var firstPoint = current; 
        var firstDirectionUsed = direction;

        var maxIterations = image.Width * image.Height;

        // Обход контура
        for(int i = 0; i < maxIterations; i++)
        {
            var next = FindNext(image, current, direction, boundaryColor);

            if (next is null) 
                break; 

            var nextPoint = next.Value.point;
            var nextDirection = next.Value.direction;

            if (nextPoint == firstPoint && nextDirection == firstDirectionUsed)
                break;

            boundary.Add(nextPoint);

            current = nextPoint; 
            direction = nextDirection;
        }

        return boundary;
    }

    private static (Point point, int direction)? FindNext(PixelBuffer image, Point current, int direction, Color boundaryColor)
    {
        /*
         Следующая точка — на 90° по часовой стрелке от направления, по которому пришли,
         если не граничная, то далее против часовой стрелки поиск граничной
        */
        var startDirection = Mod(direction - 2, 8);

        for (int i = 0; i < 8; i++)
        {
            var candidateDirection = Mod(startDirection + i, 8); // против часовой стрелки

            var candidate = new Point(
                current.X + Directions[candidateDirection].X,
                current.Y + Directions[candidateDirection].Y);

            if (!Inside(image, candidate))
                continue; 

            if (!IsBoundary(image, candidate, boundaryColor))
                continue;

            return (candidate, candidateDirection);
        }

        return null;
    }


    // Проверяет, является ли пиксель граничным
    private static bool IsBoundary(
        PixelBuffer image,
        Point point,
        Color boundaryColor)
    {
        var pixel = image.GetPixel(point); 

        // Сравниваем каналы R, G, B. Альфа-канал не учитывается.
        return pixel.R == boundaryColor.R &&
               pixel.G == boundaryColor.G &&
               pixel.B == boundaryColor.B;
    }

    private static bool Inside(
        PixelBuffer image,
        Point point)
    {
        return point.X >= 0 &&
               point.Y >= 0 &&
               point.X < image.Width &&
               point.Y < image.Height;
    }

    private static int Mod(int value, int modulus)
    {
        return (value % modulus + modulus) % modulus;
    }

    // Рисует найденный контур заданным цветом.
    public static void DrawBoundary(
        PixelBuffer image,
        IReadOnlyList<Point> boundary,
        Color color)
    {
        foreach (var point in boundary) 
        {
            if (Inside(image, point)) 
                image.SetPixel(point, color); 
        }
    }
}