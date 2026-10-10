#nullable enable

namespace Lab5;

partial class Form1
{
    private System.ComponentModel.IContainer? components;

    private Label _fileLabel = null!;
    private TrackBar _stepTrack = null!;
    private Label _stepLabel = null!;
    private NumericUpDown _midIterations = null!;
    private CheckBox _treeMode = null!;
    private CheckBox _randomBranches = null!;
    private CheckBox _autoScale = null!;
    private TextBox _lAxiom = null!;
    private TextBox _lRules = null!;
    private NumericUpDown _lIterations = null!;
    private NumericUpDown _lAngle = null!;
    private NumericUpDown _lDirection = null!;
    private NumericUpDown _lRandomness = null!;
    private NumericUpDown _treeBaseThickness = null!;
    private NumericUpDown _treeThicknessDecay = null!;
    private TextBox _midInitialHeight = null!;
    private NumericUpDown _midDisplacement = null!;
    private NumericUpDown _midRoughness = null!;
    private NumericUpDown _midSeed = null!;
    private CheckBox _midFixedSeed = null!;
    private CheckBox _showBezierPoints = null!;
    private CheckBox _showBezierLines = null!;
    private CheckBox _closeBezier = null!;
    private NumericUpDown _bezierQuality = null!;
    private Panel _lCanvas = null!;
    private Panel _midCanvas = null!;
    private Panel _bezierCanvas = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        SuspendLayout();
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96F, 96F);
        ClientSize = new Size(1120, 720);
        MinimumSize = new Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Лабораторная работа №5 — L-системы. Алгоритм midpoint displacement. Кривые Безье";
        BackColor = SystemColors.Control;
        Font = new Font("Segoe UI", 9F);
        Controls.Add(CreateTabs());
        ResumeLayout(true);
    }

    private Control CreateTabs()
    {
        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(CreateLSystemPage());
        tabs.TabPages.Add(CreateMidpointPage());
        tabs.TabPages.Add(CreateBezierPage());
        return tabs;
    }

    private TabPage CreateLSystemPage()
    {
        var page = CreatePage("1. L-системы");
        var layout = CreateTaskLayout();
        var sidebar = CreateSidebar();

        sidebar.Controls.Add(CreateSectionTitle("Описание L-системы"));
        var open = CreateWideButton("Открыть файл L-системы");
        open.Click += OpenLSystem_Click;
        sidebar.Controls.Add(open);

        _fileLabel = CreateNote("Файл не выбран");
        _fileLabel.AutoEllipsis = true;
        sidebar.Controls.Add(_fileLabel);
        sidebar.Controls.Add(CreateFieldLabel("Аксиома"));
        _lAxiom = CreateTextField("F");
        sidebar.Controls.Add(_lAxiom);
        sidebar.Controls.Add(CreateFieldLabel("Правила (символ=замена)"));
        _lRules = CreateTextField("F=F[+F]F[-F]F", true);
        _lRules.Height = 76;
        sidebar.Controls.Add(_lRules);

        _lIterations = CreateNumber(0, 8, 4);
        _lAngle = CreateNumber(0, 180, 25);
        _lDirection = CreateNumber(-360, 360, -90);
        _lRandomness = CreateNumber(0, 45, 8);
        var parameters = CreateTwoColumnGrid();
        parameters.Controls.Add(CreateLabeledControl("Итерации", _lIterations), 0, 0);
        parameters.Controls.Add(CreateLabeledControl("Угол, °", _lAngle), 1, 0);
        parameters.Controls.Add(CreateLabeledControl("Направление, °", _lDirection), 0, 1);
        parameters.Controls.Add(CreateLabeledControl("Случайность, °", _lRandomness), 1, 1);
        sidebar.Controls.Add(parameters);

        _treeMode = CreateCheckBox("Режим фрактального дерева", true);
        _treeMode.CheckedChanged += LSystemOptionChanged;
        _randomBranches = CreateCheckBox("Случайное отклонение ветвей", true);
        _autoScale = CreateCheckBox("Автомасштабирование", true);
        sidebar.Controls.Add(_treeMode);
        sidebar.Controls.Add(_randomBranches);
        sidebar.Controls.Add(_autoScale);

        _treeBaseThickness = CreateNumber(1, 30, 10);
        _treeThicknessDecay = CreateNumber(10, 100, 70);
        var treeParameters = CreateTwoColumnGrid();
        treeParameters.Controls.Add(CreateLabeledControl("Толщина", _treeBaseThickness), 0, 0);
        treeParameters.Controls.Add(CreateLabeledControl("Сужение, %", _treeThicknessDecay), 1, 0);
        sidebar.Controls.Add(treeParameters);

        var build = CreateWideButton("Построить");
        build.Margin = new Padding(0, 14, 0, 4);
        build.Click += BuildLSystem_Click;
        sidebar.Controls.Add(build);
        var clear = CreateWideButton("Очистить холст");
        clear.Click += ClearLSystem_Click;
        sidebar.Controls.Add(clear);

        _lCanvas = CreateCanvas();
        layout.Controls.Add(sidebar, 0, 0);
        layout.Controls.Add(_lCanvas, 1, 0);
        page.Controls.Add(layout);
        return page;
    }

    private TabPage CreateMidpointPage()
    {
        var page = CreatePage("2. Алгоритм midpoint displacement");
        var layout = CreateTaskLayout();
        var sidebar = CreateSidebar();

        var parametersTitle = CreateSectionTitle("Параметры построения ломаной");
        parametersTitle.Height = 40;
        sidebar.Controls.Add(parametersTitle);
        sidebar.Controls.Add(CreateFieldLabel("Начальная высота"));
        _midInitialHeight = CreateTextField("0,5");
        sidebar.Controls.Add(_midInitialHeight);

        _midIterations = CreateNumber(1, 12, 7);
        _midDisplacement = CreateNumber(1, 500, 180);
        _midRoughness = CreateNumber(1, 100, 55);
        _midSeed = CreateNumber(0, int.MaxValue, 42);
        var parameters = CreateTwoColumnGrid();
        parameters.Controls.Add(CreateLabeledControl("Итерации", _midIterations), 0, 0);
        parameters.Controls.Add(CreateLabeledControl("Смещение", _midDisplacement), 1, 0);
        parameters.Controls.Add(CreateLabeledControl("Шероховатость, %", _midRoughness), 0, 1);
        parameters.Controls.Add(CreateLabeledControl("Seed", _midSeed), 1, 1);
        sidebar.Controls.Add(parameters);

        _midFixedSeed = CreateCheckBox("Фиксировать генератор", true);
        sidebar.Controls.Add(_midFixedSeed);
        var generate = CreateWideButton("Построить ломаную");
        generate.Margin = new Padding(0, 18, 0, 10);
        generate.Click += GenerateTerrain_Click;
        sidebar.Controls.Add(generate);

        sidebar.Controls.Add(CreateSectionTitle("Шаг алгоритма"));
        _stepLabel = CreateNote("Шаг 0 из 7");
        _stepLabel.ForeColor = SystemColors.ControlText;
        sidebar.Controls.Add(_stepLabel);
        _stepTrack = new TrackBar
        {
            Minimum = 0, Maximum = 7, Value = 0, TickStyle = TickStyle.None,
            Width = 220, Margin = new Padding(3, 2, 3, 8)
        };
        _stepTrack.ValueChanged += TerrainStep_ValueChanged;
        sidebar.Controls.Add(_stepTrack);

        var navigation = CreateTwoColumnGrid();
        navigation.AutoSize = false;
        navigation.Height = 34;
        navigation.RowCount = 1;
        navigation.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        var previous = CreateButton("← Назад");
        var next = CreateButton("Вперёд →");
        previous.Width = next.Width = 104;
        previous.Click += PreviousTerrainStep_Click;
        next.Click += NextTerrainStep_Click;
        navigation.Controls.Add(previous, 0, 0);
        navigation.Controls.Add(next, 1, 0);
        sidebar.Controls.Add(navigation);

        _midCanvas = CreateCanvas();
        layout.Controls.Add(sidebar, 0, 0);
        layout.Controls.Add(_midCanvas, 1, 0);
        page.Controls.Add(layout);
        return page;
    }

    private TabPage CreateBezierPage()
    {
        var page = CreatePage("3. Кубические сплайны Безье");
        var layout = CreateTaskLayout();
        var sidebar = CreateSidebar();

        var editorTitle = CreateSectionTitle("Составная кубическая кривая Безье");
        editorTitle.Height = 40;
        sidebar.Controls.Add(editorTitle);
        var clear = CreateWideButton("Очистить все точки");
        clear.Margin = new Padding(0, 18, 0, 6);
        clear.Click += ClearBezier_Click;
        sidebar.Controls.Add(clear);
        var example = CreateWideButton("Добавить пример кривой");
        example.Click += AddBezierExample_Click;
        sidebar.Controls.Add(example);

        sidebar.Controls.Add(CreateSectionTitle("Отображение"));
        _showBezierPoints = CreateCheckBox("Показывать опорные точки", true);
        _showBezierLines = CreateCheckBox("Показывать вспомогательные линии", true);
        _closeBezier = CreateCheckBox("Замкнуть кривую", false);
        sidebar.Controls.Add(_showBezierPoints);
        sidebar.Controls.Add(_showBezierLines);
        sidebar.Controls.Add(_closeBezier);
        _bezierQuality = CreateNumber(10, 500, 100);
        var quality = CreateLabeledControl("Точность (сегментов)", _bezierQuality);
        quality.Margin = new Padding(0, 10, 0, 0);
        sidebar.Controls.Add(quality);

        _bezierCanvas = CreateCanvas();
        _bezierCanvas.MouseDown += BezierCanvas_MouseDown;
        _bezierCanvas.MouseMove += BezierCanvas_MouseMove;
        _bezierCanvas.MouseUp += BezierCanvas_MouseUp;
        layout.Controls.Add(sidebar, 0, 0);
        layout.Controls.Add(_bezierCanvas, 1, 0);
        page.Controls.Add(layout);
        return page;
    }

    private static TabPage CreatePage(string text) => new(text)
    {
        BackColor = SystemColors.Control,
        Padding = new Padding(3)
    };

    private static TableLayoutPanel CreateTaskLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, BackColor = SystemColors.Control,
            ColumnCount = 2, RowCount = 1, Margin = new Padding(0)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        return layout;
    }

    private static FlowLayoutPanel CreateSidebar() => new()
    {
        Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
        WrapContents = false, AutoScroll = true,
        BackColor = SystemColors.Control, Padding = new Padding(3)
    };

    private static Panel CreateCanvas() => new()
    {
        BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle,
        Dock = DockStyle.Fill, Margin = new Padding(3), Cursor = Cursors.Cross
    };

    private static Label CreateSectionTitle(string text) => new()
    {
        Text = text, Font = new Font("Segoe UI Semibold", 10F),
        AutoSize = false, Width = 220, Height = 28, Margin = new Padding(3, 2, 3, 2)
    };

    private static Label CreateFieldLabel(string text) => new()
    {
        Text = text, AutoSize = false, Width = 220, Height = 23,
        TextAlign = ContentAlignment.BottomLeft, Margin = new Padding(3, 4, 3, 2)
    };

    private static Label CreateNote(string text) => new()
    {
        Text = text, ForeColor = SystemColors.GrayText, AutoSize = false,
        Width = 220, Height = 24, Margin = new Padding(3, 0, 3, 4)
    };

    private static TextBox CreateTextField(string text, bool multiline = false) => new()
    {
        Text = text, Multiline = multiline, AutoSize = false,
        Width = 220, Height = multiline ? 68 : 27,
        Margin = new Padding(3, 0, 3, 6)
    };

    private static Button CreateWideButton(string text)
    {
        var button = CreateButton(text);
        button.Width = 220;
        return button;
    }

    private static Button CreateButton(string text) => new()
    {
        Text = text, AutoSize = true, MinimumSize = new Size(100, 28),
        Dock = DockStyle.Fill, UseVisualStyleBackColor = true
    };

    private static CheckBox CreateCheckBox(string text, bool value) => new()
    {
        Text = text, Checked = value, AutoSize = false,
        Width = 220, Height = 27, UseVisualStyleBackColor = true,
        Margin = new Padding(3, 1, 3, 1)
    };

    private static TableLayoutPanel CreateTwoColumnGrid()
    {
        var grid = new TableLayoutPanel
        {
            Width = 220, AutoSize = true, ColumnCount = 2,
            RowCount = 0, Margin = new Padding(3, 6, 3, 7)
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        return grid;
    }

    private static Panel CreateLabeledControl(string label, Control control)
    {
        var panel = new Panel { Width = 108, Height = 60, Margin = new Padding(0, 0, 2, 4) };
        var caption = CreateFieldLabel(label);
        caption.Width = 108;
        caption.Height = 22;
        caption.Location = new Point(0, 0);
        control.Width = 106;
        control.Height = 32;
        control.Location = new Point(0, 25);
        panel.Controls.Add(caption);
        panel.Controls.Add(control);
        return panel;
    }

    private static NumericUpDown CreateNumber(decimal min, decimal max, decimal value) => new()
    {
        Minimum = min, Maximum = max, Value = value,
        BorderStyle = BorderStyle.FixedSingle,
        TextAlign = HorizontalAlignment.Center
    };
}
