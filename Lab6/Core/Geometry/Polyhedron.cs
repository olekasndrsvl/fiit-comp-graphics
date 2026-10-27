namespace Lab6.Core.Geometry;

/// <summary>Данные многогранника в локальной системе координат.</summary>
public sealed class Polyhedron
{
    public List<Point3D> Positions { get; }
    public List<Polygon> Faces { get; }
    public List<(int Start, int End)> Edges { get; } = [];

    public Polyhedron(IEnumerable<Point3D> positions, IEnumerable<Polygon> faces)
    {
        Positions = positions.ToList();
        Faces = faces.ToList();
    }

    public void RebuildEdges()
    {
        Edges.Clear();

        var unique = new HashSet<(int Start, int End)>();
        foreach (var face in Faces)
        {
            if (face.Vertices.Count < 2)
                continue;

            for (var i = 0; i < face.Vertices.Count; i++)
            {
                var v1 = face.Vertices[i].PositionIndex;
                var v2 = face.Vertices[(i + 1) % face.Vertices.Count].PositionIndex;

                if (v1 < 0 || v1 >= Positions.Count || v2 < 0 || v2 >= Positions.Count)
                    throw new ArgumentOutOfRangeException(nameof(Faces), "Индекс вершины грани выходит за пределы списка Positions.");

                if (v1 == v2)
                    continue;

                var a = Math.Min(v1, v2);
                var b = Math.Max(v1, v2);
                unique.Add((a, b));
            }
        }

        Edges.AddRange(unique
            .Select(e => e)
            .OrderBy(e => e.Start)
            .ThenBy(e => e.End));
    }
}
