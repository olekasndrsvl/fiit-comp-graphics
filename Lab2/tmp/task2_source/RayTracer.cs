using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Task2;

internal readonly record struct ObjectRenderSettings(bool IsMirror, bool IsTransparent);

internal readonly record struct PointLightSettings(bool Enabled, Vector3 Position, float Intensity);

internal sealed record SceneSettings(
    ObjectRenderSettings[] Objects,
    PointLightSettings[] Lights,
    string? MirroredWall);

internal static class RayTracer
{
    private const float Epsilon = 0.001f;
    private const int MaxDepth = 5;

    public static Bitmap Render(
        SceneSettings settings,
        int width,
        int height,
        IProgress<int>? progress,
        CancellationToken cancellationToken)
    {
        var scene = Scene.Create(settings);
        var camera = new Camera(
            new Vector3(0f, 0.15f, 4.35f),
            new Vector3(0f, -0.45f, -2.6f),
            Vector3.UnitY,
            57f,
            (float)width / height);
        var pixels = new int[width * height];
        var completedRows = 0;

        Parallel.For(
            0,
            height,
            new ParallelOptions { CancellationToken = cancellationToken },
            y =>
            {
                for (var x = 0; x < width; x++)
                {
                    var ray = camera.GetRay(
                        (x + 0.5f) / width,
                        (y + 0.5f) / height);
                    var color = Trace(scene, ray, 0);
                    pixels[y * width + x] = ToArgb(color);
                }

                var rows = Interlocked.Increment(ref completedRows);
                if (rows == height || rows % 8 == 0)
                    progress?.Report(rows * 100 / height);
            });

        cancellationToken.ThrowIfCancellationRequested();
        return CreateBitmap(width, height, pixels);
    }

    private static Vector3 Trace(Scene scene, Ray ray, int depth)
    {
        if (!scene.FindClosest(ray, Epsilon, float.PositiveInfinity, out var hit))
            return new Vector3(0.012f, 0.014f, 0.018f);

        var localColor = ShadeSurface(scene, hit, ray);
        if (depth >= MaxDepth)
            return localColor;

        var material = hit.Material;
        var reflectionWeight = material.Reflection;
        var transmissionWeight = material.Transparency;

        if (transmissionWeight > 0f)
        {
            var cosine = MathF.Min(Vector3.Dot(-ray.Direction, hit.Normal), 1f);
            var fresnel = Schlick(cosine, material.RefractiveIndex);
            reflectionWeight += transmissionWeight * fresnel;
            transmissionWeight *= 1f - fresnel;
        }

        var surfaceWeight = MathF.Max(0f, 1f - reflectionWeight - transmissionWeight);
        var result = localColor * surfaceWeight;

        if (reflectionWeight > 0f)
        {
            var reflectedDirection = Vector3.Normalize(Vector3.Reflect(ray.Direction, hit.Normal));
            var reflectedRay = new Ray(hit.Point + hit.Normal * Epsilon, reflectedDirection);
            result += Trace(scene, reflectedRay, depth + 1) * reflectionWeight;
        }

        if (transmissionWeight > 0f)
        {
            var eta = hit.FrontFace
                ? 1f / material.RefractiveIndex
                : material.RefractiveIndex;

            if (TryRefract(ray.Direction, hit.Normal, eta, out var refractedDirection))
            {
                var refractedRay = new Ray(hit.Point - hit.Normal * Epsilon, refractedDirection);
                var transmitted = Trace(scene, refractedRay, depth + 1);
                var tint = Vector3.Lerp(Vector3.One, material.Color, 0.12f);
                result += transmitted * tint * transmissionWeight;
            }
            else
            {
                var reflectedDirection = Vector3.Normalize(Vector3.Reflect(ray.Direction, hit.Normal));
                var reflectedRay = new Ray(hit.Point + hit.Normal * Epsilon, reflectedDirection);
                result += Trace(scene, reflectedRay, depth + 1) * transmissionWeight;
            }
        }

        return result;
    }

    private static Vector3 ShadeSurface(Scene scene, HitInfo hit, Ray incomingRay)
    {
        var material = hit.Material;
        var result = material.Color * 0.045f;
        var viewDirection = -incomingRay.Direction;

        foreach (var light in scene.Lights)
        {
            if (!light.Enabled)
                continue;

            var toLight = light.Position - hit.Point;
            var distance = toLight.Length();
            if (distance <= Epsilon)
                continue;

            var lightDirection = toLight / distance;
            var diffuseFactor = MathF.Max(0f, Vector3.Dot(hit.Normal, lightDirection));
            if (diffuseFactor <= 0f)
                continue;

            var visibility = scene.GetLightVisibility(
                hit.Point + hit.Normal * Epsilon,
                lightDirection,
                distance - Epsilon);
            if (visibility <= 0f)
                continue;

            var attenuation = light.Intensity / (1f + 0.018f * distance * distance);
            var diffuse = material.Color * diffuseFactor;
            var reflectedLight = Vector3.Reflect(-lightDirection, hit.Normal);
            var specularFactor = MathF.Pow(
                MathF.Max(0f, Vector3.Dot(viewDirection, reflectedLight)),
                material.Shininess) * material.Specular;
            var specular = Vector3.One * specularFactor;

            result += (diffuse + specular) * attenuation * visibility;
        }

        return result;
    }

    private static bool TryRefract(Vector3 direction, Vector3 normal, float eta, out Vector3 refracted)
    {
        var cosTheta = MathF.Min(Vector3.Dot(-direction, normal), 1f);
        var perpendicular = eta * (direction + cosTheta * normal);
        var parallelLengthSquared = 1f - perpendicular.LengthSquared();

        if (parallelLengthSquared < 0f)
        {
            refracted = default;
            return false;
        }

        var parallel = -MathF.Sqrt(parallelLengthSquared) * normal;
        refracted = Vector3.Normalize(perpendicular + parallel);
        return true;
    }

    private static float Schlick(float cosine, float refractiveIndex)
    {
        var r0 = (1f - refractiveIndex) / (1f + refractiveIndex);
        r0 *= r0;
        return r0 + (1f - r0) * MathF.Pow(1f - cosine, 5f);
    }

    private static int ToArgb(Vector3 linearColor)
    {
        var exposed = linearColor * 1.22f;
        var mapped = new Vector3(
            exposed.X / (1f + exposed.X),
            exposed.Y / (1f + exposed.Y),
            exposed.Z / (1f + exposed.Z));
        var gamma = new Vector3(
            MathF.Pow(Math.Clamp(mapped.X, 0f, 1f), 1f / 2.2f),
            MathF.Pow(Math.Clamp(mapped.Y, 0f, 1f), 1f / 2.2f),
            MathF.Pow(Math.Clamp(mapped.Z, 0f, 1f), 1f / 2.2f));

        var red = (int)(gamma.X * 255f + 0.5f);
        var green = (int)(gamma.Y * 255f + 0.5f);
        var blue = (int)(gamma.Z * 255f + 0.5f);
        return unchecked((int)0xFF000000 | red << 16 | green << 8 | blue);
    }

    private static Bitmap CreateBitmap(int width, int height, int[] pixels)
    {
        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        var bounds = new Rectangle(0, 0, width, height);
        var data = bitmap.LockBits(bounds, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

        try
        {
            Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
        }
        finally
        {
            bitmap.UnlockBits(data);
        }

        return bitmap;
    }

    private readonly record struct Ray(Vector3 Origin, Vector3 Direction);

    private readonly struct Camera
    {
        private readonly Vector3 _origin;
        private readonly Vector3 _forward;
        private readonly Vector3 _right;
        private readonly Vector3 _up;
        private readonly float _halfWidth;
        private readonly float _halfHeight;

        public Camera(
            Vector3 origin,
            Vector3 target,
            Vector3 worldUp,
            float verticalFieldOfView,
            float aspectRatio)
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

    private sealed class Scene
    {
        private readonly List<IPrimitive> _objects = [];

        private Scene(PointLightSettings[] lights)
        {
            Lights = lights;
        }

        public PointLightSettings[] Lights { get; }

        public static Scene Create(SceneSettings settings)
        {
            var scene = new Scene(settings.Lights);
            scene.AddCornellRoom(settings.MirroredWall);

            var objectSettings = settings.Objects.Length >= 4
                ? settings.Objects
                : [new(false, false), new(false, false), new(false, false), new(false, false)];

            scene._objects.Add(new AxisAlignedBox(
                new Vector3(-3.65f, -4f, -4.65f),
                new Vector3(-1.45f, -0.65f, -2.45f),
                CreateObjectMaterial(new Vector3(0.88f, 0.48f, 0.16f), objectSettings[0])));
            scene._objects.Add(new AxisAlignedBox(
                new Vector3(1.25f, -4f, -5.05f),
                new Vector3(3.55f, -1.75f, -2.75f),
                CreateObjectMaterial(new Vector3(0.18f, 0.42f, 0.84f), objectSettings[1])));
            scene._objects.Add(new Sphere(
                new Vector3(-2.05f, -2.72f, 0.25f),
                1.28f,
                CreateObjectMaterial(new Vector3(0.86f, 0.25f, 0.18f), objectSettings[2])));
            scene._objects.Add(new Sphere(
                new Vector3(2.05f, -2.88f, -0.35f),
                1.12f,
                CreateObjectMaterial(new Vector3(0.16f, 0.72f, 0.68f), objectSettings[3])));

            return scene;
        }

        public bool FindClosest(Ray ray, float minimum, float maximum, out HitInfo closestHit)
        {
            closestHit = default;
            var found = false;
            var closestDistance = maximum;

            foreach (var item in _objects)
            {
                if (!item.Intersect(ray, minimum, closestDistance, out var hit))
                    continue;

                found = true;
                closestDistance = hit.Distance;
                closestHit = hit;
            }

            return found;
        }

        public float GetLightVisibility(Vector3 origin, Vector3 direction, float maximumDistance)
        {
            var visibility = 1f;
            var remainingDistance = maximumDistance;
            var shadowRay = new Ray(origin, direction);

            for (var step = 0; step < 6 && remainingDistance > Epsilon; step++)
            {
                if (!FindClosest(shadowRay, Epsilon, remainingDistance, out var hit))
                    break;

                if (hit.Material.Transparency <= 0f)
                    return 0f;

                visibility *= 0.58f;
                remainingDistance -= hit.Distance + Epsilon;
                shadowRay = new Ray(hit.Point + direction * Epsilon, direction);
            }

            return visibility;
        }

        private void AddCornellRoom(string? mirroredWall)
        {
            var white = new Vector3(0.76f, 0.75f, 0.71f);
            var red = new Vector3(0.78f, 0.12f, 0.09f);
            var green = new Vector3(0.08f, 0.62f, 0.22f);

            _objects.Add(new Quad(
                new Vector3(-5f, 0f, -0.5f),
                new Vector3(0f, 4f, 0f),
                new Vector3(0f, 0f, 5.5f),
                Vector3.UnitX,
                CreateWallMaterial(red, "Левая", mirroredWall)));
            _objects.Add(new Quad(
                new Vector3(5f, 0f, -0.5f),
                new Vector3(0f, 4f, 0f),
                new Vector3(0f, 0f, 5.5f),
                -Vector3.UnitX,
                CreateWallMaterial(green, "Правая", mirroredWall)));
            _objects.Add(new Quad(
                new Vector3(0f, 0f, -6f),
                new Vector3(5f, 0f, 0f),
                new Vector3(0f, 4f, 0f),
                Vector3.UnitZ,
                CreateWallMaterial(white, "Задняя", mirroredWall)));
            _objects.Add(new Quad(
                new Vector3(0f, 0f, 5f),
                new Vector3(5f, 0f, 0f),
                new Vector3(0f, 4f, 0f),
                -Vector3.UnitZ,
                CreateWallMaterial(white, "Передняя", mirroredWall)));
            _objects.Add(new Quad(
                new Vector3(0f, -4f, -0.5f),
                new Vector3(5f, 0f, 0f),
                new Vector3(0f, 0f, 5.5f),
                Vector3.UnitY,
                CreateWallMaterial(white, "Пол", mirroredWall)));
            _objects.Add(new Quad(
                new Vector3(0f, 4f, -0.5f),
                new Vector3(5f, 0f, 0f),
                new Vector3(0f, 0f, 5.5f),
                -Vector3.UnitY,
                CreateWallMaterial(white, "Потолок", mirroredWall)));
        }

        private static Material CreateObjectMaterial(Vector3 color, ObjectRenderSettings settings)
        {
            var reflection = 0f;
            var transparency = 0f;

            if (settings is { IsMirror: true, IsTransparent: true })
            {
                reflection = 0.28f;
                transparency = 0.62f;
            }
            else if (settings.IsMirror)
            {
                reflection = 0.78f;
            }
            else if (settings.IsTransparent)
            {
                transparency = 0.86f;
            }

            return new Material(
                color,
                reflection,
                transparency,
                1.5f,
                settings.IsMirror || settings.IsTransparent ? 0.85f : 0.28f,
                settings.IsMirror || settings.IsTransparent ? 80f : 30f);
        }

        private static Material CreateWallMaterial(Vector3 color, string name, string? mirroredWall)
        {
            var isMirror = string.Equals(name, mirroredWall, StringComparison.Ordinal);
            return new Material(
                color,
                isMirror ? 0.82f : 0f,
                0f,
                1.5f,
                isMirror ? 0.9f : 0.18f,
                isMirror ? 100f : 18f);
        }
    }

    private sealed record Material(
        Vector3 Color,
        float Reflection,
        float Transparency,
        float RefractiveIndex,
        float Specular,
        float Shininess);

    private readonly struct HitInfo
    {
        public HitInfo(Ray ray, float distance, Vector3 outwardNormal, Material material)
        {
            Distance = distance;
            Point = ray.Origin + ray.Direction * distance;
            FrontFace = Vector3.Dot(ray.Direction, outwardNormal) < 0f;
            Normal = FrontFace ? outwardNormal : -outwardNormal;
            Material = material;
        }

        public float Distance { get; }
        public Vector3 Point { get; }
        public Vector3 Normal { get; }
        public bool FrontFace { get; }
        public Material Material { get; }
    }

    private interface IPrimitive
    {
        bool Intersect(Ray ray, float minimum, float maximum, out HitInfo hit);
    }

    private sealed class Sphere(Vector3 center, float radius, Material material) : IPrimitive
    {
        public bool Intersect(Ray ray, float minimum, float maximum, out HitInfo hit)
        {
            var originToCenter = ray.Origin - center;
            var halfB = Vector3.Dot(originToCenter, ray.Direction);
            var c = originToCenter.LengthSquared() - radius * radius;
            var discriminant = halfB * halfB - c;

            if (discriminant < 0f)
            {
                hit = default;
                return false;
            }

            var squareRoot = MathF.Sqrt(discriminant);
            var distance = -halfB - squareRoot;
            if (distance < minimum || distance > maximum)
            {
                distance = -halfB + squareRoot;
                if (distance < minimum || distance > maximum)
                {
                    hit = default;
                    return false;
                }
            }

            var point = ray.Origin + ray.Direction * distance;
            var outwardNormal = (point - center) / radius;
            hit = new HitInfo(ray, distance, outwardNormal, material);
            return true;
        }
    }

    private sealed class AxisAlignedBox(Vector3 minimumBounds, Vector3 maximumBounds, Material material)
        : IPrimitive
    {
        public bool Intersect(Ray ray, float minimum, float maximum, out HitInfo hit)
        {
            var nearDistance = float.NegativeInfinity;
            var farDistance = float.PositiveInfinity;
            var nearNormal = Vector3.Zero;
            var farNormal = Vector3.Zero;

            for (var axis = 0; axis < 3; axis++)
            {
                var origin = Component(ray.Origin, axis);
                var direction = Component(ray.Direction, axis);
                var lower = Component(minimumBounds, axis);
                var upper = Component(maximumBounds, axis);

                if (MathF.Abs(direction) < 1e-7f)
                {
                    if (origin < lower || origin > upper)
                    {
                        hit = default;
                        return false;
                    }

                    continue;
                }

                var firstDistance = (lower - origin) / direction;
                var secondDistance = (upper - origin) / direction;
                var firstNormal = AxisNormal(axis, -1f);
                var secondNormal = AxisNormal(axis, 1f);

                if (firstDistance > secondDistance)
                {
                    (firstDistance, secondDistance) = (secondDistance, firstDistance);
                    (firstNormal, secondNormal) = (secondNormal, firstNormal);
                }

                if (firstDistance > nearDistance)
                {
                    nearDistance = firstDistance;
                    nearNormal = firstNormal;
                }

                if (secondDistance < farDistance)
                {
                    farDistance = secondDistance;
                    farNormal = secondNormal;
                }

                if (nearDistance > farDistance)
                {
                    hit = default;
                    return false;
                }
            }

            var distance = nearDistance >= minimum ? nearDistance : farDistance;
            var outwardNormal = nearDistance >= minimum ? nearNormal : farNormal;
            if (distance < minimum || distance > maximum)
            {
                hit = default;
                return false;
            }

            hit = new HitInfo(ray, distance, outwardNormal, material);
            return true;
        }

        private static float Component(Vector3 value, int axis) => axis switch
        {
            0 => value.X,
            1 => value.Y,
            _ => value.Z
        };

        private static Vector3 AxisNormal(int axis, float direction) => axis switch
        {
            0 => new Vector3(direction, 0f, 0f),
            1 => new Vector3(0f, direction, 0f),
            _ => new Vector3(0f, 0f, direction)
        };
    }

    private sealed class Quad(
        Vector3 center,
        Vector3 halfAxisU,
        Vector3 halfAxisV,
        Vector3 outwardNormal,
        Material material) : IPrimitive
    {
        public bool Intersect(Ray ray, float minimum, float maximum, out HitInfo hit)
        {
            var denominator = Vector3.Dot(ray.Direction, outwardNormal);
            if (MathF.Abs(denominator) < 1e-7f)
            {
                hit = default;
                return false;
            }

            var distance = Vector3.Dot(center - ray.Origin, outwardNormal) / denominator;
            if (distance < minimum || distance > maximum)
            {
                hit = default;
                return false;
            }

            var point = ray.Origin + ray.Direction * distance;
            var relative = point - center;
            var u = Vector3.Dot(relative, halfAxisU) / halfAxisU.LengthSquared();
            var v = Vector3.Dot(relative, halfAxisV) / halfAxisV.LengthSquared();

            if (MathF.Abs(u) > 1.0001f || MathF.Abs(v) > 1.0001f)
            {
                hit = default;
                return false;
            }

            hit = new HitInfo(ray, distance, outwardNormal, material);
            return true;
        }
    }
}
