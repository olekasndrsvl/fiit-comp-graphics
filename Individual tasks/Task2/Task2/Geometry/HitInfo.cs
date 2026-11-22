using System.Numerics;

namespace Task2;

internal readonly struct HitInfo
{
    public HitInfo(Ray ray, float distance, Vector3 outwardNormal, Material material)
    {
        Distance = distance;
        Point = ray.Origin + ray.Direction * distance;
        FrontFace = Vector3.Dot(ray.Direction, outwardNormal) < 0f;
        Normal = FrontFace ? outwardNormal : -outwardNormal;
        Material = material;
    }

    public float Distance { get; }
    public Point3 Point { get; }
    public Vector3 Normal { get; }
    public bool FrontFace { get; }
    public Material Material { get; }
}
