using Lab6.Core.Geometry;
using Lab6.Core.Scene;

namespace Lab6.Rendering;

public static class BackfaceCuller
{
    private static readonly Dictionary<Polyhedron, bool> WindingCache = new();
    public static HashSet<(int, int)> ComputeVisibleEdges(
        SceneObject item,
        Point3D[] worldPositions,
        bool perspective,
        Normal viewDirection,
        Point3D cameraPosition)
    {
        if (!WindingCache.TryGetValue(item.Geometry, out var invertWinding))
        {
            invertWinding = IsWindingInverted(item.Geometry);
            WindingCache[item.Geometry] = invertWinding;
        }

        var visible = new HashSet<(int, int)>();
        if (worldPositions.Length == 0) return visible;

        var look = viewDirection.Normalize();
        var toCameraParallel = new Normal(-look.X, -look.Y, -look.Z);

        foreach (var face in item.Geometry.Faces)
        {
            var verts = face.Vertices;
            var count = verts.Count;
            if (count < 3) continue;

            void AddFaceEdges()
            {
                for (int i = 0; i < count; i++)
                {
                    var a = verts[i].PositionIndex;
                    var b = verts[(i + 1) % count].PositionIndex;
                    visible.Add(a < b ? (a, b) : (b, a));
                }
            }

            var p0 = worldPositions[verts[0].PositionIndex];
            var p1 = worldPositions[verts[1].PositionIndex];
            var p2 = worldPositions[verts[2].PositionIndex];

            var normal = Normal.FromPoints(p0, p1, p2).Normalize();
            if (normal.Length < 1e-12) { AddFaceEdges(); continue; }
            if (invertWinding) normal = -normal;

            Normal toCamera;
            if (perspective)
            {
                double fx = 0, fy = 0, fz = 0;
                for (int i = 0; i < count; i++)
                {
                    var p = worldPositions[verts[i].PositionIndex];
                    fx += p.X; fy += p.Y; fz += p.Z;
                }
                fx /= count; fy /= count; fz /= count;

                toCamera = new Normal(
                    cameraPosition.X - fx,
                    cameraPosition.Y - fy,
                    cameraPosition.Z - fz).Normalize();
            }
            else toCamera = toCameraParallel;

            if (Normal.Dot(normal, toCamera) > 0)
                AddFaceEdges();
        }

        return visible;
    }

    private static bool IsWindingInverted(Polyhedron geometry)
    {
        if (geometry.Positions.Count == 0) return false;

        double cx = 0, cy = 0, cz = 0;
        foreach (var p in geometry.Positions) { cx += p.X; cy += p.Y; cz += p.Z; }
        cx /= geometry.Positions.Count; cy /= geometry.Positions.Count; cz /= geometry.Positions.Count;

        double sum = 0;
        foreach (var face in geometry.Faces)
        {
            var verts = face.Vertices;
            if (verts.Count < 3) continue;

            var p0 = geometry.Positions[verts[0].PositionIndex];
            var p1 = geometry.Positions[verts[1].PositionIndex];
            var p2 = geometry.Positions[verts[2].PositionIndex];

            var normal = Normal.FromPoints(p0, p1, p2);

            double fx = 0, fy = 0, fz = 0;
            for (int i = 0; i < verts.Count; i++)
            {
                var p = geometry.Positions[verts[i].PositionIndex];
                fx += p.X; fy += p.Y; fz += p.Z;
            }
            fx /= verts.Count; fy /= verts.Count; fz /= verts.Count;

            var toFace = new Normal(fx - cx, fy - cy, fz - cz);
            sum += Normal.Dot(normal, toFace);
        }

        return sum < 0;
    }
}