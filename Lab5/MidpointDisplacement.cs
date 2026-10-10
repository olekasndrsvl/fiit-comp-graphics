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
        SelectedStep = settings.Iterations;
        Steps = []; // Точка входа для генерации и сохранения последовательных шагов.
    }

    public void SelectStep(int index) => SelectedStep = Math.Max(0, index);
}
