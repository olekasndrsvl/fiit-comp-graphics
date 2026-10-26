using Lab6.Core.Geometry;
using MathNet.Numerics.LinearAlgebra;

namespace Lab6.Core.Mathematics;

/// <summary>Матрицы преобразований 4×4. Векторы-строки, углы в радианах.</summary>
public static class TransformMatrix
{
    public static Matrix<double> Identity => Matrix<double>.Build.DenseIdentity(4);

    // TODO [2]: заполнить матрицу переноса; учесть столбцы вместо строк со слайда 16.
    public static Matrix<double> Translation(double x, double y, double z) =>
        throw new NotImplementedException("TODO [2]: матрица переноса.");

    // TODO [2]: заполнить матрицу масштабирования по X, Y и Z.
    public static Matrix<double> Scale(double x, double y, double z) =>
        throw new NotImplementedException("TODO [2]: матрица масштаба.");

    // TODO [2]: нормализовать направление оси и собрать матрицу поворота (слайд 19).
    public static Matrix<double> Rotation(double x, double y, double z, double radians) =>
        throw new NotImplementedException("TODO [2]: матрица поворота.");

    // TODO [2]: построить направление B−A и составить матрицу поворота вокруг AB.
    // При A=B выбросить ArgumentException.
    public static Matrix<double> RotationAroundLine(Point3D a, Point3D b, double radians) =>
        throw new NotImplementedException("TODO [2]: поворот вокруг прямой AB.");

    // TODO [2]: матрица отражения относительно XY, XZ или YZ.
    public static Matrix<double> Reflection(string plane) =>
        throw new NotImplementedException("TODO [2]: матрица отражения.");

    // TODO [2]: составить перенос к началу, операцию и обратный перенос.
    public static Matrix<double> Around(Point3D pivot, Matrix<double> operation) =>
        throw new NotImplementedException("TODO [2]: преобразование относительно заданной точки.");

    // TODO [2]: умножить матрицу на (x, y, z, 1), сохранив w для проецирования.
    public static (double X, double Y, double Z, double W) TransformHomogeneous(
        Matrix<double> matrix, Point3D point) =>
        throw new NotImplementedException("TODO [2]: преобразование однородных координат.");

    // TODO [2]: получить декартовы координаты с проверкой w перед делением.
    public static Point3D Transform(Matrix<double> matrix, Point3D point) =>
        throw new NotImplementedException("TODO [2]: преобразование точки.");
}
