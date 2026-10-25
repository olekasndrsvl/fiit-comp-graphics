namespace Lab6.Core.Geometry;

/// <summary>Грань с упорядоченным обходом вершин.</summary>
public sealed class Polygon
{
    public List<FaceVertex> Vertices { get; }

    public Polygon(IEnumerable<FaceVertex> vertices)
    {
        ArgumentNullException.ThrowIfNull(vertices);
        Vertices = vertices.ToList();
        if (Vertices.Count < 3)
            throw new ArgumentException("Грань должна содержать минимум три вершины.", nameof(vertices));
    }
}
