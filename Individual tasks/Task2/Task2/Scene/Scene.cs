using System.Numerics;

namespace Task2;

internal sealed class Scene
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
            new Point3(-3.65f, -4f, -4.65f),
            new Point3(-1.45f, -0.65f, -2.45f),
            CreateObjectMaterial(new ColorRgb(0.88f, 0.48f, 0.16f), objectSettings[0])));
        scene._objects.Add(new AxisAlignedBox(
            new Point3(1.25f, -4f, -5.05f),
            new Point3(3.55f, -1.75f, -2.75f),
            CreateObjectMaterial(new ColorRgb(0.18f, 0.42f, 0.84f), objectSettings[1])));
        scene._objects.Add(new Sphere(
            new Point3(-2.05f, -2.72f, 0.25f),
            1.28f,
            CreateObjectMaterial(new ColorRgb(0.86f, 0.25f, 0.18f), objectSettings[2])));
        scene._objects.Add(new Sphere(
            new Point3(2.05f, -2.88f, -0.35f),
            1.12f,
            CreateObjectMaterial(new ColorRgb(0.16f, 0.72f, 0.68f), objectSettings[3])));

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

    public float GetLightVisibility(Point3 origin, Vector3 direction, float maximumDistance)
    {
        var visibility = 1f;
        var remainingDistance = maximumDistance;
        var shadowRay = new Ray(origin, direction);

        for (var step = 0; step < 6 && remainingDistance > RayTracingConstants.Epsilon; step++)
        {
            if (!FindClosest(shadowRay, RayTracingConstants.Epsilon, remainingDistance, out var hit))
                break;

            if (hit.Material.Transparency <= 0f)
                return 0f;

            visibility *= hit.Material.Transparency;
            remainingDistance -= hit.Distance + RayTracingConstants.Epsilon;
            shadowRay = new Ray(hit.Point + direction * RayTracingConstants.Epsilon, direction);
        }

        return visibility;
    }

    private void AddCornellRoom(string? mirroredWall)
    {
        var white = new ColorRgb(0.76f, 0.75f, 0.71f);
        var red = new ColorRgb(0.78f, 0.12f, 0.09f);
        var green = new ColorRgb(0.08f, 0.62f, 0.22f);

        _objects.Add(new Quad(
            new Point3(-5f, 0f, -0.5f),
            new Vector3(0f, 4f, 0f),
            new Vector3(0f, 0f, 5.5f),
            Vector3.UnitX,
            CreateWallMaterial(red, "Левая", mirroredWall)));
        _objects.Add(new Quad(
            new Point3(5f, 0f, -0.5f),
            new Vector3(0f, 4f, 0f),
            new Vector3(0f, 0f, 5.5f),
            -Vector3.UnitX,
            CreateWallMaterial(green, "Правая", mirroredWall)));
        _objects.Add(new Quad(
            new Point3(0f, 0f, -6f),
            new Vector3(5f, 0f, 0f),
            new Vector3(0f, 4f, 0f),
            Vector3.UnitZ,
            CreateWallMaterial(white, "Задняя", mirroredWall)));
        _objects.Add(new Quad(
            new Point3(0f, 0f, 5f),
            new Vector3(5f, 0f, 0f),
            new Vector3(0f, 4f, 0f),
            -Vector3.UnitZ,
            CreateWallMaterial(white, "Передняя", mirroredWall)));
        _objects.Add(new Quad(
            new Point3(0f, -4f, -0.5f),
            new Vector3(5f, 0f, 0f),
            new Vector3(0f, 0f, 5.5f),
            Vector3.UnitY,
            CreateWallMaterial(white, "Пол", mirroredWall)));
        _objects.Add(new Quad(
            new Point3(0f, 4f, -0.5f),
            new Vector3(5f, 0f, 0f),
            new Vector3(0f, 0f, 5.5f),
            -Vector3.UnitY,
            CreateWallMaterial(white, "Потолок", mirroredWall)));
    }

    private static Material CreateObjectMaterial(ColorRgb color, ObjectRenderSettings settings)
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

    private static Material CreateWallMaterial(ColorRgb color, string name, string? mirroredWall)
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
