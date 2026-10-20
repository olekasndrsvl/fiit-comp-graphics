using System.Drawing.Drawing2D;

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
        _lDrawingSymbols.Text = _lSystem.DrawingSymbols;
        _lAngle.Value = Math.Clamp((decimal)_lSystem.Angle, _lAngle.Minimum, _lAngle.Maximum);
        _lDirection.Value = Math.Clamp(
            (decimal)_lSystem.InitialDirection, _lDirection.Minimum, _lDirection.Maximum);
        _lRules.Lines = _lSystem.Rules.Select(rule => $"{rule.Key}={rule.Value}").ToArray();
    }

    private void BuildLSystem_Click(object? sender, EventArgs e)
    {
        var settings = new LSystemSettings(
            _lAxiom.Text,
            _lRules.Lines,
            _lDrawingSymbols.Text,
            (int)_lIterations.Value,
            (float)_lAngle.Value,
            (float)_lDirection.Value,
            _treeMode.Checked && _randomBranches.Checked ? (float)_lRandomness.Value : 0,
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

    private void LSystemCanvas_Paint(object? sender, PaintEventArgs e) =>
        _lSystem.Draw(e.Graphics, _lCanvas.ClientRectangle);

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

    private void RefreshBezierCanvas(object? sender, EventArgs e) => _bezierCanvas.Invalidate();

    private void BezierCanvas_Paint(object? sender, PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var closed = _closeBezier.Checked;
        var segments = _bezier.BuildSegments(closed);

        if (_showBezierLines.Checked)
            DrawBezierConstruction(graphics, segments, closed);

        var polyline = _bezier.BuildPolyline(closed, (int)_bezierQuality.Value);
        if (polyline.Count >= 2)
        {
            using var curvePen = new Pen(Color.Crimson, 2f)
            {
                LineJoin = LineJoin.Round,
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };
            graphics.DrawLines(curvePen, polyline.ToArray());
        }

        if (_showBezierPoints.Checked)
        {
            foreach (var point in _bezier.Points)
                DrawHandle(graphics, point, Color.Black, 4.5f);
        }
    }

    // Вспомогательные линии: контрольный многоугольник Безье и вычисленные касательные точки.
    private static void DrawBezierConstruction(Graphics graphics, List<BezierSegment> segments, bool closed)
    {
        if (segments.Count == 0)
            return;

        var polygon = new List<PointF>();
        foreach (var segment in segments)
        {
            polygon.Add(segment.P0);
            polygon.Add(segment.P1);
            polygon.Add(segment.P2);
        }

        // Последняя точка P3 замыкает многоугольник (совпадает с P0 у замкнутой кривой).
        polygon.Add(segments[^1].P3);

        using var controlPen = new Pen(Color.SteelBlue, 1f) { DashStyle = DashStyle.Dash };
        using var tangentBrush = new SolidBrush(Color.SteelBlue);

        if (polygon.Count >= 2)
            graphics.DrawLines(controlPen, polygon.ToArray());

        foreach (var segment in segments)
        {
            graphics.FillRectangle(tangentBrush, segment.P1.X - 2.5f, segment.P1.Y - 2.5f, 5f, 5f);
            graphics.FillRectangle(tangentBrush, segment.P2.X - 2.5f, segment.P2.Y - 2.5f, 5f, 5f);
        }
    }

    private static void DrawHandle(Graphics graphics, PointF point, Color color, float radius)
    {
        using var brush = new SolidBrush(color);
        using var pen = new Pen(Color.White, 1.5f);
        graphics.FillEllipse(brush, point.X - radius, point.Y - radius, radius * 2f, radius * 2f);
        graphics.DrawEllipse(pen, point.X - radius, point.Y - radius, radius * 2f, radius * 2f);
    }

    private void UpdateStepLabel() =>
        _stepLabel.Text = $"Шаг {_stepTrack.Value} из {_stepTrack.Maximum}";

    private static Panel CreateBezierCanvas() => new BezierCanvasPanel
    {
        BackColor = Color.White,
        BorderStyle = BorderStyle.FixedSingle,
        Dock = DockStyle.Fill,
        Margin = new Padding(3)
    };

    // Панель с двойной буферизацией — для плавного перетаскивания опорных точек.
    private sealed class BezierCanvasPanel : Panel
    {
        public BezierCanvasPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }
    }

    private void MidCanvas_Paint(object? sender, PaintEventArgs e)
    {
        var steps = _midpoint.Steps;
        if (steps.Count == 0) return;

        int idx = Math.Clamp(_midpoint.SelectedStep, 0, steps.Count - 1);
        var polyline = steps[idx];
        if (polyline.Count < 2) return;

        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Color.White);

        float minY = polyline.Min(p => p.Y);
        float maxY = polyline.Max(p => p.Y);
        if (maxY - minY < 1e-3f) maxY = minY + 1f;

        const float margin = 20f;
        float w = _midCanvas.ClientSize.Width - margin * 2;
        float h = _midCanvas.ClientSize.Height - margin * 2;
        float scaleX = w / (polyline.Count - 1);
        float scaleY = h / (maxY - minY);

        var pts = new PointF[polyline.Count];
        for (int i = 0; i < polyline.Count; i++)
        {
            pts[i] = new PointF(
                margin + polyline[i].X * scaleX,
                margin + (maxY - polyline[i].Y) * scaleY);
        }

        using (var fill = new SolidBrush(Color.FromArgb(60, 70, 130, 180)))
        {
            var poly = new List<PointF>(pts);
            poly.Add(new PointF(pts[^1].X, margin + h));
            poly.Add(new PointF(pts[0].X, margin + h));
            g.FillPolygon(fill, poly.ToArray());
        }

        using var pen = new Pen(Color.SteelBlue, 2f);
        g.DrawLines(pen, pts);

        using var dot = new SolidBrush(Color.Firebrick);
        foreach (var p in pts)
            g.FillEllipse(dot, p.X - 2, p.Y - 2, 4, 4);
    }
}
