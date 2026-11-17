using Lab6.Core.Geometry;
using Lab6.Core.Lighting;
using Lab6.Core.Mathematics;
using Lab6.Core.Scene;
using MathNet.Numerics.LinearAlgebra;

namespace Lab6.Rendering;

record struct VertexColor(float R, float G, float B)
{
    public static VertexColor FromColor(Color color) =>
        new(color.R / 255f, color.G / 255f, color.B / 255f);
    public VertexColor Clamped() => new(
       Math.Clamp(R, 0f, 1f),
       Math.Clamp(G, 0f, 1f),
       Math.Clamp(B, 0f, 1f));
}

/// Матрица нормалей: обратная транспонированная от верхнего блока 3×3
record struct NormalMatrix
{
    private const double MinimumDeterminant = 1e-12;

    public static Matrix<double> FromModel(Matrix<double> world)
    {
        var linear = world.SubMatrix(0, 3, 0, 3);

        return Math.Abs(linear.Determinant()) < MinimumDeterminant ?
            Matrix<double>.Build.DenseIdentity(3) : linear.Inverse().Transpose();
    }

    public static Normal Transform(Matrix<double> normalMatrix, Normal normal)
    {
        var vector = Vector<double>.Build.DenseOfArray([normal.X, normal.Y, normal.Z]) * normalMatrix;
        var length = vector.L2Norm();

        return length < MinimumDeterminant
          ? new Normal(0, 0, 0) : new Normal(vector[0] / length, vector[1] / length, vector[2] / length);
    }
}

/// Вершина в мировой системе координат
record struct WorldVertex(Point3D Position, Normal WorldNormal);

/// Вершинная стадия: общая для Гуро и для Фонга
static class VertexTransform
{
    public static WorldVertex[] Transform(SceneObject item, Matrix<double> world)
    {
        var positions = item.Geometry.Positions;
        var result = new WorldVertex[positions.Count];
        if (result.Length == 0)
            return result;

        var normals = VertexNormals.For(item.Geometry);
        var normalMatrix = NormalMatrix.FromModel(world);

        for (var i = 0; i < positions.Count; i++)
        {
            result[i] = new WorldVertex(
                TransformMatrix.Transform(world, positions[i]),
                NormalMatrix.Transform(normalMatrix, normals[i]));
        }

        return result;
    }
}

/// Модель освещения, используется в обоих шейдерах
static class LightingModel
{
    private const double MinimumLength = 1e-12;

    // Единичный вектор из точки в источник; (0,0,0), если источник совпал с точкой
    public static Normal ToLight(Point3D point, Light light)
    {
        var dx = light.Position.X - point.X;
        var dy = light.Position.Y - point.Y;
        var dz = light.Position.Z - point.Z;
        var length = Math.Sqrt(dx * dx + dy * dy + dz * dz);

        return length < MinimumLength
            ? new Normal(0, 0, 0) : new Normal(dx / length, dy / length, dz / length);
    }

    // Косинус угла между нормалью и направлением на свет (0..1). Нормаль должна быть единичной
    public static double Lambert(Normal normal, Point3D point, Light light)
    {
        if (!light.Enabled || normal.Length < MinimumLength)
            return 0;

        return Math.Max(0.0, Normal.Dot(normal, ToLight(point, light)));
    }

    // Цвет по модели Ламберта: ambient + диффузное отражение
    public static VertexColor ShadeLambert(VertexColor baseColor, Normal normal, Point3D point, Light light)
    {
        var ambient = light.Enabled ? Math.Clamp(light.Ambient, 0.0, 1.0) : 1.0;
        var intensity = light.Enabled ? Math.Max(light.Intensity, 0.0) : 0.0;
        var lightColor = VertexColor.FromColor(light.Color);
        var k = intensity * Lambert(normal, point, light);

        return new VertexColor(
            (float)(baseColor.R * (ambient + lightColor.R * k)),
            (float)(baseColor.G * (ambient + lightColor.G * k)),
            (float)(baseColor.B * (ambient + lightColor.B * k))).Clamped();
    }
}