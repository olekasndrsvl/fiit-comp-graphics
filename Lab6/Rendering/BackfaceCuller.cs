using Lab6.Core.Geometry;
using Lab6.Core.Scene;

namespace Lab6.Rendering;

public static class BackfaceCuller
{
    public static HashSet<(int, int)> ComputeVisibleEdges(SceneObject item, Point3D[] worldPositions, bool perspective, Normal viewDirection, Point3D cameraPosition)
    {

        var visible = new HashSet<(int, int)>();
        if (worldPositions.Length == 0) return visible;

        foreach (var face in item.Geometry.Faces)
        {
            var verts = face.Vertices;
            var count = verts.Count;
            if (count < 3) continue;

            if (!IsFaceVisible(item.Geometry, face, worldPositions, perspective, viewDirection, cameraPosition))
                continue;

            for (int i = 0; i < count; i++)
            {
                var a = verts[i].PositionIndex;
                var b = verts[(i + 1) % count].PositionIndex;
                visible.Add(a < b ? (a, b) : (b, a));
            }
        }

        return visible;
    }

    public static bool IsFaceVisible(Polyhedron geometry, Polygon face, Point3D[] viewPositions, bool perspective, Normal viewDirection, Point3D cameraPosition)
    {
        var verts = face.Vertices;
        if (verts.Count < 3) return true;

        var p0 = viewPositions[verts[0].PositionIndex];
        var p1 = viewPositions[verts[1].PositionIndex];
        var p2 = viewPositions[verts[2].PositionIndex];

        var normal = Normal.FromPoints(p0, p1, p2).Normalize();
        if (normal.Length < 1e-12) return true;
        if (MeshOrientation.IsWindingInverted(geometry)) normal = -normal; 

        Normal toCamera;
        if (perspective)
        {
            double fx = 0, fy = 0, fz = 0;
            foreach (var vertex in verts)
            {
                var p = viewPositions[vertex.PositionIndex];
                fx += p.X; fy += p.Y; fz += p.Z;
            }
            var count = verts.Count;
            fx /= count; fy /= count; fz /= count;

            toCamera = new Normal(
                cameraPosition.X - fx,
                cameraPosition.Y - fy,
                cameraPosition.Z - fz).Normalize();
        }
        else toCamera = new Normal(-viewDirection.X, -viewDirection.Y, -viewDirection.Z).Normalize();

        return Normal.Dot(normal, toCamera) > 0;
    }
}
