#nullable enable

using Lab6.UI;

namespace Lab6;

partial class Form1
{
    private System.ComponentModel.IContainer? components;
    private DrawingCanvas canvas = null!;
    private ComboBox projectionPicker = null!;
    private ComboBox rotationAxis = null!, reflectionPlane = null!;
    private NumericUpDown angle = null!, lineAngle = null!;
    private NumericUpDown[] translation = null!, scaleInputs = null!, lineStart = null!, lineEnd = null!;
    private CheckBox rotationCenter = null!, scaleCenter = null!;
    private Label status = null!, modelInfo = null!, centerInfo = null!;
    private TabControl toolTabs = null!;
    private readonly List<Panel> operationPanels = [];
    private readonly List<RadioButton> operationButtons = [];
    private readonly List<RadioButton> modelButtons = [];

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
        Font = new Font("Segoe UI", 9F);
        ClientSize = new Size(1220, 760);
        MinimumSize = new Size(980, 650);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "lab6";
        BackColor = EditorTheme.Background;
        ForeColor = EditorTheme.Text;
        BuildInterface();
        ResumeLayout(true);
    }

    private void BuildInterface()
    {
        var workspace = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2,
            Padding = new Padding(6), Margin = Padding.Empty
        };
        workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        workspace.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        workspace.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill, Size = new Size(1200, 710), SplitterDistance = 850,
            FixedPanel = FixedPanel.Panel2, Panel1MinSize = 400, Panel2MinSize = 330,
            SplitterWidth = 5, BackColor = EditorTheme.Border, Margin = Padding.Empty
        };
        split.Panel1.BackColor = split.Panel2.BackColor = EditorTheme.Background;
        canvas = new DrawingCanvas { Dock = DockStyle.Fill, Margin = Padding.Empty };
        split.Panel1.Controls.Add(canvas);

        var inspector = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2,
            Padding = new Padding(6, 0, 0, 0), Margin = Padding.Empty
        };
        inspector.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        inspector.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        inspector.RowStyles.Add(new RowStyle(SizeType.Absolute, 108));
        toolTabs = new TabControl
        {
            Dock = DockStyle.Fill,
            SizeMode = TabSizeMode.Fixed, ItemSize = new Size(102, 42), Margin = Padding.Empty
        };
        var icons = new ImageList(components!) { ImageSize = new Size(24, 24), ColorDepth = ColorDepth.Depth32Bit };
        var tabIcons = new[] { "Cube", "Move", "Projection" }.Select(EditorTheme.LoadIcon).ToArray();
        foreach (var icon in tabIcons) icons.Images.Add(icon);
        icons.Disposed += (_, _) => { foreach (var icon in tabIcons) icon.Dispose(); };
        toolTabs.ImageList = icons;
        toolTabs.TabPages.Add(CreateModelPage());
        toolTabs.TabPages.Add(CreateTransformPage());
        toolTabs.TabPages.Add(CreateViewPage());
        for (var i = 0; i < toolTabs.TabCount; i++) toolTabs.TabPages[i].ImageIndex = i;
        toolTabs.SelectedIndex = 1;
        toolTabs.SelectedIndexChanged += (_, _) => toolTabs.Invalidate();
        inspector.Controls.Add(toolTabs, 0, 0);

        var footer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(8, 4, 8, 2) };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        footer.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        footer.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        footer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        modelInfo = Note("Модель не создана");
        centerInfo = Note("Центр: —");
        modelInfo.Dock = centerInfo.Dock = DockStyle.Fill;
        footer.Controls.Add(modelInfo, 0, 0);
        footer.Controls.Add(centerInfo, 0, 1);
        var reset = ActionButton("Сбросить преобразования", ResetModel);
        reset.Dock = DockStyle.Fill;
        footer.Controls.Add(reset, 0, 2);
        inspector.Controls.Add(footer, 0, 1);
        split.Panel2.Controls.Add(inspector);
        status = new Label
        {
            Dock = DockStyle.Fill, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = EditorTheme.Muted, Text = "Готово", Padding = new Padding(8, 0, 0, 0)
        };
        workspace.Controls.Add(split, 0, 0);
        workspace.Controls.Add(status, 0, 1);
        Controls.Add(workspace);
        projectionPicker.SelectedIndexChanged += (_, _) => UpdateScene("Проекция изменена");
    }

    private TabPage CreateModelPage()
    {
        var page = Page("Модели", out var panel);
        panel.Controls.Add(Title("Правильные многогранники"));
        var palette = Palette();
        var names = new[]
        {
            ("Tetrahedron", "Тетраэдр"), ("Cube", "Гексаэдр"), ("Octahedron", "Октаэдр"),
            ("Icosahedron", "Икосаэдр"), ("Dodecahedron", "Додекаэдр")
        };
        for (var i = 0; i < names.Length; i++)
        {
            var index = i;
            var button = IconButton(names[i].Item1, names[i].Item2, () => SelectModel(index));
            modelButtons.Add(button);
            palette.Controls.Add(button);
        }
        MarkSelected(modelButtons[selectedModel], true);
        void FitModelPalette()
        {
            var columns = Math.Max(1, palette.ClientSize.Width / 96);
            palette.Height = ((modelButtons.Count + columns - 1) / columns) * 74;
        }
        palette.SizeChanged += (_, _) => FitModelPalette();
        FitModelPalette();
        panel.Controls.Add(palette);
        panel.Controls.Add(ActionButton("Создать заново", () => SelectModel(selectedModel)));
        return page;
    }

    private TabPage CreateTransformPage()
    {
        var page = new TabPage("Правка") { BackColor = EditorTheme.Background, Padding = new Padding(8) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = Padding.Empty };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 160));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var palette = Palette();
        palette.Dock = DockStyle.Fill;
        var deck = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
        var tools = new[] { ("Move", "Смещение"), ("Rotate", "Поворот"), ("Scale", "Масштаб"), ("Reverse", "Отражение"), ("Matrix", "Прямая AB") };
        for (var i = 0; i < tools.Length; i++)
        {
            var index = i;
            var button = IconButton(tools[i].Item1, tools[i].Item2, () => SelectOperation(index));
            operationButtons.Add(button);
            palette.Controls.Add(button);
            var settings = Stack();
            settings.Visible = false;
            settings.Controls.Add(Title(tools[i].Item2));
            operationPanels.Add(settings);
            deck.Controls.Add(settings);
        }
        translation = Triple(0, 0, 0, -10000, 10000);
        operationPanels[0].Controls.Add(TripleField("Смещение по мировым осям", translation));
        operationPanels[0].Controls.Add(ActionButton("Сместить", TranslateModel));

        rotationAxis = Picker("X", "Y", "Z");
        angle = Number(-360, 360, 15, 1, 5);
        rotationCenter = Check("Ось через центр многогранника", true);
        operationPanels[1].Controls.Add(Field("Координатная ось", rotationAxis));
        operationPanels[1].Controls.Add(Field("Угол, °", angle));
        operationPanels[1].Controls.Add(rotationCenter);
        operationPanels[1].Controls.Add(ActionButton("Повернуть", () => Rotate(false)));

        scaleInputs = Triple(1.2m, 1.2m, 1.2m, -100, 100);
        scaleCenter = Check("Относительно своего центра", true);
        operationPanels[2].Controls.Add(TripleField("Коэффициенты масштаба", scaleInputs));
        operationPanels[2].Controls.Add(scaleCenter);
        operationPanels[2].Controls.Add(ActionButton("Масштабировать", ScaleModel));

        reflectionPlane = Picker("XY", "XZ", "YZ");
        operationPanels[3].Controls.Add(Field("Координатная плоскость", reflectionPlane));
        operationPanels[3].Controls.Add(ActionButton("Отразить", Reflect));

        lineStart = Triple(0, 0, 0, -10000, 10000);
        lineEnd = Triple(1, 1, 1, -10000, 10000);
        lineAngle = Number(-360, 360, 15, 1, 5);
        operationPanels[4].Controls.Add(TripleField("Точка A", lineStart));
        operationPanels[4].Controls.Add(TripleField("Точка B", lineEnd));
        operationPanels[4].Controls.Add(Field("Угол, °", lineAngle));
        operationPanels[4].Controls.Add(ActionButton("Повернуть вокруг AB", () => Rotate(true)));
        SelectOperation(0);
        layout.Controls.Add(palette, 0, 0);
        layout.Controls.Add(deck, 0, 1);
        page.Controls.Add(layout);
        return page;
    }

    private TabPage CreateViewPage()
    {
        var page = Page("Проекция", out var panel);
        panel.Controls.Add(Title("Проецирование"));
        projectionPicker = Picker("Аксонометрическая", "Перспективная");
        panel.Controls.Add(Field("Тип проекции", projectionPicker));
        var legend = Palette();
        legend.Height = 26;
        legend.AutoSize = false;
        foreach (var (text, color) in new[] { ("X", Color.IndianRed), ("Y", Color.ForestGreen), ("Z", Color.RoyalBlue) })
            legend.Controls.Add(new Label { Text = text, ForeColor = color, Width = 60 });
        panel.Controls.Add(legend);
        return page;
    }

    private void SelectOperation(int index)
    {
        for (var i = 0; i < operationPanels.Count; i++)
        {
            operationPanels[i].Visible = i == index;
            MarkSelected(operationButtons[i], i == index);
        }
        operationPanels[index].BringToFront();
    }

    private static FlowLayoutPanel Stack()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
            WrapContents = false, AutoScroll = true, Padding = new Padding(2), Margin = Padding.Empty
        };
        void ResizeChildren()
        {
            var width = Math.Max(100, panel.ClientSize.Width - panel.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth - 2);
            foreach (Control child in panel.Controls) child.Width = width;
        }
        panel.ClientSizeChanged += (_, _) => ResizeChildren();
        panel.ControlAdded += (_, _) => ResizeChildren();
        return panel;
    }

    private static TabPage Page(string text, out FlowLayoutPanel panel)
    {
        var page = new TabPage(text) { BackColor = EditorTheme.Background, Padding = new Padding(8) };
        panel = Stack();
        page.Controls.Add(panel);
        return page;
    }

    private static FlowLayoutPanel Palette() => new()
    {
        Width = 300, Height = 150, Margin = new Padding(0, 2, 0, 8),
        FlowDirection = FlowDirection.LeftToRight, WrapContents = true
    };

    private RadioButton IconButton(string iconName, string text, Action action)
    {
        var button = new RadioButton
        {
            Text = text, Appearance = Appearance.Button, AutoCheck = false,
            TextAlign = ContentAlignment.MiddleCenter, UseVisualStyleBackColor = true
        };
        button.Click += (_, _) => RunAction(action);
        button.Size = new Size(90, 68);
        button.Margin = new Padding(0, 0, 6, 6);
        button.Image = EditorTheme.LoadIcon(iconName);
        button.ImageAlign = ContentAlignment.TopCenter;
        button.TextImageRelation = TextImageRelation.ImageAboveText;
        button.Padding = new Padding(2, 6, 2, 2);
        button.Font = new Font("Segoe UI", 8.5F);
        button.Disposed += (_, _) => button.Image?.Dispose();
        return button;
    }

    private static void MarkSelected(RadioButton button, bool selected) => button.Checked = selected;

    private static Label Title(string text) => new()
    {
        Text = text, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = EditorTheme.Text,
        Width = 280, Height = 34, Margin = new Padding(0, 6, 0, 4)
    };

    private static Label Note(string text, int height = 24) => new()
    {
        Text = text, ForeColor = EditorTheme.Muted, Width = 280,
        Height = height, Margin = new Padding(0, 3, 0, 3), AutoEllipsis = true
    };

    private static ComboBox Picker(params string[] items)
    {
        var picker = new ComboBox
        {
            Width = 280, DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = EditorTheme.Input, ForeColor = EditorTheme.Text
        };
        picker.Items.AddRange(items);
        picker.SelectedIndex = 0;
        return picker;
    }

    private Button ActionButton(string text, Action action)
    {
        var button = new Button
        {
            Text = text, Width = 280, Height = 36, Margin = new Padding(0, 8, 0, 4),
            UseVisualStyleBackColor = true
        };
        button.Click += (_, _) => RunAction(action);
        return button;
    }

    private static CheckBox Check(string text, bool value) => new()
    {
        Text = text, Checked = value, Width = 280, Height = 42,
        ForeColor = EditorTheme.Text, Margin = new Padding(0, 4, 0, 4)
    };

    private static NumericUpDown Number(decimal min, decimal max, decimal value,
        int decimals = 2, decimal increment = 0.1m) => new()
    {
        Minimum = min, Maximum = max, Value = value, DecimalPlaces = decimals,
        Increment = increment, TextAlign = HorizontalAlignment.Center,
        BorderStyle = BorderStyle.Fixed3D, BackColor = EditorTheme.Input, ForeColor = EditorTheme.Text
    };

    private static NumericUpDown[] Triple(decimal x, decimal y, decimal z, decimal min, decimal max) =>
        [Number(min, max, x), Number(min, max, y), Number(min, max, z)];

    private static Control Field(string label, Control control)
    {
        var panel = new Panel { Width = 280, Height = 62, Margin = new Padding(0, 3, 0, 4) };
        panel.Controls.Add(new Label { Text = label, Dock = DockStyle.Top, Height = 24, ForeColor = EditorTheme.Muted });
        control.Location = new Point(0, 28);
        control.Width = panel.Width;
        control.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        panel.Controls.Add(control);
        return panel;
    }

    private static Control TripleField(string label, NumericUpDown[] inputs)
    {
        var panel = new TableLayoutPanel { Width = 280, Height = 88, ColumnCount = 3, RowCount = 3, Margin = new Padding(0, 3, 0, 4) };
        for (var i = 0; i < 3; i++) panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / 3));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var caption = new Label { Text = label, Dock = DockStyle.Fill, ForeColor = EditorTheme.Muted };
        panel.Controls.Add(caption, 0, 0);
        panel.SetColumnSpan(caption, 3);
        var colors = new[] { Color.IndianRed, Color.ForestGreen, Color.RoyalBlue };
        for (var i = 0; i < inputs.Length; i++)
        {
            panel.Controls.Add(new Label { Text = "XYZ"[i].ToString(), ForeColor = colors[i], TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill }, i, 1);
            inputs[i].Dock = DockStyle.Fill;
            inputs[i].Margin = new Padding(1, 0, 4, 0);
            panel.Controls.Add(inputs[i], i, 2);
        }
        return panel;
    }
}
