using System.Drawing.Drawing2D;

namespace Lab5;

internal sealed record LSystemSettings(
    string Axiom, IReadOnlyList<string> Rules,
    int Iterations, float Angle,
    float InitialDirection, float RandomAngle,
    bool TreeMode, bool AutoScale,
    float BaseThickness, float ThicknessDecay,
    Color TrunkColor, Color BranchColor);

// для дерева из 1 б
internal readonly record struct LSystemSegment(
    PointF Start,
    PointF End, 
    float Thickness,
    Color Color);

internal sealed class LSystemEngine
{
    private const float TurtleStep = 10f;

    public string Axiom { get; private set; } = "F";
    public Dictionary<char, string> Rules { get; } = [];
    public string Grammar { get; private set; } = "F";
    public IReadOnlyList<LSystemSegment> Segments { get; private set; } = [];
    public bool AutoScale { get; private set; } = true;

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
        AutoScale = settings.AutoScale;
        Segments = BuildTurtle(settings);
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

    public IReadOnlyList<LSystemSegment> BuildTurtle(LSystemSettings settings)
    {
        var segments = new List<LSystemSegment>();
        var states = new Stack<TurtleState>();
        var position = PointF.Empty;
        var direction = settings.InitialDirection;
        var depth = 0;
        var maxDepth = GetMaximumBranchDepth(Grammar);

        foreach (var command in Grammar)
        {
            switch (command)
            {
                case '+':
                    direction += GetTurnAngle(settings);
                    break;
                case '-':
                    direction -= GetTurnAngle(settings);
                    break;
                case '[':
                    states.Push(new TurtleState(position, direction, depth));
                    depth++;
                    break;
                case ']' when states.Count > 0:
                {
                    var state = states.Pop();
                    position = state.Position;
                    direction = state.Direction;
                    depth = state.Depth;
                    break;
                }
                default:
                    if (char.IsLetter(command))
                    {
                        var next = Move(position, direction);
                        var thickness = settings.TreeMode
                            ? Math.Max(1f, settings.BaseThickness * MathF.Pow(settings.ThicknessDecay, depth))
                            : 1f;
                        var color = settings.TreeMode
                            ? MixColors(settings.TrunkColor, settings.BranchColor,
                                maxDepth == 0 ? 0f : (float)depth / maxDepth)
                            : Color.Black;

                        segments.Add(new LSystemSegment(position, next, thickness, color));
                        position = next;
                    }
                    break;
            }
        }

        return segments;
    }

    public void Draw(Graphics graphics, Rectangle bounds)
    {
        if (Segments.Count == 0 || bounds.Width <= 0 || bounds.Height <= 0)
            return;

        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        const float padding = 20f;
        var minX = Segments.Min(s => Math.Min(s.Start.X, s.End.X));
        var maxX = Segments.Max(s => Math.Max(s.Start.X, s.End.X));
        var minY = Segments.Min(s => Math.Min(s.Start.Y, s.End.Y));
        var maxY = Segments.Max(s => Math.Max(s.Start.Y, s.End.Y));

        using var matrix = new Matrix();

        if (AutoScale)
        {
            var width = maxX - minX;
            var height = maxY - minY;
            var availableWidth = Math.Max(1f, bounds.Width - 2 * padding);
            var availableHeight = Math.Max(1f, bounds.Height - 2 * padding);
            var scaleX = width > 0 ? availableWidth / width : float.MaxValue;
            var scaleY = height > 0 ? availableHeight / height : float.MaxValue;
            var scale = Math.Min(scaleX, scaleY);

            if (!float.IsFinite(scale))
                scale = 1f;

            matrix.Translate(-(minX + maxX) / 2f, -(minY + maxY) / 2f, MatrixOrder.Append);
            matrix.Scale(scale, scale, MatrixOrder.Append);
            matrix.Translate(bounds.Left + bounds.Width / 2f, bounds.Top + bounds.Height / 2f, MatrixOrder.Append);
        }
        else
        {
            matrix.Translate(bounds.Left + bounds.Width / 2f, bounds.Bottom - padding);
        }

        foreach (var segment in Segments)
        {
            PointF[] points = [segment.Start, segment.End];
            matrix.TransformPoints(points);

            using var pen = new Pen(segment.Color, segment.Thickness)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };

            graphics.DrawLine(pen, points[0], points[1]);
        }
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

    private static PointF Move(PointF position, float direction)
    {
        var radians = direction * MathF.PI / 180f;
        return new PointF(
            position.X + MathF.Cos(radians) * TurtleStep,
            position.Y + MathF.Sin(radians) * TurtleStep);
    }

    private static float GetTurnAngle(LSystemSettings settings) => settings.Angle + (settings.RandomAngle == 0 ? 0 : (Random.Shared.NextSingle() * 2f - 1f) * settings.RandomAngle);

    private static int GetMaximumBranchDepth(string grammar)
    {
        var depth = 0;
        var maximum = 0;
        foreach (var symbol in grammar)
        {
            if (symbol == '[')
                maximum = Math.Max(maximum, ++depth);
            else if (symbol == ']' && depth > 0)
                depth--;
        }

        return maximum;
    }

    private static Color MixColors(Color start, Color end, float amount)
    {
        amount = Math.Clamp(amount, 0f, 1f);
        return Color.FromArgb(
            (int)(start.R + (end.R - start.R) * amount),
            (int)(start.G + (end.G - start.G) * amount),
            (int)(start.B + (end.B - start.B) * amount));
    }

    public void Clear()
    {
        Grammar = string.Empty;
        Segments = [];
    }

    private readonly record struct TurtleState(PointF Position, float Direction, int Depth);
}
