using Lab6.Core.Geometry;
using System.Net.NetworkInformation;

namespace Lab6.Modeling;

public class FigureRevolution
{
    private List<Point3D> profile = [];
    public IReadOnlyList<Point3D> Profile => profile;

    public void AddPoint(Point3D point) => profile.Add(point);
    public void RemovePoint(int index)
    {
        if(index < 0 || index >= profile.Count)
            throw new ArgumentOutOfRangeException(nameof(index),
              "Выберите существующую точку образующей.");
        profile.RemoveAt( index );
    }
    public void Clear() => profile.Clear();

    public Polyhedron Generate(int axisIndex, int divisions)
    {
        if(profile.Count < 2)
            throw new InvalidOperationException("Для построения фигуры задайте минимум две точки образующей.");

        if(divisions < 3)
            throw new ArgumentException("Количество разбиений должно быть не меньше трёх.", nameof(divisions));

        if (axisIndex < 0 || axisIndex > 2)
            throw new ArgumentException("Некорректно выбрана ось вращения.", nameof(axisIndex));

        var profileCount = profile.Count;
        var positions = new List<Point3D>(profileCount * divisions);
        var faces = new List<Polygon>((profileCount - 1) * divisions);

        GenerateVertices(positions, axisIndex, divisions);
        GenerateFaces(faces, profileCount, divisions);

        var geometry = new Polyhedron(positions, faces);
        geometry.RebuildEdges();

        return geometry;
    }

    public void GenerateVertices(List<Point3D> positions, int axisIndex, int divisions)
    {
        for(int i = 0; i < divisions; i++)
        {
            var angle = Math.PI * i * 2 / divisions;
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);

            foreach(var point in profile)
            {
                var rotated = axisIndex switch
                {
                    0 => new Point3D(
                       point.X,
                       point.Y * cos - point.Z * sin,
                       point.Y * sin + point.Z * cos),

                    1 => new Point3D(
                        point.X * cos + point.Z * sin,
                        point.Y,
                        -point.X * sin + point.Z * cos),

                    _ => new Point3D(
                        point.X * cos - point.Y * sin,
                        point.X * sin + point.Y * cos,
                        point.Z)
                };

                positions.Add(rotated);
            }
        }
    }

    public void GenerateFaces(List<Polygon> faces, int profileCount, int divisions)
    {
        for(int ring = 0; ring < divisions; ring++)
        {
            var nextRing = (ring + 1) % divisions;
            for(int point = 0; point < profileCount - 1; point++)
            {
                var a = ring * profileCount + point;
                var b = nextRing * profileCount + point;
                var c = nextRing * profileCount + point + 1;
                var d = ring * profileCount + point + 1;

                faces.Add(new Polygon([new FaceVertex(a), new FaceVertex(b),new FaceVertex(c), new FaceVertex(d)]));
            }
        }
    }
}