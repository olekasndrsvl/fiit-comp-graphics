namespace Lab6.Core.Geometry;
public readonly record struct Normal(double X, double Y, double Z)
{
    public double Length => Math.Sqrt(X * X + Y * Y + Z * Z);

    public Normal Normalize()
    {
        var len = Length;
        return len < 1e-12 ? this : new Normal(X / len, Y / len, Z / len);
    }

    public static double Dot(Normal a, Normal b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    public static Normal Cross(Normal a, Normal b) => new(
        a.Y * b.Z - a.Z * b.Y,
        a.Z * b.X - a.X * b.Z,
        a.X * b.Y - a.Y * b.X
    );

    public static Normal FromPoints(Point3D p0, Point3D p1, Point3D p2)
    {
        var ux = p1.X - p0.X; var uy = p1.Y - p0.Y; var uz = p1.Z - p0.Z;
        var vx = p2.X - p0.X; var vy = p2.Y - p0.Y; var vz = p2.Z - p0.Z;
        return new Normal(
            uy * vz - uz * vy,
            uz * vx - ux * vz,
            ux * vy - uy * vx
        );
    }

    public static Normal FromPoint(Point3D p) => new(p.X, p.Y, p.Z);

    public static Normal operator -(Normal a, Normal b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Normal operator +(Normal a, Normal b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Normal operator -(Normal a) => new(-a.X, -a.Y, -a.Z);
    public static Normal operator *(Normal a, double s) => new(a.X * s, a.Y * s, a.Z * s);
}