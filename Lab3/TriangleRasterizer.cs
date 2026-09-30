namespace Lab3;
using FastBitmap;
public readonly record struct ColoredVertex(Point Position, Color Color);

public static class TriangleRasterizer
{
    private static double SignedDoubleArea(Point a, Point b, Point c) => ((double)b.X - a.X) * ((double)c.Y - a.Y) - ((double)c.X - a.X) * ((double)b.Y - a.Y) ;
    private static int InterpolateColorChannel(int a, int b, int c, double weightA, double weightB, double weightC) => Math.Clamp((int)Math.Round(a * weightA + b * weightB + c * weightC), 0, 255);
    
    public static void Draw(Bitmap image, ColoredVertex a, ColoredVertex b, ColoredVertex c)
    {
        var area = SignedDoubleArea(a.Position, b.Position, c.Position);
        
        if (area == 0)
            return; 

        var minX = Math.Max(0, Math.Min(a.Position.X, Math.Min(b.Position.X, c.Position.X)));
        var maxX = Math.Min(image.Width - 1, Math.Max(a.Position.X, Math.Max(b.Position.X, c.Position.X)));
        
        var minY = Math.Max(0, Math.Min(a.Position.Y, Math.Min(b.Position.Y, c.Position.Y)));
        var maxY = Math.Min(image.Height - 1, Math.Max(a.Position.Y, Math.Max(b.Position.Y, c.Position.Y)));
        
        // фиксим, чтобы все не ломалось после смены размеры окна
        if (minX > maxX || minY > maxY)
            return;

        using var pixels = new FastBitmap(image);
        
        for (var y = minY; y <= maxY; y++)
        for (var x = minX; x <= maxX; x++)
        {
            var point = new Point(x, y);
            
            var weightA = SignedDoubleArea(point, b.Position, c.Position) / area;
            var weightB = SignedDoubleArea(point, c.Position, a.Position) / area;
            var weightC = SignedDoubleArea(point, a.Position, b.Position) / area;
            
            // точка вне треугольника, скип
            if (weightA < 0 || weightB < 0 || weightC < 0)
                continue;

            pixels[x, y] = Color.FromArgb(InterpolateColorChannel(a.Color.R, b.Color.R, c.Color.R, weightA, weightB, weightC), InterpolateColorChannel(a.Color.G, b.Color.G, c.Color.G, weightA, weightB, weightC), InterpolateColorChannel(a.Color.B, b.Color.B, c.Color.B, weightA, weightB, weightC));
        }
    }
}
