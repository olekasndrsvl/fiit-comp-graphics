namespace Lab6.Core.Geometry;

/// Нормали к вершинам (по индексу позиции) в системе координат модели
public static class VertexNormals
{
    private const double MinimumLength = 1e-12;

    private static readonly Dictionary<Polyhedron, Normal[]> Cache = new();

    public static Normal[] For(Polyhedron geometry)
    {
        if (Cache.TryGetValue(geometry, out var normals))
            return normals;

        normals = Compute(geometry);
        Cache[geometry] = normals;
        return normals;
    }

    private static Normal[] Compute(Polyhedron geometry)
    {
        var result = new Normal[geometry.Positions.Count];
        if (result.Length == 0)
            return result;

        var inverted = MeshOrientation.IsWindingInverted(geometry);
        var useFileNormals = geometry.Normals.Count > 0;

        foreach (var face in geometry.Faces)
        {
            var vertices = face.Vertices;
            if (vertices.Count < 3)
                continue;

            var p0 = geometry.Positions[vertices[0].PositionIndex];
            var p1 = geometry.Positions[vertices[1].PositionIndex];
            var p2 = geometry.Positions[vertices[2].PositionIndex];

            var faceNormal = Normal.FromPoints(p0, p1, p2);
            if (inverted)
                faceNormal = -faceNormal;

            foreach (var vertex in vertices)
            {
                var normal = useFileNormals &&
                             vertex.NormalIndex is int index &&
                             index >= 0 && index < geometry.Normals.Count
                    ? geometry.Normals[index]
                    : faceNormal;

                var positionIndex = vertex.PositionIndex;
                if (positionIndex < 0 || positionIndex >= result.Length) 
                    continue;

                result[positionIndex] = result[positionIndex] + normal;
            }
        }

        for (var i = 0; i < result.Length; i++)
        {
            var normal = result[i];
            result[i] = normal.Length < MinimumLength
                ? new Normal(0, 0, 0)
                : normal.Normalize();
        }

        return result;
    }
}
