using System.Drawing.Drawing2D;

namespace Task1;

public partial class Form1 : Form
{
    private readonly ConvexHull _convexHull = new();
    private Point[] _points = [];
    private Point[] _hull = [];

    public Form1()
    {
        InitializeComponent();
        UpdateInterface();
    }

    private void DrawingPanel_MouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
            return;

        _convexHull.Add(e.Location);
        _points = _convexHull.Points.ToArray();
        _hull = [];
        UpdateInterface("Точка добавлена. Постройте оболочку заново.");
        drawingPanel.Invalidate();
    }

    private void BuildButton_Click(object? sender, EventArgs e)
    {
        _points = _convexHull.Points.ToArray();
        _hull = _convexHull.Build();

        string message = _points.Length < 3
            ? "Для оболочки нужно не менее трёх различных точек."
            : $"Готово: в оболочке {_hull.Length} вершин.";

        UpdateInterface(message);
        drawingPanel.Invalidate();
    }

    private void ClearButton_Click(object? sender, EventArgs e)
    {
        _convexHull.Clear();
        _points = [];
        _hull = [];
        UpdateInterface("Поле очищено. Добавьте новые точки.");
        drawingPanel.Invalidate();
    }

    private void DrawingPanel_Paint(object? sender, PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        if (_hull.Length >= 2)
        {
            using var hullFill = new SolidBrush(Color.FromArgb(28, 37, 99, 235));
            using var hullPen = new Pen(Color.FromArgb(37, 99, 235), 3f)
            {
                LineJoin = LineJoin.Round
            };

            if (_hull.Length >= 3)
                e.Graphics.FillPolygon(hullFill, _hull);

            if (_hull.Length == 2)
                e.Graphics.DrawLine(hullPen, _hull[0], _hull[1]);
            else
                e.Graphics.DrawPolygon(hullPen, _hull);
        }

        foreach (Point point in _points)
        {
            const int radius = 5;
            var pointBounds = new Rectangle(
                point.X - radius,
                point.Y - radius,
                radius * 2,
                radius * 2);

            bool isHullPoint = Array.IndexOf(_hull, point) >= 0;
            using var brush = new SolidBrush(isHullPoint
                ? Color.FromArgb(29, 78, 216)
                : Color.FromArgb(30, 41, 59));

            e.Graphics.FillEllipse(brush, pointBounds);
            e.Graphics.DrawEllipse(Pens.White, pointBounds);
        }
    }

    private void UpdateInterface(string? status = null)
    {
        pointsValueLabel.Text = $"Выбрано точек: {_points.Length}";
        hullValueLabel.Text = $"Вершин оболочки: {_hull.Length}";
        buildButton.Enabled = _points.Length > 0;
        clearButton.Enabled = _points.Length > 0;

        if (status is not null)
            statusLabel.Text = status;
    }
}
