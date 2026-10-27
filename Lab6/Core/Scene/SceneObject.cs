using Lab6.Core.Geometry;
using Lab6.Core.Mathematics;
using MathNet.Numerics.LinearAlgebra;

namespace Lab6.Core.Scene;

public sealed class SceneObject(Polyhedron geometry)
{
    public Polyhedron Geometry { get; } = geometry;
    public Matrix<double> Model { get; private set; } = TransformMatrix.Identity;

    public Point3D Center
    {
        get
        {
            var positions = Geometry.Positions;

            if (positions == null || positions.Count == 0)
                return TransformMatrix.Transform(Model, new Point3D(0, 0, 0));

            double sumX = positions.Sum(p => p.X);
            double sumY = positions.Sum(p => p.Y);
            double sumZ = positions.Sum(p => p.Z);
            int count = positions.Count;

            var localCenter = new Point3D(sumX / count, sumY / count, sumZ / count);

            return TransformMatrix.Transform(Model, localCenter);
        }
    }

    public void Apply(Matrix<double> operation) => Model = Model * operation;

    public void Reset() => Model = TransformMatrix.Identity;
}
