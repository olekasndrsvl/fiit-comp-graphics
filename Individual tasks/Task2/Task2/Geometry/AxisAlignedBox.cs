using System.Numerics;

namespace Task2;

internal sealed class AxisAlignedBox(Point3 minimumBounds, Point3 maximumBounds, Material material) : IPrimitive
{
    public bool Intersect(Ray ray, float minimum, float maximum, out HitInfo hit)
    {
        var nearDistance = float.NegativeInfinity;
        var farDistance = float.PositiveInfinity;
        var nearNormal = Vector3.Zero;
        var farNormal = Vector3.Zero;

        for (var axis = 0; axis < 3; axis++)
        {
            var origin = Component(ray.Origin, axis);
            var direction = Component(ray.Direction, axis);
            var lower = Component(minimumBounds, axis);
            var upper = Component(maximumBounds, axis);

            if (MathF.Abs(direction) < 1e-7f)
            {
                if (origin < lower || origin > upper)
                {
                    hit = default;
                    return false;
                }

                continue;
            }

            var firstDistance = (lower - origin) / direction;
            var secondDistance = (upper - origin) / direction;
            var firstNormal = AxisNormal(axis, -1f);
            var secondNormal = AxisNormal(axis, 1f);

            if (firstDistance > secondDistance)
            {
                (firstDistance, secondDistance) = (secondDistance, firstDistance);
                (firstNormal, secondNormal) = (secondNormal, firstNormal);
            }

            if (firstDistance > nearDistance)
            {
                nearDistance = firstDistance;
                nearNormal = firstNormal;
            }

            if (secondDistance < farDistance)
            {
                farDistance = secondDistance;
                farNormal = secondNormal;
            }

            if (nearDistance > farDistance)
            {
                hit = default;
                return false;
            }
        }

        var distance = nearDistance >= minimum ? nearDistance : farDistance;
        var outwardNormal = nearDistance >= minimum ? nearNormal : farNormal;
        if (distance < minimum || distance > maximum)
        {
            hit = default;
            return false;
        }

        hit = new HitInfo(ray, distance, outwardNormal, material);
        return true;
    }

    private static float Component(Point3 value, int axis) => axis switch
    {
        0 => value.X,
        1 => value.Y,
        _ => value.Z
    };

    private static float Component(Vector3 value, int axis) => axis switch
    {
        0 => value.X,
        1 => value.Y,
        _ => value.Z
    };

    private static Vector3 AxisNormal(int axis, float direction) => axis switch
    {
        0 => new Vector3(direction, 0f, 0f),
        1 => new Vector3(0f, direction, 0f),
        _ => new Vector3(0f, 0f, direction)
    };
}
