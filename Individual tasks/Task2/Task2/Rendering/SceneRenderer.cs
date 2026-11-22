using System.Numerics;

namespace Task2;

internal static class SceneRenderer
{
    public static Bitmap Render(SceneSettings settings, int width, int height, IProgress<int>? progress, CancellationToken cancellationToken)
    {
        var scene = Scene.Create(settings);
        var aspectRatio = (float)width / height;
        const float fieldOfView = 70f;

        var verticalFieldOfView = aspectRatio >= 1f
            ? fieldOfView
            : 2f * MathF.Atan(MathF.Tan(fieldOfView * MathF.PI / 360f) / aspectRatio)
                * 180f / MathF.PI;
        var camera = new Camera(
            new Point3(0f, 2.8f, 4.7f),
            new Point3(0f, -1.8f, -1.5f),
            Vector3.UnitY,
            verticalFieldOfView,
            aspectRatio);
        return BitmapRenderer.Render(scene, camera, width, height, progress, cancellationToken);
    }
}
