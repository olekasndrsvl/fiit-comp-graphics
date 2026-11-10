using Lab6.Core.Geometry;
using Lab6.Core.Scene;

namespace Lab6.Rendering;

public static class BackfaceCuller
{
    // Кэш: хранит информацию о том, что модель "вывернута наизнанку"
    private static readonly Dictionary<Polyhedron, bool> WindingCache = new();

    public static HashSet<(int, int)> ComputeVisibleEdges(
        SceneObject item,
        Point3D[] viewPositions,
        bool perspective)
    {
        if (!WindingCache.TryGetValue(item.Geometry, out var invertWinding))
        {
            invertWinding = IsWindingInverted(item.Geometry);
            WindingCache[item.Geometry] = invertWinding;
        }

        var visible = new HashSet<(int, int)>();
        if (viewPositions.Length == 0) return visible;

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

            var p0 = viewPositions[verts[0].PositionIndex];
            var p1 = viewPositions[verts[1].PositionIndex];
            var p2 = viewPositions[verts[2].PositionIndex];

            // Вычисляем нормаль грани в пространстве вида
            var normal = Normal.FromPoints(p0, p1, p2).Normalize();

            if (normal.Length < 1e-12) { AddFaceEdges(); continue; } // Вырожденная грань

            // Если модель была "вывернута", инвертируем нормаль
            if (invertWinding) normal = -normal;

            // Центр грани
            double fx = 0, fy = 0, fz = 0;
            for (int i = 0; i < count; i++)
            {
                var p = viewPositions[verts[i].PositionIndex];
                fx += p.X; fy += p.Y; fz += p.Z;
            }
            fx /= count; fy /= count; fz /= count;

            // Вектор обзора
            Normal viewDir;
            if (perspective)
            {
                // В пространстве вида камера находится в (0,0,0)
                viewDir = new Normal(-fx, -fy, -fz).Normalize();
            }
            else
            {
                // Камера смотрит вдоль -Z
                viewDir = new Normal(0, 0, -1);
            }

            // Если скалярное произведение > 0 — грань лицевая
            if (Normal.Dot(normal, viewDir) > 0)
                AddFaceEdges();
        }

        return visible;
    }

    /// <summary>
    /// Проверяет, направлены ли нормали граней внутрь объекта. 
    /// Если да, значит обход вершин задан по часовой стрелке (CW).
    /// </summary>
    private static bool IsWindingInverted(Polyhedron geometry)
    {
        if (geometry.Positions.Count == 0) return false;

        // Находим приблизительный центр объекта
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

            // Центр текущей грани
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