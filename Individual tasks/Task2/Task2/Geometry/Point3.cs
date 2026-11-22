using System.Numerics;

namespace Task2;

internal readonly record struct Point3(float X, float Y, float Z)
{
    public static Vector3 operator -(Point3 left, Point3 right) =>
        new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

    public static Point3 operator +(Point3 point, Vector3 offset) =>
        new(point.X + offset.X, point.Y + offset.Y, point.Z + offset.Z);

    public static Point3 operator -(Point3 point, Vector3 offset) =>
        new(point.X - offset.X, point.Y - offset.Y, point.Z - offset.Z);
}
