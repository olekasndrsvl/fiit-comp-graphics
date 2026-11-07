
using Lab6.Core.Geometry;

namespace Lab6.Modeling;

public enum SurfaceFunction
{
    SinXPlusCosY,
    SinRadius,
    XSquaredMinusYSquared,
    CosXSinY
}

public static class FunctionSurfaceBuilder
{
    public static Polyhedron Create(
        SurfaceFunction function,
        double xMin,
        double xMax,
        double yMin,
        double yMax,
        int xDivisions,
        int yDivisions)
    {
        if (xMin >= xMax)
            throw new ArgumentException("XMin должен быть меньше XMax.");

        if (yMin >= yMax)
            throw new ArgumentException("YMin должен быть меньше YMax.");

        if (xDivisions < 1 || yDivisions < 1)
            throw new ArgumentException("Количество разбиений должно быть положительным.");

        var positions = new List<Point3D>();
        var faces = new List<Polygon>();

        double dx = (xMax - xMin) / xDivisions;
        double dy = (yMax - yMin) / yDivisions;

        // Генерация вершин поверхности.
        for (int i = 0; i <= xDivisions; i++)
        {
            double x = xMin + i * dx;

            for (int j = 0; j <= yDivisions; j++)
            {
                double y = yMin + j * dy;
                double z = Evaluate(function, x, y);

                positions.Add(new Point3D(x, y, z));
            }
        }

        // Соединение соседних вершин четырёхугольными гранями.
        for (int i = 0; i < xDivisions; i++)
        {
            for (int j = 0; j < yDivisions; j++)
            {
                int current = i * (yDivisions + 1) + j;
                int nextRow = (i + 1) * (yDivisions + 1) + j;

                faces.Add(new Polygon(
                [
                    new FaceVertex(current),
                    new FaceVertex(nextRow),
                    new FaceVertex(nextRow + 1),
                    new FaceVertex(current + 1)
                ]));
            }
        }

        var polyhedron = new Polyhedron(positions, faces);
        polyhedron.RebuildEdges();

        return polyhedron;
    }

    private static double Evaluate(SurfaceFunction function, double x, double y)
    {
        return function switch
        {
            SurfaceFunction.SinXPlusCosY =>
                Math.Sin(x) + Math.Cos(y),

            SurfaceFunction.SinRadius =>
                Math.Sin(Math.Sqrt(x * x + y * y)),

            SurfaceFunction.XSquaredMinusYSquared =>
                x * x - y * y,

            SurfaceFunction.CosXSinY =>
                Math.Cos(x) * Math.Sin(y),

            _ => throw new ArgumentOutOfRangeException(nameof(function))
        };
    }
}
