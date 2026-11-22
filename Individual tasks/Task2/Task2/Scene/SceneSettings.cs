namespace Task2;

internal readonly record struct ObjectRenderSettings(bool IsMirror, bool IsTransparent);

internal readonly record struct PointLightSettings(bool Enabled, Point3 Position, float Intensity);

internal sealed record SceneSettings(ObjectRenderSettings[] Objects, PointLightSettings[] Lights, string? MirroredWall);
