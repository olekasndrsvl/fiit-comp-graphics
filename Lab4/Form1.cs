using System.Drawing.Drawing2D;
using System.Globalization;

namespace Lab4;

public partial class Form1 : Form
{
    private enum Tool
    {
        Draw,
        Intersect,
        Inside,
        Side,
        Pivot
    }

    private Tool tool = Tool.Draw;
    private readonly List<PointF> edgeClicks = [];
    private (PointF A, PointF B)? intersectionReference;
    private readonly AffineTransformService affine = new();
    private readonly PolygonContainmentService containment = new();
    private readonly SegmentGeometryService segmentGeometry = new();

    public Form1() => InitializeComponent();

    private void Canvas_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left || !canvas.ClientRectangle.Contains(e.Location))
            return;

        var point = new PointF(e.X, e.Y);
        switch (tool)
        {
            case Tool.Draw:
                AddPolygonVertex(point);
                break;

            case Tool.Pivot:
                pivotX.Text = e.X.ToString();
                pivotY.Text = e.Y.ToString();
                canvas.PivotMarker = point;
                SetTool(Tool.Draw);
                SetStatus($"Точка преобразования: ({e.X}, {e.Y}).");
                break;

            case Tool.Intersect:
                HandleIntersectionClick(point);
                break;

            case Tool.Inside:
                canvas.QueryPoints.Add(point);
                if (SelectedPolygon() is { } polygon)
                {

                    var algorithm = SelectedContainmentAlgorithm();
                    var contains = containment.ContainsPoint(polygon, point, algorithm);
                    var relation = contains ? "внутри" : "снаружи";
                    SetStatus($"Точка ({e.X}, {e.Y}): {relation} полигона «{polygon.Name}» ({AlgorithmName(algorithm)}). Щёлкните следующую точку.");

                }
                else
                {
                    SetStatus("Сначала выберите полигон в списке.");
                }
                break;

            case Tool.Side:
                HandleSideClick(point);
                break;
        }

        canvas.Invalidate();
    }

    private void HandleSideClick(PointF point)
    {
        if (canvas.ReferenceEdge is null)
        {
            edgeClicks.Add(point);
            canvas.QueryPoints.Add(point);
            if (edgeClicks.Count < 2)
            {
                SetStatus("Укажите вторую точку ребра.");
                return;
            }

            canvas.ReferenceEdge = (edgeClicks[0], edgeClicks[1]);
            edgeClicks.Clear();
            canvas.QueryPoints.Clear();
            SetStatus("Ребро задано. Щёлкайте точки для классификации.");
            return;
        }

        canvas.QueryPoints.Add(point);
        var edge = canvas.ReferenceEdge.Value;

        var side = segmentGeometry.ClassifyPoint(new Segment(edge.A, edge.B), point);
        SetStatus($"Точка ({point.X}, {point.Y}): {SideText(side)}. Щёлкните следующую точку.");

    }

    private void HandleIntersectionClick(PointF point)
    {
        edgeClicks.Add(point);
        canvas.QueryPoints.Add(point);
        if (edgeClicks.Count < 2)
        {
            SetStatus(intersectionReference is null
                ? "Укажите вторую точку первого ребра."
            : "Укажите вторую точку проверяемого ребра.");
            return;
        }

        var edge = (edgeClicks[0], edgeClicks[1]);
        edgeClicks.Clear();
        canvas.QueryPoints.Clear();
        if (intersectionReference is null)
        {
            intersectionReference = edge;
            canvas.ReferenceEdge = edge;
            SetStatus("Первое ребро задано. Укажите две точки второго ребра.");
            return;
        }

        canvas.SecondEdge = edge;

        canvas.ResultPoint = segmentGeometry.FindIntersection(
            new Segment(intersectionReference.Value.A, intersectionReference.Value.B),
            new Segment(edge.Item1, edge.Item2));
        SetStatus(canvas.ResultPoint is { } hit
            ? $"Пересечение: ({hit.X:0.##}, {hit.Y:0.##}). Укажите следующую пару точек."
            : "Рёбра не пересекаются. Укажите следующую пару точек.");

    }

    private void FinishPolygon()
    {
        if (canvas.CurrentPolygon is not { Vertices.Count: > 0 } polygon)
        {
            SetStatus("Добавьте хотя бы одну вершину.");
            return;
        }

        canvas.Polygons.Add(polygon);
        canvas.CurrentPolygon = null;
        polygonPicker.Items.Add($"Полигон {canvas.Polygons.Count} ({polygon.Vertices.Count} вершин)");
        polygonPicker.SelectedIndex = polygonPicker.Items.Count - 1;
        canvas.Invalidate();
        SetTool(Tool.Draw);
        SetStatus("Полигон сохранён. Щёлкните, чтобы начать следующий.");
    }

    private void AddPolygonVertex(PointF point)
    {
        if (canvas.CurrentPolygon is null)
            canvas.CurrentPolygon = new PolygonShape { Name = $"Полигон {canvas.Polygons.Count + 1}" };

        canvas.CurrentPolygon.Vertices.Add(point);
        SetStatus($"Вершин: {canvas.CurrentPolygon.Vertices.Count}. Нажмите «Завершить полигон», чтобы сохранить.");
    }

    private void ClearScene()
    {
        canvas.ClearScene();
        polygonPicker.Items.Clear();
        edgeClicks.Clear();
        intersectionReference = null;
        SetTool(Tool.Draw);
        SetStatus("Сцена очищена.");
    }

    private void StartContainment()
    {
        canvas.QueryPoints.Clear();
        canvas.ResultPoint = null;
        SetTool(Tool.Inside);
        SetStatus(SelectedPolygon() is null ? "Выберите полигон в списке, затем щёлкайте точки." : "Щёлкайте точки на холсте. Режим проверки остаётся активным.");
    }

    private void StartIntersection()
    {
        edgeClicks.Clear();
        canvas.QueryPoints.Clear();
        canvas.SecondEdge = null;
        canvas.ResultPoint = null;
        SetTool(Tool.Intersect);
        SetStatus(intersectionReference is null ? "Укажите две точки первого ребра." : "Укажите две точки нового ребра.");
    }

    private void ResetIntersection()
    {
        intersectionReference = null;
        edgeClicks.Clear();
        canvas.QueryPoints.Clear();
        canvas.ReferenceEdge = null;
        canvas.SecondEdge = null;
        canvas.ResultPoint = null;
        SetTool(Tool.Intersect);
        SetStatus("Укажите две точки нового первого ребра.");
    }

    private void StartSideCheck()
    {
        edgeClicks.Clear();
        canvas.QueryPoints.Clear();
        canvas.ReferenceEdge = null;
        canvas.ResultPoint = null;
        SetTool(Tool.Side);
        SetStatus("Укажите две точки ребра, затем щёлкайте точки для классификации.");
    }

    private void RotateSelected()
    {
        try
        {
            if (GetPivot() is { } pivot)
                TryTransform(() => affine.CreateRotation(ReadNumber(angle), pivot));
        }
        catch (FormatException)
        {
            SetStatus("Введите корректные числа.");
        }
    }

    private void ScaleSelected()
    {
        try
        {
            if (GetPivot() is { } pivot)
            {
                TryTransform(() => affine.CreateScaling(ReadNumber(sx), ReadNumber(sy), pivot));
            }
        }
        catch (FormatException)
        {
            SetStatus("Введите корректные числа.");
        }
    }

    private void TryTransform(Func<Matrix> createMatrix)
    {
        if (SelectedPolygon() is not { } polygon)
        {
            SetStatus("Выберите полигон в списке.");
            return;
        }

        try
        {
            using var matrix = createMatrix();
            affine.Apply(polygon, matrix);
            canvas.Invalidate();
            SetStatus("Преобразование применено.");
        }
        catch (FormatException)
        {
            SetStatus("Введите корректные числа.");
        }
    }

    private PointF? GetPivot()
    {
        if (!centerPivot.Checked)
            return new PointF(ReadNumber(pivotX), ReadNumber(pivotY));

        if (SelectedPolygon() is not { } polygon)
        {
            SetStatus("Сначала выберите полигон.");
            return null;
        }

        return affine.GetCenter(polygon);
    }

    private PolygonShape? SelectedPolygon()
    {
        if (canvas.SelectedIndex < 0 || canvas.SelectedIndex >= canvas.Polygons.Count)
            return null;

        return canvas.Polygons[canvas.SelectedIndex];
    }

    private ContainmentAlgorithm SelectedContainmentAlgorithm() =>
        angleAlgorithm.Checked ? ContainmentAlgorithm.AngleSum : ContainmentAlgorithm.RayCasting;

    private static string AlgorithmName(ContainmentAlgorithm algorithm) => algorithm switch
    {
        ContainmentAlgorithm.AngleSum => "метод суммы углов",
        _ => "метод луча"
    };

    private void SetTool(Tool next)
    {
        tool = next;
        canvas.Cursor = next == Tool.Draw ? Cursors.Cross : Cursors.Hand;
    }

    private void SetStatus(string message) => status.Text = $"Режим: {ToolName(tool)}\n{message}";
    private static string ToolName(Tool value) => value switch
    {
        Tool.Draw => "рисование полигона", Tool.Intersect => "пересечение рёбер", Tool.Inside => "проверка точки",
        Tool.Side => "сторона ребра", _ => "выбор точки преобразования"
    };
    private static string SideText(PointSide side) => side switch { PointSide.Left => "слева", PointSide.Right => "справа", _ => "на ребре" };
    private static float ReadNumber(TextBox input)
    {
        if (!float.TryParse(input.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out var value))
        {
            throw new FormatException();
        }

        return value;
    }

}
