namespace Lab5;

internal sealed record LSystemSettings(
    string Axiom, IReadOnlyList<string> Rules,
    int Iterations, float Angle,
    float InitialDirection, float RandomAngle,
    bool TreeMode, bool AutoScale,
    float BaseThickness, float ThicknessDecay,
    Color TrunkColor, Color BranchColor);

internal sealed class LSystemEngine
{
    public string Axiom { get; private set; } = "F";
    public Dictionary<char, string> Rules { get; } = [];
    public string Grammar { get; private set; } = "F";
    public IReadOnlyList<PointF> Points { get; private set; } = [];

    public void Load(string path)
    {
        var lines = File.ReadAllLines(path)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        if (lines.Length == 0) 
            return;
        
        Axiom = lines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
        ParseRules(lines.Skip(1));
    }

    public void Build(LSystemSettings settings)
    {
        Axiom = settings.Axiom;
        Grammar = BuildGrammar(settings);
        Points = []; // Точка входа для развёртки правил и интерпретации «черепахой».
    }

    public string BuildGrammar(LSystemSettings settings)
    {
        ParseRules(settings.Rules);

        var current = settings.Axiom;
        for (var generation = 0; generation < settings.Iterations; generation++)
        {
            var next = new System.Text.StringBuilder();
            foreach (var symbol in current)
            {
                if (Rules.TryGetValue(symbol, out var replacement))
                    next.Append(replacement);
                else
                    next.Append(symbol);
            }

            current = next.ToString();
        }

        return current;
    }

    private void ParseRules(IEnumerable<string> lines)
    {
        Rules.Clear();
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split(["->", "="], 2, StringSplitOptions.TrimEntries);
            if (parts.Length == 2 && parts[0].Length == 1)
                Rules[parts[0][0]] = parts[1];
        }
    }

    public void Clear() => Points = [];
}
