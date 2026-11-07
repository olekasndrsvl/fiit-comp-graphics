namespace Lab6.Core.Geometry;

/// <summary>Текстурная координата Wavefront OBJ. Компоненты V и W необязательны.</summary>
public readonly record struct TextureCoordinate(double U, double? V = null, double? W = null);
