namespace Lab6.Core.Geometry;

/// Согласованность обхода вершин граней: нормали наружу или внутрь. Была вынесена из BackfaceCuller.cs для удобства пользования
public static class MeshOrientation
{
    private static readonly Dictionary<Polyhedron, bool> Cache = new();

    public static bool IsWindingInverted(Polyhedron geometry)
    {
        if (Cache.TryGetValue(geometry, out var inverted))
            return inverted;

        inverted = Compute(geometry);
        Cache[geometry] = inverted;
        return inverted;
    }

    private static bool Compute(Polyhedron geometry)
    {
        if (geometry.Positions.Count == 0)
            return false;

        double cx = 0, cy = 0, cz = 0;
        foreach (var point in geometry.Positions)
        {
            cx += point.X; cy += point.Y; cz += point.Z;
        }
        cx /= geometry.Positions.Count;
        cy /= geometry.Positions.Count;
        cz /= geometry.Positions.Count;

        double sum = 0;
        foreach (var face in geometry.Faces)
        {
            var vertexIndices = face.Vertices;
            if (vertexIndices.Count < 3)
                continue;

            var p0 = geometry.Positions[vertexIndices[0].PositionIndex];
            var p1 = geometry.Positions[vertexIndices[1].PositionIndex];
            var p2 = geometry.Positions[vertexIndices[2].PositionIndex];
            var normal = Normal.FromPoints(p0, p1, p2);

            double fx = 0, fy = 0, fz = 0;
            foreach (var vertex in vertexIndices)
            {
                var p = geometry.Positions[vertex.PositionIndex];
                fx += p.X; fy += p.Y; fz += p.Z;
            }
            fx /= vertexIndices.Count;
            fy /= vertexIndices.Count;
            fz /= vertexIndices.Count;

            var toFace = new Normal(fx - cx, fy - cy, fz - cz);
            sum += Normal.Dot(normal, toFace);
        }

        return sum < 0;
    }
}
