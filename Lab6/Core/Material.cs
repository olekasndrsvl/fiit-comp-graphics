namespace Lab6.Core.Materials;

public sealed class Material
{
    public Color Color { get; set; } = Color.LightGray;
    public Texture2D? Texture { get; set; }
}
