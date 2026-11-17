namespace Lab6.Rendering;

/// Атрибуты вершины, которые нужно проинтерполировать по грани
[Flags]
internal enum VertexAttributes
{
    None = 0,
    Texture = 1,
    Color = 2,
    WorldPosition = 4,
    WorldNormal = 8,
    WorldSpace = WorldPosition | WorldNormal
}
