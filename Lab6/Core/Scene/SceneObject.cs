using Lab6.Core.Geometry;
using Lab6.Core.Mathematics;
using MathNet.Numerics.LinearAlgebra;

namespace Lab6.Core.Scene;

public sealed class SceneObject(Polyhedron geometry)
{
    public Polyhedron Geometry { get; } = geometry;
    public Matrix<double> Model { get; private set; } = TransformMatrix.Identity;

    // TODO [2]: найти центр по вершинам и перевести его матрицей Model в мировые координаты.
    public Point3D Center => throw new NotImplementedException("TODO [2]: центр многогранника.");

    // TODO [2]: накопить преобразование в Model, не меняя исходные точки.
    public void Apply(Matrix<double> operation) =>
        throw new NotImplementedException("TODO [2]: применение преобразования к объекту.");

    public void Reset() => Model = TransformMatrix.Identity;
}
