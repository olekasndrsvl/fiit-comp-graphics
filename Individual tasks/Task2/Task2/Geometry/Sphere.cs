using System.Numerics;

namespace Task2;

internal sealed class Sphere(Point3 center, float radius, Material material) : IPrimitive
{
    public bool Intersect(Ray ray, float minimum, float maximum, out HitInfo hit)
    {
        var originToCenter = ray.Origin - center;
        var halfB = Vector3.Dot(originToCenter, ray.Direction);
        var c = originToCenter.LengthSquared() - radius * radius;
        var discriminant = halfB * halfB - c;

        if (discriminant < 0f)
        {
            hit = default;
            return false;
        }

        var squareRoot = MathF.Sqrt(discriminant);
        var distance = -halfB - squareRoot;
        if (distance < minimum || distance > maximum)
        {
            distance = -halfB + squareRoot;
            if (distance < minimum || distance > maximum)
            {
                hit = default;
                return false;
            }
        }

        var point = ray.Origin + ray.Direction * distance;
        var outwardNormal = (point - center) / radius;
        hit = new HitInfo(ray, distance, outwardNormal, material);
        return true;
    }
}
