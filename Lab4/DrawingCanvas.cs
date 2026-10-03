using System.ComponentModel;

namespace Lab4;

public sealed class DrawingCanvas : Panel
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<PolygonShape> Polygons { get; } = [];

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public PolygonShape? CurrentPolygon { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int SelectedIndex { get; set; } = -1;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<PointF> QueryPoints { get; } = [];

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public (PointF A, PointF B)? ReferenceEdge { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public (PointF A, PointF B)? SecondEdge { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public PointF? ResultPoint { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public PointF? PivotMarker { get; set; }

    public DrawingCanvas()
    {
        DoubleBuffered = true;
        BackColor = Color.White;
        BorderStyle = BorderStyle.FixedSingle;
        Cursor = Cursors.Cross;
        ResizeRedraw = true;
    }

    public void ClearScene()
    {
        Polygons.Clear();
        CurrentPolygon = null;
        SelectedIndex = -1;
        QueryPoints.Clear();
        ReferenceEdge = SecondEdge = null;
        ResultPoint = PivotMarker = null;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        for (var i = 0; i < Polygons.Count; i++)
        {
            DrawPolygon(e.Graphics, Polygons[i], i == SelectedIndex, false);
        }

        if (CurrentPolygon is not null)
        {
            DrawPolygon(e.Graphics, CurrentPolygon, true, true);
        }

        DrawEdge(e.Graphics, ReferenceEdge, Color.DarkOrange);
        DrawEdge(e.Graphics, SecondEdge, Color.MediumPurple);
        foreach (var point in QueryPoints)
        {
            DrawMarker(e.Graphics, point, Color.Crimson);
        }

        if (PivotMarker is { } pivot)
        {
            DrawMarker(e.Graphics, pivot, Color.DarkGreen);
        }

        if (ResultPoint is { } result)
        {
            DrawMarker(e.Graphics, result, Color.Blue);
        }
    }

    private static void DrawPolygon(Graphics g, PolygonShape polygon, bool selected, bool open)
    {
        var points = polygon.Vertices.ToArray();
        if (points.Length == 0)
        {
            return;
        }

        using var pen = new Pen(selected ? Color.RoyalBlue : Color.FromArgb(55, 75, 90), selected ? 2.5f : 1.7f);
        if (points.Length > 1)
        {
            g.DrawLines(pen, points);
        }

        if (!open && points.Length > 2)
        {
            g.DrawLine(pen, points[^1], points[0]);
        }

        foreach (var point in points)
        {
            DrawMarker(g, point, selected ? Color.RoyalBlue : Color.Black);
        }
    }

    private static void DrawEdge(Graphics g, (PointF A, PointF B)? edge, Color color)
    {
        if (edge is not { } value)
        {
            return;
        }

        using var pen = new Pen(color, 2);
        g.DrawLine(pen, value.A, value.B);
        DrawMarker(g, value.A, color);
        DrawMarker(g, value.B, color);
    }

    private static void DrawMarker(Graphics g, PointF point, Color color)
    {
        using var brush = new SolidBrush(color);
        g.FillEllipse(brush, point.X - 4, point.Y - 4, 8, 8);
    }
}
