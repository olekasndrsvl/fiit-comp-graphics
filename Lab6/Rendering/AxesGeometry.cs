using Lab6.Core.Geometry;

namespace Lab6.Rendering;

public static class AxesGeometry
{
    public const double Length = 2.0;

    public static readonly (Point3D A, Point3D B, Color Color)[] Segments =
    [
        (new(0, 0, 0), new(Length, 0, 0), Color.IndianRed),   // X
        (new(0, 0, 0), new(0, Length, 0), Color.ForestGreen), // Y
        (new(0, 0, 0), new(0, 0, Length), Color.RoyalBlue),   // Z
    ];
}
