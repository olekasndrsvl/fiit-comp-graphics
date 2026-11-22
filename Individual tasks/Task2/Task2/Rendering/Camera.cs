using System.Numerics;

namespace Task2;

internal readonly struct Camera
{
    private readonly Point3 _origin;
    private readonly Vector3 _forward;
    private readonly Vector3 _right;
    private readonly Vector3 _up;
    private readonly float _halfWidth;
    private readonly float _halfHeight;

    public Camera(Point3 origin, Point3 target, Vector3 worldUp, float verticalFieldOfView, float aspectRatio)
    {
        _origin = origin;
        _forward = Vector3.Normalize(target - origin);
        _right = Vector3.Normalize(Vector3.Cross(_forward, worldUp));
        _up = Vector3.Normalize(Vector3.Cross(_right, _forward));
        _halfHeight = MathF.Tan(verticalFieldOfView * MathF.PI / 360f);
        _halfWidth = aspectRatio * _halfHeight;
    }

    public Ray GetRay(float u, float v)
    {
        var horizontal = (2f * u - 1f) * _halfWidth;
        var vertical = (1f - 2f * v) * _halfHeight;
        var direction = Vector3.Normalize(_forward + horizontal * _right + vertical * _up);
        return new Ray(_origin, direction);
    }
}
