namespace Lab6.Core.Geometry;

/// <summary>
/// Индексы позиции, текстурной координаты и нормали вершины грани.
/// Внутри приложения все индексы начинаются с нуля.
/// </summary>
public readonly record struct FaceVertex(int PositionIndex, int? TextureCoordinateIndex = null, int? NormalIndex = null);
