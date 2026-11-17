using Lab6.Core.Geometry;                                          
using Lab6.Core.Materials;

namespace Lab6.Rendering;
internal readonly record struct Fragment(                          
    float Depth,
    double U,
    double V,
    VertexColor Color,                                        
    Point3D WorldPosition,                                    
    Normal WorldNormal,                                          
    VertexAttributes Attributes)                                   
{
    public bool Has(VertexAttributes attribute) => (Attributes & attribute) == attribute;
}

internal interface IFragmentShader
{
    int Shade(in Fragment fragment, Material material);
}

internal sealed class UnlitTextureShader : IFragmentShader
{
    public int Shade(in Fragment fragment, Material material)
    {
        if (fragment.Has(VertexAttributes.Texture) && material.Texture is not null)        
            return material.Texture.SampleArgb(fragment.U, fragment.V);

        return material.Color.ToArgb();
    }
}

internal sealed class DepthShader(float minDepth, float maxDepth) : IFragmentShader
{
    private const float MinimumChannel = 25;
    private const float ChannelRange = 230;
    private const float MinimumDepthRange = 1e-6f;

    public int Shade(in Fragment fragment, Material material)
    {
        var range = maxDepth - minDepth;
        var normalizedDepth = range > MinimumDepthRange
            ? (fragment.Depth - minDepth) / range
            : 0.5f;
        var intensity = 1f - Math.Clamp(normalizedDepth, 0f, 1f);
        var channel = (int)(MinimumChannel + intensity * ChannelRange);

        return Color.FromArgb(channel, channel, channel).ToArgb();
    }
}

internal sealed class GouraudFragmentShader : IFragmentShader  
{
    public int Shade(in Fragment fragment, Material material)
    {
        if (!fragment.Has(VertexAttributes.Color))
            return material.Color.ToArgb();

        var color = fragment.Color;

        if (fragment.Has(VertexAttributes.Texture) && material.Texture is not null)
        {
            var texel = material.Texture.SampleArgb(fragment.U, fragment.V);
            color = new VertexColor(
                color.R * (((texel >> 16) & 0xFF) / 255f),
                color.G * (((texel >> 8) & 0xFF) / 255f),
                color.B * ((texel & 0xFF) / 255f));
        }

        return Color.FromArgb(ToByte(color.R), ToByte(color.G), ToByte(color.B)).ToArgb();
    }

    private static int ToByte(float value) =>
        (int)Math.Round(Math.Clamp(value, 0f, 1f) * 255f);
}
