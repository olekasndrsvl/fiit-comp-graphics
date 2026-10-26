using Lab6.Core.Geometry;
using MathNet.Numerics.LinearAlgebra;

namespace Lab6.Core.Mathematics;

/// <summary>Матрицы преобразований 4×4. Векторы-столбцы, углы в радианах.</summary>
public static class TransformMatrix
{
    public static Matrix<double> Identity => Matrix<double>.Build.DenseIdentity(4);

    public static Matrix<double> Translation(double x, double y, double z)
    {
        var matr = Matrix<double>.Build.DenseIdentity(4);
        matr[3, 0] = x; matr[3, 1] = y; matr[3, 2] = z;
        return matr;
    }

    public static Matrix<double> Scale(double x, double y, double z)
    {
        var matr = Matrix<double>.Build.DenseIdentity(4);
        matr[0, 0] = x; matr[1, 1] = y; matr[2, 2] = z;
        return matr;
    }

    public static Matrix<double> Rotation(double x, double y, double z, double radians)
    {
        var length = Math.Sqrt(x * x  + y * y + z * z); // вычисление длины вектора

        if (length == 0) return Identity;

        var (l, m, n) = (x / length, y / length, z / length); // нормализация оси

        double sin = Math.Sin(radians); 
        double cos = Math.Cos(radians);

        var matr = Matrix<double>.Build.Dense(4, 4);

        matr[0, 0] = l * l + cos * (1 - l * l);
        matr[0, 1] = l * (1 - cos) * m + n * sin;
        matr[0, 2] = l * (1 - cos) * n - m * sin;

        matr[1, 0] = l * (1 - cos) * m - n * sin;
        matr[1, 1] = m * m + cos * (1 - m * m);
        matr[1, 2] = m * (1 - cos) * n + l * sin;

        matr[2, 0] = l * (1 - cos) * n + m * sin;
        matr[2, 1] = m * (1 - cos) * n - l * sin;
        matr[2, 2] = n * n + cos * (1 - n * n);

        matr[3, 3] = 1.0;

        return matr;
    }

    public static Matrix<double> RotationAroundLine(Point3D a, Point3D b, double radians)
    {
        var (dx, dy, dz) = (b.X - a.X, b.Y - a.Y, b.Z - a.Z);

        if (dx == 0 && dy == 0 && dz == 0)
            throw new ArgumentException("Точки А и В не должны совпадать.");

        return Rotation(dx, dy, dz, radians);
    }

    public static Matrix<double> Reflection(string plane)
    {
        var matr = Matrix<double>.Build.DenseIdentity(4);

        switch (plane.ToUpperInvariant())
        {
            case "XY": matr[2, 2] = -1; break; // Z
            case "XZ": matr[1, 1] = -1; break; // Y
            case "YZ": matr[0, 0] = -1; break; // X
            default: throw new ArgumentException("Неизвестная плоскость.");
        }

        return matr;
    }

    public static Matrix<double> Around(Point3D pivot, Matrix<double> operation)
    {
        var toOrigin = Translation(-pivot.X, -pivot.Y, -pivot.Z);

        var fromOrigin = Translation(pivot.X, pivot.Y, pivot.Z);

        return toOrigin * operation * fromOrigin;
    }

    public static (double X, double Y, double Z, double W) TransformHomogeneous(Matrix<double> matrix, Point3D point)
    {
        var vector = Vector<double>.Build.DenseOfArray(new double[] { point.X, point.Y, point.Z, 1.0 });

        var result = vector * matrix;

        return (result[0], result[1], result[2], result[3]);
    }

    public static Point3D Transform(Matrix<double> matrix, Point3D point)
    {
        var (x, y, z, w) = TransformHomogeneous(matrix, point);

        if (w == 0)
        {
            throw new DivideByZeroException("Координата W равна нулю.");
        }

        return new Point3D(x / w, y / w, z / w);
    }
}
