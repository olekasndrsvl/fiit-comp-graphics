using System.Drawing.Drawing2D;
using System.Globalization;

namespace Lab4;

public partial class Form1 : Form
{
    private enum Tool { Draw, Intersect, Inside, Side, Pivot }

    private DrawingCanvas canvas = null!;
    private ComboBox polygonPicker = null!;
    private Label status = null!;
    private RadioButton customPivot = null!, centerPivot = null!;
    private TextBox dx = null!, dy = null!, angle = null!, sx = null!, sy = null!, pivotX = null!, pivotY = null!;
    private Tool tool = Tool.Draw;
    private readonly List<PointF> edgeClicks = [];
    private (PointF A, PointF B)? intersectionReference;
    private readonly IAffineTransformService affine = new AffineTransformService();
    private readonly IPolygonContainmentService containment = new PolygonContainmentService();
    private readonly ISegmentGeometryService segmentGeometry = new SegmentGeometryService();

    public Form1() => InitializeComponent();

    private void BuildInterface()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(12),
            BackColor = Color.FromArgb(244, 247, 250)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 390));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(layout);

        var sidebar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = false,
            Padding = new Padding(4)
        };
        canvas = new DrawingCanvas { Dock = DockStyle.Fill, Margin = new Padding(12, 0, 0, 0) };
        canvas.MouseDown += Canvas_MouseDown;
        layout.Controls.Add(sidebar, 0, 0);
        layout.Controls.Add(canvas, 1, 0);
        sidebar.Controls.Add(Header("Аффинная геометрия", 19));

        polygonPicker = new ComboBox { Width = 354, DropDownStyle = ComboBoxStyle.DropDownList };
        polygonPicker.SelectedIndexChanged += (_, _) =>
        {
            canvas.SelectedIndex = polygonPicker.SelectedIndex;
            canvas.Invalidate();
        };
        sidebar.Controls.Add(Group("Выбранный полигон", polygonPicker,
            Button("Рисовать полигон", () => SetTool(Tool.Draw)),
            Button("Завершить полигон", FinishPolygon),
            Button("Очистить сцену", ClearScene)));

        dx = Number(10);
        dy = Number(10);
        angle = Number(15);
        sx = Number(1.2f);
        sy = Number(1.2f);
        pivotX = Number(0);
        pivotY = Number(0);

        customPivot = new RadioButton { Text = "Заданная точка", AutoSize = true, Checked = true };
        centerPivot = new RadioButton { Text = "Центр полигона", AutoSize = true };
        var pivotChoice = new FlowLayoutPanel { Width = 344, Height = 28, WrapContents = false };
        pivotChoice.Controls.Add(customPivot);
        pivotChoice.Controls.Add(centerPivot);

        var taskPicker = new ComboBox { Width = 354, DropDownStyle = ComboBoxStyle.DropDownList };
        taskPicker.Items.AddRange(new object[]
        {
            "1. Аффинные преобразования",
            "2. Принадлежность точки полигону",
            "3. Операции с рёбрами"
        });

        var taskPanel = new Panel { Width = 364, Height = 540 };
        var taskBlocks = new Control[]
        {
            CreateTaskPanel("Аффинные преобразования",
                Row("Смещение dx / dy", dx, dy),
                Button("Применить смещение", () => TryTransform(() =>
                    affine.CreateTranslation(ReadNumber(dx), ReadNumber(dy)))),
                Row("Угол поворота (°)", angle),
                pivotChoice,
                Row("Точка X / Y", pivotX, pivotY),
                Button("Выбрать точку на холсте", () => SetTool(Tool.Pivot)),
                Button("Повернуть", RotateSelected),
                Row("Масштаб sx / sy", sx, sy),
                Button("Масштабировать", ScaleSelected)),
            CreateTaskPanel("Принадлежность точки",
                Button("Проверить точку в полигоне", StartContainment)),
            CreateTaskPanel("Операции с рёбрами",
                Button("Найти пересечение рёбер", StartIntersection),
                Button("Задать новое первое ребро", ResetIntersection),
                Button("Проверить сторону точки", StartSideCheck))
        };

        taskPicker.SelectedIndexChanged += (_, _) =>
        {
            taskPanel.Controls.Clear();
            var selected = taskPicker.SelectedIndex;
            if (selected >= 0)
            {
                taskBlocks[selected].Dock = DockStyle.Fill;
                taskPanel.Controls.Add(taskBlocks[selected]);
            }
        };
        taskPicker.SelectedIndex = 0;
        sidebar.Controls.Add(Group("Задание", taskPicker));
        sidebar.Controls.Add(taskPanel);

        status = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(354, 0),
            ForeColor = Color.FromArgb(39, 75, 100),
            Font = new Font(Font, FontStyle.Bold),
            Text = "Режим: рисование полигона"
        };
        sidebar.Controls.Add(Group("Статус", status));
    }

    private void Canvas_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left || !canvas.ClientRectangle.Contains(e.Location))
        {
            return;
        }

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
                    try
                    {
                        var contains = containment.ContainsPoint(polygon, point);
                        var relation = contains ? "внутри" : "снаружи";
                        SetStatus($"Точка ({e.X}, {e.Y}): {relation} полигона «{polygon.Name}». Щёлкните следующую точку.");
                    }
                    catch (NotImplementedException)
                    {
                        SetStatus("TODO: проверка принадлежности точки полигону");
                    }
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
        try
        {
            var side = segmentGeometry.ClassifyPoint(new Segment(edge.A, edge.B), point);
            SetStatus($"Точка ({point.X}, {point.Y}): {SideText(side)}. Щёлкните следующую точку.");
        }
        catch (NotImplementedException)
        {
            SetStatus("TODO: классификация точки относительно ребра");
        }
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
        try
        {
            canvas.ResultPoint = segmentGeometry.FindIntersection(
                new Segment(intersectionReference.Value.A, intersectionReference.Value.B),
                new Segment(edge.Item1, edge.Item2));
            SetStatus(canvas.ResultPoint is { } hit
                ? $"Пересечение: ({hit.X:0.##}, {hit.Y:0.##}). Укажите следующую пару точек."
                : "Рёбра не пересекаются. Укажите следующую пару точек.");
        }
        catch (NotImplementedException)
        {
            SetStatus("TODO: поиск пересечения отрезков");
        }
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
        {
            canvas.CurrentPolygon = new PolygonShape { Name = $"Полигон {canvas.Polygons.Count + 1}" };
        }

        canvas.CurrentPolygon.Vertices.Add(point);
        SetStatus($"Вершин: {canvas.CurrentPolygon.Vertices.Count}. Нажмите «Завершить полигон», чтобы сохранить.");
    }

    private void ClearScene()
    {
        canvas.ClearScene(); polygonPicker.Items.Clear(); edgeClicks.Clear(); intersectionReference = null;
        SetTool(Tool.Draw); SetStatus("Сцена очищена.");
    }

    private void StartContainment()
    {
        canvas.QueryPoints.Clear(); canvas.ResultPoint = null; SetTool(Tool.Inside);
        SetStatus(SelectedPolygon() is null ? "Выберите полигон в списке, затем щёлкайте точки." : "Щёлкайте точки на холсте. Режим проверки остаётся активным.");
    }

    private void StartIntersection()
    {
        edgeClicks.Clear(); canvas.QueryPoints.Clear(); canvas.SecondEdge = null; canvas.ResultPoint = null; SetTool(Tool.Intersect);
        SetStatus(intersectionReference is null ? "Укажите две точки первого ребра." : "Укажите две точки нового ребра.");
    }

    private void ResetIntersection()
    {
        intersectionReference = null; edgeClicks.Clear(); canvas.QueryPoints.Clear(); canvas.ReferenceEdge = null;
        canvas.SecondEdge = null; canvas.ResultPoint = null; SetTool(Tool.Intersect);
        SetStatus("Укажите две точки нового первого ребра.");
    }

    private void StartSideCheck()
    {
        edgeClicks.Clear(); canvas.QueryPoints.Clear(); canvas.ReferenceEdge = null; canvas.ResultPoint = null; SetTool(Tool.Side);
        SetStatus("Укажите две точки ребра, затем щёлкайте точки для классификации.");
    }

    private void RotateSelected()
    {
        try
        {
            if (GetPivot() is { } pivot)
            {
                TryTransform(() => affine.CreateRotation(ReadNumber(angle), pivot));
            }
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
        if (SelectedPolygon() is not { } polygon) { SetStatus("Выберите полигон в списке."); return; }
        try
        {
            using var matrix = createMatrix(); affine.Apply(polygon, matrix);
            canvas.Invalidate(); SetStatus("Преобразование применено.");
        }
        catch (FormatException)
        {
            SetStatus("Введите корректные числа.");
        }
        catch (NotImplementedException) { SetStatus("TODO: матричное преобразование"); }
    }

    private PointF? GetPivot()
    {
        if (!centerPivot.Checked)
        {
            return new PointF(ReadNumber(pivotX), ReadNumber(pivotY));
        }

        if (SelectedPolygon() is not { } polygon)
        {
            SetStatus("Сначала выберите полигон.");
            return null;
        }

        try
        {
            return affine.GetCenter(polygon);
        }
        catch (NotImplementedException)
        {
            SetStatus("TODO: вычисление центра полигона");
            return null;
        }
    }

    private PolygonShape? SelectedPolygon() => canvas.SelectedIndex >= 0 && canvas.SelectedIndex < canvas.Polygons.Count ? canvas.Polygons[canvas.SelectedIndex] : null;
    private void SetTool(Tool next) { tool = next; canvas.Cursor = next == Tool.Draw ? Cursors.Cross : Cursors.Hand; }
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

    private static GroupBox CreateTaskPanel(string title, params Control[] controls)
    {
        var group = new GroupBox
        {
            Text = title,
            Dock = DockStyle.Fill,
            Width = 360,
            Height = 530,
            Padding = new Padding(8)
        };
        var content = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = false
        };

        foreach (var control in controls)
        {
            content.Controls.Add(control);
        }

        group.Controls.Add(content);
        return group;
    }

    private static Control Group(string title, params Control[] children)
    {
        var box = new GroupBox { Text = title, Width = 360, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(8), Margin = new Padding(2, 5, 2, 5) };
        var stack = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true };
        foreach (var child in children) { child.Margin = new Padding(2, 3, 2, 3); stack.Controls.Add(child); }
        box.Controls.Add(stack); return box;
    }

    private static Control Row(string title, params TextBox[] inputs)
    {
        var panel = new FlowLayoutPanel { Width = 344, Height = 48, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        panel.Controls.Add(new Label { Text = title, AutoSize = true });
        var row = new FlowLayoutPanel { Width = 340, Height = 28, WrapContents = false };
        foreach (var input in inputs) { input.Width = inputs.Length == 1 ? 320 : 150; row.Controls.Add(input); }
        panel.Controls.Add(row); return panel;
    }

    private static Label Header(string text, float size) => new() { Text = text, AutoSize = true, Font = new Font("Segoe UI", size, FontStyle.Bold), Margin = new Padding(3, 2, 3, 6) };
    private static Button Button(string text, Action action)
    {
        var button = new Button { Text = text, Width = 326, Height = 34, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8, 0, 0, 0) };
        button.Click += (_, _) => action(); return button;
    }
    private static TextBox Number(float value) => new()
    {
        Text = value.ToString(CultureInfo.CurrentCulture),
        TextAlign = HorizontalAlignment.Right,
        Height = 26
    };
}
