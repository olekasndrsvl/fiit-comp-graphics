namespace Lab6.Core.Geometry;

/// <summary>
/// Индекс вершины грани в массиве точек многогранника.
/// Внутри приложения все индексы начинаются с нуля.
/// </summary>
public readonly record struct FaceVertex(int PositionIndex);
