namespace Lab6.Rendering;

public sealed class ZBuffer
{
    private float[] depths = [];
    private int width;

    public void Initialize(Size viewport)
    {
        width = viewport.Width;
        var bufferSize = viewport.Width * viewport.Height;

        if (depths.Length != bufferSize)
            depths = new float[bufferSize];

        Array.Fill(depths, float.PositiveInfinity);
    }

    public bool TestAndSet(int x, int y, float depth)
    {
        var index = y * width + x;
        if (depth >= depths[index])
            return false;

        depths[index] = depth;
        return true;
    }
}
