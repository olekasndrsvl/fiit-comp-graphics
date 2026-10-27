using MathNet.Numerics.LinearAlgebra;

namespace Lab6.Rendering;

public static class Projections
{
    public static Matrix<double> Perspective(double aspect)
    {
        const double fov = Math.PI / 3; // 60
        const double near = 0.1;
        const double far = 1000.0;

        var scaleY = 1.0 / Math.Tan(fov / 2);
        var scaleX = scaleY / aspect;

        return Matrix<double>.Build.DenseOfArray(new double[,]
        {
            { scaleX, 0,      0,                          0 },
            { 0,      scaleY, 0,                          0 },
            { 0,      0,      far / (far - near),         1 },
            { 0,      0,     -near * far / (far - near),  0 }
        });
    }


    public static Matrix<double> Axonometric()
    {
        const double azimuth = Math.PI / 4; // 45 вокруг вертикальной оси
        var elevation = Math.Atan(Math.Sin(azimuth)); // 35,26 для изометрии

        var sinAzimuth = Math.Sin(azimuth);
        var cosAzimuth = Math.Cos(azimuth);
        var sinElevation = Math.Sin(elevation);
        var cosElevation = Math.Cos(elevation);
        return Matrix<double>.Build.DenseOfArray(new double[,]
        {
            {  cosAzimuth, -sinElevation * sinAzimuth,    cosElevation * sinAzimuth,       0 },
            {  0,                    cosElevation,              sinElevation,              0 },
            { -sinAzimuth, -sinElevation * cosAzimuth,    cosElevation * cosAzimuth,       0 },
            {  0,                    0,                          0,                        1 }
        });
    }
}
