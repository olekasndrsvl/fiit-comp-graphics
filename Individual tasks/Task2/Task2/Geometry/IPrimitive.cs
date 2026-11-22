using System.Numerics;

namespace Task2;

internal interface IPrimitive
{
    bool Intersect(Ray ray, float minimum, float maximum, out HitInfo hit);
}
