#nullable enable

namespace Lab3;

partial class Form1
{
    private DrawingCanvas fillCanvas = null!, lineCanvas = null!, triangleCanvas = null!;
    private ComboBox fillMode = null!, lineAlgorithm = null!;
    private Label linePointsLabel = null!, trianglePointsLabel = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) pattern?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = new Font("Segoe UI", 9F);
        ClientSize = new Size(1120, 720);
        MinimumSize = new Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Lab3 — Растровые алгоритмы";
        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(CreateFillTab());
        tabs.TabPages.Add(CreateLineTab());
        tabs.TabPages.Add(CreateTriangleTab());
        Controls.Add(tabs);
        ResumeLayout(true);
    }

    private TabPage CreateFillTab()
    {
        fillCanvas = new DrawingCanvas();
        fillCanvas.MouseDown += FillCanvas_MouseDown;
        fillCanvas.MouseMove += FillCanvas_MouseMove;
        fillCanvas.MouseUp += (_, _) => { previousDrawingPoint = null; fillCanvas.Capture = false; };
        fillCanvas.MouseCaptureChanged += (_, _) => previousDrawingPoint = null;
        fillMode = Choice("Рисовать область", "1а. Заливка цветом", "1б. Заливка рисунком", "1в. Обход границы");
        fillMode.SelectedIndexChanged += (_, _) => previousDrawingPoint = null;
        return TaskTab("1. Заливка и граница", fillCanvas,
            fillMode,
            ColorButton("Цвет границы", Color.FromArgb(38, 50, 56), c => boundaryColor = c),
            ColorButton("Цвет заливки", Color.FromArgb(77, 182, 172), c => fillColor = c),
            ColorButton("Цвет выделения", Color.FromArgb(255, 112, 67), c => traceColor = c),
            ActionButton("Загрузить изображение…", LoadSourceImage),
            ActionButton("Загрузить рисунок…", LoadPattern),
            ActionButton("Очистить", () =>
            {
                fillCanvas.Clear();
                previousDrawingPoint = null;
            }));
    }

    private TabPage CreateLineTab()
    {
        lineCanvas = new DrawingCanvas();
        lineCanvas.MouseDown += LineCanvas_MouseDown;
        lineAlgorithm = Choice("Брезенхем (целочисленный)", "Ву (со сглаживанием)");
        linePointsLabel = CoordinateLabel("Начало: —\nКонец: —");
        return TaskTab("2. Отрезки", lineCanvas,
            lineAlgorithm,
            ColorButton("Цвет отрезка", Color.FromArgb(92, 107, 192), c => lineColor = c), linePointsLabel,
            ActionButton("Нарисовать", DrawSelectedLine),
            ActionButton("Очистить", () => { linePoints.Clear(); lineCanvas.Clear(); UpdateLinePoints(); }));
    }

    private TabPage CreateTriangleTab()
    {
        triangleCanvas = new DrawingCanvas();
        triangleCanvas.MouseDown += TriangleCanvas_MouseDown;
        trianglePointsLabel = CoordinateLabel("A: —\nB: —\nC: —");
        return TaskTab("3. Градиентный треугольник", triangleCanvas,
            ColorButton("Цвет вершины A", Color.Red, c => vertexColors[0] = c),
            ColorButton("Цвет вершины B", Color.Lime, c => vertexColors[1] = c),
            ColorButton("Цвет вершины C", Color.Blue, c => vertexColors[2] = c),
            trianglePointsLabel,
            ActionButton("Закрасить", DrawSelectedTriangle),
            ActionButton("Очистить", () => { trianglePoints.Clear(); triangleCanvas.Clear(); UpdateTrianglePoints(); }));
    }

    private static TabPage TaskTab(string title, DrawingCanvas canvas, params Control[] controls)
    {
        var tab = new TabPage(title) { Padding = new Padding(3), BackColor = SystemColors.Control };
        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var sidebar = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
        foreach (var control in controls)
        {
            control.Width = 220;
            control.Margin = new Padding(3, 4, 3, 4);
            sidebar.Controls.Add(control);
        }
        canvas.Dock = DockStyle.Fill;
        grid.Controls.Add(sidebar, 0, 0);
        grid.Controls.Add(canvas, 1, 0);
        tab.Controls.Add(grid);
        return tab;
    }

    private static Label CoordinateLabel(string text) => new() { Text = text, AutoSize = true };

    private static ComboBox Choice(params string[] items)
    {
        var combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        combo.Items.AddRange(items);
        combo.SelectedIndex = 0;
        return combo;
    }

    private static Button ActionButton(string text, Action action)
    {
        var button = new Button { Text = text, Height = 32, UseVisualStyleBackColor = true };
        button.Click += (_, _) => action();
        return button;
    }

    private Button ColorButton(string text, Color initial, Action<Color> selected)
    {
        var button = new Button { Text = text, Height = 32, BackColor = initial, UseVisualStyleBackColor = false };
        void UpdateColor(Color color)
        {
            button.BackColor = color;
            button.ForeColor = color.GetBrightness() < 0.45f ? Color.White : Color.Black;
            selected(color);
        }
        UpdateColor(initial);
        button.Click += (_, _) =>
        {
            using var dialog = new ColorDialog { Color = button.BackColor, FullOpen = true };
            if (dialog.ShowDialog(this) == DialogResult.OK) UpdateColor(dialog.Color);
        };
        return button;
    }
}
