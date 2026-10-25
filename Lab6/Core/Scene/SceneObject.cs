using Lab6.Core.Geometry;
using Lab6.Core.Mathematics;

namespace Lab6.Core.Scene;

public sealed class SceneObject(Polyhedron geometry)
{
    public Polyhedron Geometry { get; } = geometry;
    public Matrix4 Model { get; private set; } = Matrix4.Identity;

    // TODO [2]: найти центр по вершинам и перевести его матрицей Model в мировые координаты.
    public Point3D Center => throw new NotImplementedException("TODO [2]: центр многогранника.");

    // TODO [2]: накопить преобразование в Model, не меняя исходные точки.
    public void Apply(Matrix4 operation) =>
        throw new NotImplementedException("TODO [2]: применение преобразования к объекту.");

    public void Reset() => Model = Matrix4.Identity;
}
