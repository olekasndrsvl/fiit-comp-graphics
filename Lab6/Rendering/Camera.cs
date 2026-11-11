using Lab6.Core.Geometry;
using MathNet.Numerics.LinearAlgebra;

namespace Lab6.Rendering;

public sealed class Camera
{
    public const double FieldOfView = Math.PI / 3;
    private const double FramePadding = 1.1;
    private const double OrbitRadiansPerPixel = Math.PI / 360;
    private const double ZoomFactorPerWheelStep = 0.9;
    private const double MaximumElevation = Math.PI / 2 - Math.PI / 180;
    private const double MinimumDistance = 0.01;

    private double azimuth;
    private double elevation;
    private double distance = 1;
    private double fittedDistance = 1;

    public Point3D Position { get; private set; } = new(0, 0, -1);
    public Point3D Target { get; private set; } = new(0, 0, 0);
    public Point3D Up { get; private set; } = new(0, 1, 0);
    public double OrthographicZoom => fittedDistance / distance;

    public void FitTo(IReadOnlyList<Point3D> positions, double aspect)
    {
        if (positions.Count == 0)
        {
            Position = new Point3D(0, 0, -1);
            Target = new Point3D(0, 0, 0);
            return;
        }

        var minX = positions.Min(point => point.X);
        var maxX = positions.Max(point => point.X);
        var minY = positions.Min(point => point.Y);
        var maxY = positions.Max(point => point.Y);
        var minZ = positions.Min(point => point.Z);
        var maxZ = positions.Max(point => point.Z);

        Target = new Point3D(
            (minX + maxX) / 2,
            (minY + maxY) / 2,
            (minZ + maxZ) / 2);

        var halfWidth = (maxX - minX) / 2;
        var halfHeight = (maxY - minY) / 2;
        var halfDepth = (maxZ - minZ) / 2;
        var tanHalfFov = Math.Tan(FieldOfView / 2);
        var distanceByWidth = halfWidth / (tanHalfFov * aspect);
        var distanceByHeight = halfHeight / tanHalfFov;
        var distance = (Math.Max(distanceByWidth, distanceByHeight) + halfDepth) * FramePadding;

        this.distance = Math.Max(distance, MinimumDistance);
        fittedDistance = this.distance;
        azimuth = 0;
        elevation = 0;
        UpdatePosition();
    }

    public void Orbit(int horizontalPixels, int verticalPixels)
    {
        azimuth += horizontalPixels * OrbitRadiansPerPixel;
        elevation = Math.Clamp(
            elevation + verticalPixels * OrbitRadiansPerPixel,
            -MaximumElevation,
            MaximumElevation);
        UpdatePosition();
    }

    public void Zoom(double wheelSteps)
    {
        distance = Math.Max(
            MinimumDistance,
            distance * Math.Pow(ZoomFactorPerWheelStep, wheelSteps));
        UpdatePosition();
    }

    public void MoveTo(Point3D position)
    {
        Position = position;
        UpdateOrbitParameters();
    }

    public void LookAt(Point3D target)
    {
        Target = target;
        UpdateOrbitParameters();
    }

    private void UpdatePosition()
    {
        var horizontalDistance = distance * Math.Cos(elevation);
        Position = new Point3D(
            Target.X + horizontalDistance * Math.Sin(azimuth),
            Target.Y + distance * Math.Sin(elevation),
            Target.Z - horizontalDistance * Math.Cos(azimuth));
    }

    private void UpdateOrbitParameters()
    {
        var offsetX = Position.X - Target.X;
        var offsetY = Position.Y - Target.Y;
        var offsetZ = Position.Z - Target.Z;
        distance = Math.Max(
            Math.Sqrt(offsetX * offsetX + offsetY * offsetY + offsetZ * offsetZ),
            MinimumDistance);
        fittedDistance = distance;
        elevation = Math.Asin(Math.Clamp(offsetY / distance, -1, 1));
        azimuth = Math.Atan2(offsetX, -offsetZ);
    }

    public Matrix<double> GetViewMatrix()
    {
        var position = ToVector(Position);
        var forward = Normalize(ToVector(Target) - position);
        var right = Normalize(Cross(ToVector(Up), forward));
        var cameraUp = Cross(forward, right);

        return Matrix<double>.Build.DenseOfArray(new[,]
        {
            { right[0], cameraUp[0], forward[0], 0 },
            { right[1], cameraUp[1], forward[1], 0 },
            { right[2], cameraUp[2], forward[2], 0 },
            {
                -position.DotProduct(right),
                -position.DotProduct(cameraUp),
                -position.DotProduct(forward),
                1
            }
        });
    }

    private static Vector<double> ToVector(Point3D point) =>
        Vector<double>.Build.DenseOfArray([point.X, point.Y, point.Z]);

    private static Vector<double> Cross(Vector<double> left, Vector<double> right) =>
        Vector<double>.Build.DenseOfArray(
        [
            left[1] * right[2] - left[2] * right[1],
            left[2] * right[0] - left[0] * right[2],
            left[0] * right[1] - left[1] * right[0]
        ]);

    private static Vector<double> Normalize(Vector<double> vector)
    {
        if (vector.L2Norm() == 0)
            throw new InvalidOperationException("Невозможно определить направление камеры.");
        return vector.Normalize(2);
    }
}
