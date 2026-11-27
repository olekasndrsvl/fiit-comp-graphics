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

    private static double ToonBand(double lambert)
    {
        const double transition = 0.08;

        if (lambert < 0.2 - transition)
            return 0.2;

        if (lambert < 0.2 + transition)
        {
            var t = SmoothStep(
                (lambert - (0.2 - transition)) / (2.0 * transition));

            return 0.2 + (0.4 - 0.2) * t;
        }

        if (lambert < 0.45 - transition)
            return 0.4;

        if (lambert < 0.45 + transition)
        {
            var t = SmoothStep(
                (lambert - (0.45 - transition)) / (2.0 * transition));

            return 0.4 + (0.7 - 0.4) * t;
        }

        if (lambert < 0.75 - transition)
            return 0.7;

        if (lambert < 0.75 + transition)
        {
            var t = SmoothStep(
                (lambert - (0.75 - transition)) / (2.0 * transition));

            return 0.7 + (1.0 - 0.7) * t;
        }

        return 1.0;
    }

    private static double SmoothStep(double value)
    {
        value = Math.Clamp(value, 0.0, 1.0);
        return value * value * (3.0 - 2.0 * value);
    }

    // Косинус угла между нормалью и направлением на свет (0..1). Нормаль должна быть единичной
    public static double Lambert(Normal normal, Point3D point, Light light)
    {
        if (!light.Enabled || normal.Length < MinimumLength)
            return 0;

        return Math.Max(0.0, Normal.Dot(normal, ToLight(point, light)));
    }

    public static double Specular(
        Normal normal,
        Point3D point,
        Point3D cameraPosition,
        Light light)
    {
        if (!light.Enabled || normal.Length < MinimumLength)
            return 0.0;

        var lightDirection = ToLight(point, light);
        normal = normal.Normalize();

        var ndotl = Normal.Dot(normal, lightDirection);

        if (ndotl <= 0)
            return 0.0;

        var reflection = new Normal(
            2.0 * ndotl * normal.X - lightDirection.X,
            2.0 * ndotl * normal.Y - lightDirection.Y,
            2.0 * ndotl * normal.Z - lightDirection.Z
        ).Normalize();

        var viewDirection = new Normal(
            cameraPosition.X - point.X,
            cameraPosition.Y - point.Y,
            cameraPosition.Z - point.Z
        ).Normalize();

        var rdotv = Math.Max(0.0, Normal.Dot(reflection, viewDirection));

        return light.SpecularStrength
            * Math.Max(light.Intensity, 0.0)
            * Math.Pow(rdotv, light.Shininess);
    }

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


    public static VertexColor ShadePhongToon(
        VertexColor baseColor,
        Normal normal,
        Point3D point,
        Point3D cameraPosition,
        Light light)
    {
        var ambient = light.Enabled
            ? Math.Clamp(light.Ambient, 0.0, 1.0)
            : 1.0;

        var intensity = light.Enabled
            ? Math.Max(light.Intensity, 0.0)
            : 0.0;

        var lambert = Lambert(normal, point, light);

        var band = ToonBand(lambert);

        var lightColor = VertexColor.FromColor(light.Color);
        var diffuseIntensity = intensity * band;

        var result = new VertexColor(
            (float)(baseColor.R * (ambient + lightColor.R * diffuseIntensity)),
            (float)(baseColor.G * (ambient + lightColor.G * diffuseIntensity)),
            (float)(baseColor.B * (ambient + lightColor.B * diffuseIntensity)));

        var specular = Specular(
            normal,
            point,
            cameraPosition,
            light);

        result = new VertexColor(
            result.R + (float)(lightColor.R * specular),
            result.G + (float)(lightColor.G * specular),
            result.B + (float)(lightColor.B * specular));

        return result.Clamped();
    }
}