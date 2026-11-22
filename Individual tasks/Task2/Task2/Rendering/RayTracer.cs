using System.Numerics;

namespace Task2;

internal static class RayTracer
{
    private const float Epsilon = RayTracingConstants.Epsilon;
    private const int MaxDepth = 15;

    public static ColorRgb Trace(Scene scene, Ray ray, int depth = 0)
    {
        if (!scene.FindClosest(ray, Epsilon, float.PositiveInfinity, out var hit))
            return new ColorRgb(0.012f, 0.014f, 0.018f);

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
                var tint = ColorRgb.Lerp(ColorRgb.White, material.Color, 0.12f);
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

    private static ColorRgb ShadeSurface(Scene scene, HitInfo hit, Ray incomingRay)
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
            var specular = ColorRgb.White * specularFactor;

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
}
