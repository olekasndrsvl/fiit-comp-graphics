using Lab6.Core.Materials;

namespace Lab6.Rendering;

internal readonly record struct Fragment(
    float Depth,
    double U,
    double V,
    bool HasTextureCoordinates);

internal interface IFragmentShader
{
    int Shade(in Fragment fragment, Material material);
}

internal sealed class UnlitTextureShader : IFragmentShader
{
    public int Shade(in Fragment fragment, Material material)
    {
        if (fragment.HasTextureCoordinates && material.Texture is not null)
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
