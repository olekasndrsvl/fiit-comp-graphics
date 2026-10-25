using Lab6.Core.Geometry;

namespace Lab6.Core.Mathematics;

/// <summary>Матрица 4×4. Векторы-столбцы, углы в радианах.</summary>
public sealed class Matrix4
{
    public double[,] Values { get; } = new double[4, 4];

    public static Matrix4 Identity
    {
        get
        {
            var matrix = new Matrix4();
            for (var i = 0; i < 4; i++) matrix.Values[i, i] = 1;
            return matrix;
        }
    }

    // TODO [2]: заполнить матрицу переноса; учесть столбцы вместо строк со слайда 16.
    public static Matrix4 Translation(double x, double y, double z) =>
        throw new NotImplementedException("TODO [2]: матрица переноса.");

    // TODO [2]: заполнить матрицу масштабирования по X, Y и Z.
    public static Matrix4 Scale(double x, double y, double z) =>
        throw new NotImplementedException("TODO [2]: матрица масштаба.");

    // TODO [2]: нормализовать направление оси и собрать матрицу поворота (слайд 19).
    public static Matrix4 Rotation(double x, double y, double z, double radians) =>
        throw new NotImplementedException("TODO [2]: матрица поворота.");

    // TODO [2]: построить направление B−A и составить матрицу поворота вокруг AB.
    // При A=B выбросить ArgumentException.
    public static Matrix4 RotationAroundLine(Point3D a, Point3D b, double radians) =>
        throw new NotImplementedException("TODO [2]: поворот вокруг прямой AB.");

    // TODO [2]: матрица отражения относительно XY, XZ или YZ.
    public static Matrix4 Reflection(string plane) =>
        throw new NotImplementedException("TODO [2]: матрица отражения.");

    // TODO [2]: составить перенос к началу, операцию и обратный перенос.
    public static Matrix4 Around(Point3D pivot, Matrix4 operation) =>
        throw new NotImplementedException("TODO [2]: преобразование относительно заданной точки.");

    // TODO [2]: реализовать умножение матриц 4×4. Порядок операций существенен.
    public static Matrix4 operator *(Matrix4 a, Matrix4 b) =>
        throw new NotImplementedException("TODO [2]: умножение матриц.");

    // TODO [2]: умножить матрицу на (x, y, z, 1), сохранив w для проецирования.
    public (double X, double Y, double Z, double W) TransformHomogeneous(Point3D point) =>
        throw new NotImplementedException("TODO [2]: преобразование однородных координат.");

    // TODO [2]: получить декартовы координаты с проверкой w перед делением.
    public Point3D Transform(Point3D point) =>
        throw new NotImplementedException("TODO [2]: преобразование точки.");
}
