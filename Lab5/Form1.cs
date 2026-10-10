namespace Lab5;

public partial class Form1 : Form
{
    private readonly LSystemEngine _lSystem = new();
    private readonly MidpointDisplacement _midpoint = new();
    private readonly BezierSpline _bezier = new();

    public Form1() => InitializeComponent();

    private void OpenLSystem_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Выберите описание L-системы",
            Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        _lSystem.Load(dialog.FileName);
        _fileLabel.Text = Path.GetFileName(dialog.FileName);
        _fileLabel.ForeColor = Color.ForestGreen;
        _lAxiom.Text = _lSystem.Axiom;
        _lRules.Lines = _lSystem.Rules.Select(rule => $"{rule.Key}={rule.Value}").ToArray();
    }

    private void BuildLSystem_Click(object? sender, EventArgs e)
    {
        var settings = new LSystemSettings(
            _lAxiom.Text,
            _lRules.Lines,
            (int)_lIterations.Value,
            (float)_lAngle.Value,
            (float)_lDirection.Value,
            _randomBranches.Checked ? (float)_lRandomness.Value : 0,
            _treeMode.Checked,
            _autoScale.Checked,
            (float)_treeBaseThickness.Value,
            (float)_treeThicknessDecay.Value / 100f,
            Color.SaddleBrown,
            Color.ForestGreen);

        _lSystem.Build(settings);
        _lCanvas.Invalidate();
    }

    private void ClearLSystem_Click(object? sender, EventArgs e)
    {
        _lSystem.Clear();
        _lCanvas.Invalidate();
    }

    private void LSystemOptionChanged(object? sender, EventArgs e)
    {
        _randomBranches.Enabled = _treeMode.Checked;
        _treeBaseThickness.Enabled = _treeMode.Checked;
        _treeThicknessDecay.Enabled = _treeMode.Checked;
        _lCanvas.Invalidate();
    }

    private void GenerateTerrain_Click(object? sender, EventArgs e)
    {
        var initialHeight = float.TryParse(_midInitialHeight.Text, out var value) ? value : 0.5f;
        var settings = new MidpointSettings(
            initialHeight,
            (int)_midIterations.Value,
            (float)_midDisplacement.Value,
            (float)_midRoughness.Value / 100f,
            _midFixedSeed.Checked ? (int)_midSeed.Value : null);

        _midpoint.Generate(settings);
        _stepTrack.Maximum = (int)_midIterations.Value;
        _stepTrack.Value = _stepTrack.Maximum;
        UpdateStepLabel();
        _midCanvas.Invalidate();
    }

    private void TerrainStep_ValueChanged(object? sender, EventArgs e)
    {
        _midpoint.SelectStep(_stepTrack.Value);
        UpdateStepLabel();
        _midCanvas.Invalidate();
    }

    private void PreviousTerrainStep_Click(object? sender, EventArgs e) =>
        _stepTrack.Value = Math.Max(_stepTrack.Minimum, _stepTrack.Value - 1);

    private void NextTerrainStep_Click(object? sender, EventArgs e) =>
        _stepTrack.Value = Math.Min(_stepTrack.Maximum, _stepTrack.Value + 1);

    private void ClearBezier_Click(object? sender, EventArgs e)
    {
        _bezier.Clear();
        _bezierCanvas.Invalidate();
    }

    private void AddBezierExample_Click(object? sender, EventArgs e)
    {
        _bezier.SetPoints(
        [
            new PointF(120, 360), new PointF(260, 160),
            new PointF(440, 420), new PointF(620, 220)
        ]);
        _bezierCanvas.Invalidate();
    }

    private void BezierCanvas_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left) _bezier.BeginEdit(e.Location);
        if (e.Button == MouseButtons.Right) _bezier.RemoveNearest(e.Location);
        _bezierCanvas.Invalidate();
    }

    private void BezierCanvas_MouseMove(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        _bezier.MoveActive(e.Location);
        _bezierCanvas.Invalidate();
    }

    private void BezierCanvas_MouseUp(object? sender, MouseEventArgs e) => _bezier.EndEdit();

    private void UpdateStepLabel() =>
        _stepLabel.Text = $"Шаг {_stepTrack.Value} из {_stepTrack.Maximum}";
}
