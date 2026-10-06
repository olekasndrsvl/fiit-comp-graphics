#nullable enable

using System.Globalization;

namespace Lab4;

partial class Form1
{
    private System.ComponentModel.IContainer? components;
    private DrawingCanvas canvas = null!;
    private ComboBox polygonPicker = null!;
    private Label status = null!;
    private RadioButton customPivot = null!, centerPivot = null!;
    private RadioButton rayCastAlgorithm = null!, angleAlgorithm = null!;
    private TextBox dx = null!, dy = null!, angle = null!, sx = null!, sy = null!, pivotX = null!, pivotY = null!;


    protected override void Dispose(bool disposing)
    {
        if (disposing)
            components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96F, 96F);
        Font = new Font("Segoe UI", 9F);
        Text = "Lab4 — Аффинные преобразования";
        BackColor = SystemColors.Control;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 560);
        ClientSize = new Size(1120, 680);
        BuildInterface();
    }

    private void BuildInterface()
    {
        var workspace = Grid(2, 2);
        workspace.Padding = new Padding(6);
        workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        workspace.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        workspace.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Controls.Add(workspace);

        canvas = new DrawingCanvas { Dock = DockStyle.Fill };
        canvas.MouseDown += Canvas_MouseDown;
        polygonPicker = new ComboBox { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
        polygonPicker.SelectedIndexChanged += (_, _) =>
        {
            canvas.SelectedIndex = polygonPicker.SelectedIndex;
            canvas.Invalidate();
        };
        workspace.Controls.Add(Stack(
            new Label { Text = "Полигон", AutoSize = true }, polygonPicker,
            Button("Рисовать полигон", () => SetTool(Tool.Draw)),
            Button("Завершить полигон", FinishPolygon),
            Button("Очистить сцену", ClearScene)), 0, 0);

        dx = Number(10);
        dy = Number(10);
        angle = Number(15);
        sx = Number(1.2f);
        sy = Number(1.2f);
        pivotX = Number(0);
        pivotY = Number(0);

        customPivot = new RadioButton { Text = "Заданная точка", AutoSize = true, Checked = true };
        centerPivot = new RadioButton { Text = "Центр полигона", AutoSize = true };
        var pivotChoice = new FlowLayoutPanel
        {
            Width = 200, AutoSize = true, FlowDirection = FlowDirection.TopDown,
            WrapContents = false, Margin = new Padding(0)
        };
        pivotChoice.Controls.Add(customPivot);
        pivotChoice.Controls.Add(centerPivot);

        rayCastAlgorithm = new RadioButton { Text = "Метод луча", AutoSize = true, Checked = true };
        angleAlgorithm = new RadioButton { Text = "Метод суммы углов", AutoSize = true };
        var algorithmChoice = new FlowLayoutPanel
        {
            Width = 200, AutoSize = true, FlowDirection = FlowDirection.TopDown,
            WrapContents = false, Margin = new Padding(0)
        };
        algorithmChoice.Controls.Add(rayCastAlgorithm);
        algorithmChoice.Controls.Add(angleAlgorithm);

        var tabs = new TabControl { Dock = DockStyle.Fill };
        var pages = new[]
        {
            CreateTaskTab("1. Преобразования",
                Row("Смещение dx / dy", dx, dy),
                Button("Сместить", () => TryTransform(() =>
                    affine.CreateTranslation(ReadNumber(dx), ReadNumber(dy)))),
                pivotChoice,
                Row("Точка X / Y", pivotX, pivotY),
                Button("Выбрать точку мышью", () => SetTool(Tool.Pivot)),
                Row("Угол (°)", angle),
                Button("Повернуть", RotateSelected),
                Row("Масштаб sx / sy", sx, sy),
                Button("Масштабировать", ScaleSelected)),
            CreateTaskTab("2. Принадлежность точки",
                new Label { Text = "Алгоритм", AutoSize = true },
                algorithmChoice,
                Button("Проверить точку", StartContainment)),
            CreateTaskTab("3. Рёбра",
                Button("Найти пересечение", StartIntersection),
                Button("Новое первое ребро", ResetIntersection),
                Button("Проверить сторону точки", StartSideCheck))
        };
        foreach (var page in pages) tabs.TabPages.Add(page);
        // Один холст переносится между вкладками: все полигоны и точки сохраняются.
        void ShowCanvas()
        {
            if (tabs.SelectedTab?.Controls[0] is TableLayoutPanel body)
                body.Controls.Add(canvas, 1, 0);
        }
        tabs.SelectedIndexChanged += (_, _) => ShowCanvas();
        workspace.Controls.Add(tabs, 1, 0);
        tabs.SelectedIndex = 0;
        // До создания оконных дескрипторов SelectedTab может ещё быть null.
        ((TableLayoutPanel)pages[0].Controls[0]).Controls.Add(canvas, 1, 0);

        status = new Label
        {
            AutoSize = true, Dock = DockStyle.Fill,
            Text = "Режим: рисование полигона", Padding = new Padding(3)
        };
        workspace.Controls.Add(status, 0, 1);
        workspace.SetColumnSpan(status, 2);
    }

    private static TableLayoutPanel Grid(int columns, int rows) => new()
    {
        ColumnCount = columns, RowCount = rows, Dock = DockStyle.Fill, Margin = new Padding(3)
    };

    private static FlowLayoutPanel Stack(params Control[] controls)
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
            WrapContents = false, AutoScroll = true
        };
        foreach (var control in controls)
        {
            control.Margin = new Padding(3);
            panel.Controls.Add(control);
        }
        return panel;
    }

    private static TabPage CreateTaskTab(string title, params Control[] controls)
    {
        var tab = new TabPage(title) { BackColor = SystemColors.Control, Padding = new Padding(3) };
        var body = Grid(2, 1);
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        body.Controls.Add(Stack(controls), 0, 0);
        tab.Controls.Add(body);
        return tab;
    }

    private static Control Row(string title, params TextBox[] inputs)
    {
        var panel = new TableLayoutPanel
        {
            Width = 200, AutoSize = true, ColumnCount = inputs.Length, RowCount = 2,
            Margin = new Padding(3)
        };
        for (var i = 0; i < inputs.Length; i++)
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / inputs.Length));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var label = new Label { Text = title, AutoSize = true };
        panel.Controls.Add(label, 0, 0);
        panel.SetColumnSpan(label, inputs.Length);
        for (var i = 0; i < inputs.Length; i++)
        {
            inputs[i].Dock = DockStyle.Fill;
            panel.Controls.Add(inputs[i], i, 1);
        }
        return panel;
    }

    private static Button Button(string text, Action action)
    {
        var button = new Button
        {
            Text = text, Width = 200, Height = 28, AutoSize = true,
            MinimumSize = new Size(200, 28), UseVisualStyleBackColor = true
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static TextBox Number(float value) => new()
    {
        Text = value.ToString(CultureInfo.CurrentCulture),
        TextAlign = HorizontalAlignment.Right
    };
}
