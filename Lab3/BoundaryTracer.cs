using System.Drawing;
using PixelBuffer = FastBitmap.FastBitmap;

namespace Lab3;

public static class BoundaryTracer
{
    private static readonly Point[] Directions =
    [
        new(1, 0),
        new(1, -1),
        new(0, -1),
        new(-1, -1),
        new(-1, 0),
        new(-1, 1),
        new(0, 1),
        new(1, 1)
    ];

    public static IReadOnlyList<Point> Trace(PixelBuffer image, Point start, Color boundaryColor)
    {
        var boundary = new List<Point>();

        if (!Inside(image, start))
            return boundary;

        var actualStart = FindNearestBoundary(image, start, boundaryColor);

        if (actualStart is null)
            return boundary;

        start = actualStart.Value;
        boundary.Add(start);

        const int firstDirection = 6;
        var first = FindNext(image, start, firstDirection, boundaryColor);

        if (first is null)
            return boundary;

        var current = first.Value.point;
        var direction = first.Value.direction;

        if (current == start)
            return boundary;

        boundary.Add(current);

        var visited = new HashSet<(Point point, int direction)>
        {
            (start, firstDirection),
            (current, direction)
        };

        int maxIterations = image.Width * image.Height * 8;

        for (int i = 0; i < maxIterations; i++)
        {
            var next = FindNext(image, current, direction, boundaryColor);

            if (next is null)
                break;

            var nextPoint = next.Value.point;
            var nextDirection = next.Value.direction;

            if (nextPoint == start)
                break;

            if (!visited.Add((nextPoint, nextDirection)))
                break;

            boundary.Add(nextPoint);

            current = nextPoint;
            direction = nextDirection;
        }

        return boundary;
    }

    // Я добавил метод поиска самой близкой точки, принадлежащей границе, чтобы не страдать в попытках попасть по ней
    private static Point? FindNearestBoundary(PixelBuffer image, Point start, Color boundaryColor)
    {
        if (!Inside(image, start))
            return null;

        if (IsBoundary(image, start, boundaryColor))
            return start;

        int maxRadius = Math.Max(image.Width, image.Height);

        for (int radius = 1; radius <= maxRadius; radius++)
        {
            int minX = Math.Max(0, start.X - radius);
            int maxX = Math.Min(image.Width - 1, start.X + radius);
            int minY = Math.Max(0, start.Y - radius);
            int maxY = Math.Min(image.Height - 1, start.Y + radius);

            for (int x = minX; x <= maxX; x++)
            {
                var top = new Point(x, minY);

                if (IsBoundary(image, top, boundaryColor))
                    return top;

                var bottom = new Point(x, maxY);

                if (IsBoundary(image, bottom, boundaryColor))
                    return bottom;
            }

            for (int y = minY + 1; y < maxY; y++)
            {
                var left = new Point(minX, y);

                if (IsBoundary(image, left, boundaryColor))
                    return left;

                var right = new Point(maxX, y);

                if (IsBoundary(image, right, boundaryColor))
                    return right;
            }
        }

        return null;
    }

    private static (Point point, int direction)? FindNext(PixelBuffer image, Point current, int direction, Color boundaryColor)
    {
        int startDirection = Mod(direction - 2, 8);

        for (int i = 0; i < 8; i++)
        {
            int candidateDirection = Mod(startDirection + i, 8);
            var offset = Directions[candidateDirection];
            var candidate = new Point(current.X + offset.X, current.Y + offset.Y);

            if (!Inside(image, candidate))
                continue;

            if (!IsBoundary(image, candidate, boundaryColor))
                continue;

            return (candidate, candidateDirection);
        }

        return null;
    }

    private static bool IsBoundary(PixelBuffer image, Point point, Color boundaryColor)
    {
        var pixel = image.GetPixel(point);
        return pixel.R == boundaryColor.R && pixel.G == boundaryColor.G && pixel.B == boundaryColor.B;
    }

    private static bool Inside(PixelBuffer image, Point point)
    {
        return point.X >= 0 && point.Y >= 0 && point.X < image.Width && point.Y < image.Height;
    }

    private static int Mod(int value, int modulus)
    {
        return (value % modulus + modulus) % modulus;
    }

    public static void DrawBoundary(PixelBuffer image, IReadOnlyList<Point> boundary, Color color)
    {
        foreach (var point in boundary)
        {
            if (Inside(image, point))
                image.SetPixel(point, color);
        }
    }
}
