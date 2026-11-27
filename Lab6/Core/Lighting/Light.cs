using Lab6.Core.Geometry;

namespace Lab6.Core.Lighting;

/// Источник света в мировой системе координат
public sealed class Light
{
    public Point3D Position { get; set; } = new(5, 5, -5);
    public Color Color { get; set; } = Color.White;
    public double Intensity { get; set; } = 1.0;
    public double Ambient { get; set; } = 0.2;
    public bool Enabled { get; set; } = true;
    public double SpecularStrength { get; set; } = 0.5;
    public double Shininess { get; set; } = 32.0;
}
