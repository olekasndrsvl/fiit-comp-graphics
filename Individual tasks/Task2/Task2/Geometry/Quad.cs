using System.Numerics;

namespace Task2;

internal sealed class Quad(Point3 center, Vector3 halfAxisU, Vector3 halfAxisV, Vector3 outwardNormal, Material material) : IPrimitive
{
    public bool Intersect(Ray ray, float minimum, float maximum, out HitInfo hit)
    {
        var denominator = Vector3.Dot(ray.Direction, outwardNormal);
        if (MathF.Abs(denominator) < 1e-7f)
        {
            hit = default;
            return false;
        }

        var distance = Vector3.Dot(center - ray.Origin, outwardNormal) / denominator;
        if (distance < minimum || distance > maximum)
        {
            hit = default;
            return false;
        }

        var point = ray.Origin + ray.Direction * distance;
        var relative = point - center;
        var u = Vector3.Dot(relative, halfAxisU) / halfAxisU.LengthSquared();
        var v = Vector3.Dot(relative, halfAxisV) / halfAxisV.LengthSquared();

        if (MathF.Abs(u) > 1.0001f || MathF.Abs(v) > 1.0001f)
        {
            hit = default;
            return false;
        }

        hit = new HitInfo(ray, distance, outwardNormal, material);
        return true;
    }
}
