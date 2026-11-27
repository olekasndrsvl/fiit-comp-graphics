using Lab6.Core.Geometry;
using Lab6.Core.Materials;
using Lab6.Core.Lighting;

namespace Lab6.Rendering;

internal sealed class PhongFragmentShader : IFragmentShader
{
    private readonly Light light;
    private readonly Point3D cameraPosition;

    public PhongFragmentShader(Light light, Point3D cameraPosition)
    {
        this.light = light;
        this.cameraPosition = cameraPosition;
    }

    public int Shade(in Fragment fragment, Material material)
    {
        var normal = fragment.WorldNormal.Normalize();

        var baseColor = VertexColor.FromColor(material.Color);

        var litColor = LightingModel.ShadePhongToon(
            baseColor,
            normal,
            fragment.WorldPosition,
            cameraPosition,
            light);

        if (fragment.Has(VertexAttributes.Texture) &&
            material.Texture is not null)
        {
            var textureColor = Color.FromArgb(
                material.Texture.SampleArgb(fragment.U, fragment.V));

            var texture = VertexColor.FromColor(textureColor);

            litColor = new VertexColor(
                litColor.R * texture.R,
                litColor.G * texture.G,
                litColor.B * texture.B);
        }

        litColor = litColor.Clamped();

        return Color.FromArgb(
            255,
            ToByte(litColor.R),
            ToByte(litColor.G),
            ToByte(litColor.B)).ToArgb();
    }

    private static int ToByte(float value) =>
        (int)Math.Round(Math.Clamp(value, 0f, 1f) * 255f);
}