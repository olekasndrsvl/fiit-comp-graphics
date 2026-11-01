namespace Lab5;

internal sealed record MidpointSettings(
    float InitialHeight, 
    int Iterations,
    float Displacement,
    float Roughness,
    int? Seed);

internal sealed class MidpointDisplacement
{
    public IReadOnlyList<IReadOnlyList<PointF>> Steps { get; private set; } = [];

    public int SelectedStep { get; private set; }

    public void Generate(MidpointSettings settings)
    {
        int n = (int)Math.Pow(2, settings.Iterations) + 1;
        float[] height = new float[n];

        var random = settings.Seed.HasValue ? new Random(settings.Seed.Value) : new Random();

        height[0] = (float)random.NextDouble() * settings.InitialHeight;
        height[n - 1] = (float)random.NextDouble() * settings.InitialHeight;

        var steps = new List<IReadOnlyList<PointF>> { Snapshot(height) }; 

        float amplitude = settings.Displacement;
        int stepSize = n - 1;

        for(var iteration = 0; iteration < settings.Iterations; iteration++)
        {
            int half = stepSize / 2;

            for(var j = half; j < n ; j += stepSize)
            {
                float average = (height[j - half] + height[j + half]) * 0.5f;
                float noise = ((float)random.NextDouble() * 2 - 1) * amplitude;
                height[j] = average + noise;
            }

            amplitude *= settings.Roughness;
            stepSize = half;

            steps.Add(Snapshot(height));
        }

        Steps = steps;
        SelectedStep = settings.Iterations;
    }

    private static List<PointF> Snapshot(float[] height)
    {
        List<PointF> points = new List<PointF>(height.Length);

        for (int i = 0; i < height.Length; i++)
            points.Add(new PointF(i, height[i]));
        
        return points;
    }

    public void SelectStep(int index) => SelectedStep = Math.Max(0, index);
}


