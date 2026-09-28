namespace Lab3;

public readonly record struct ColoredVertex(Point Position, Color Color);

public static class TriangleRasterizer
{
    public static void Draw(Bitmap image, ColoredVertex a, ColoredVertex b, ColoredVertex c)
    {
        // TODO 3: растеризовать треугольник и интерполировать RGB трёх вершин.
        // TODO: учесть порядок вершин, вырожденный треугольник и границы холста.
        throw new NotImplementedException("TODO 3: растеризация треугольника с градиентом.");
    }
}
