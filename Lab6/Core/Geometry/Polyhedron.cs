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

    // TODO [1]: проверить индексы граней и построить список уникальных рёбер.
    // Вызывать после заполнения или изменения граней; повторный вызов не должен дублировать рёбра.
    public void RebuildEdges() =>
        throw new NotImplementedException("TODO [1]: построение рёбер многогранника.");
}
