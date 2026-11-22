using System.Numerics;

namespace Task2;

internal sealed record Material(Vector3 Color, float Reflection, float Transparency, float RefractiveIndex, float Specular, float Shininess);
