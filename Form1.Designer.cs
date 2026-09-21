#nullable enable

namespace Lab2;

partial class Form1
{
    private System.ComponentModel.IContainer? components;
    private PictureBox sourcePreview = null!;
    private PictureBox grayscaleFirstPreview = null!;
    private PictureBox grayscaleSecondPreview = null!;
    private PictureBox differencePreview = null!;
    private PictureBox redChannelPreview = null!;
    private PictureBox greenChannelPreview = null!;
    private PictureBox blueChannelPreview = null!;
    private PictureBox hsvResultPreview = null!;
    private HistogramView grayscaleFirstHistogram = null!;
    private HistogramView grayscaleSecondHistogram = null!;
    private HistogramView redHistogram = null!;
    private HistogramView greenHistogram = null!;
    private HistogramView blueHistogram = null!;
    private Button openImageButton = null!;
    private Button clearImageButton = null!;
    private Button runGrayscaleButton = null!;
    private Button runChannelsButton = null!;
    private Button updateHsvButton = null!;
    private Button resetHsvButton = null!;
    private Button saveHsvButton = null!;
    private TrackBar hueTrackBar = null!;
    private TrackBar saturationTrackBar = null!;
    private TrackBar valueTrackBar = null!;
    private Label hueValueLabel = null!;
    private Label saturationValueLabel = null!;
    private Label valueValueLabel = null!;
    private CheckBox autoPreviewCheckBox = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
            sourceBitmap?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        SuspendLayout();
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = new Font("Segoe UI", 9F);
        BackColor = SystemColors.Control;
        ClientSize = new Size(1120, 680);
        MinimumSize = new Size(900, 560);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "RGB / HSV";

        var workspace = CreateGrid(2, 1);
        workspace.Padding = new Padding(6);
        workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        workspace.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        workspace.Controls.Add(CreateSourcePanel(), 0, 0);
        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(CreateGrayscaleTab());
        tabs.TabPages.Add(CreateChannelsTab());
        tabs.TabPages.Add(CreateHsvTab());
        workspace.Controls.Add(tabs, 1, 0);

        Controls.Add(workspace);
        ResumeLayout(true);
    }

    private Control CreateSourcePanel()
    {
        var panel = CreateGrid(1, 3);
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        for (var i = 0; i < 2; i++)
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        sourcePreview = CreatePictureBox();
        panel.Controls.Add(CreateLabeledView("Исходник", sourcePreview), 0, 0);
        openImageButton = CreateButton("Открыть...");
        openImageButton.Click += OpenImageButton_Click;
        clearImageButton = CreateButton("Очистить");
        clearImageButton.Enabled = false;
        clearImageButton.Click += ClearImageButton_Click;
        panel.Controls.Add(openImageButton, 0, 1);
        panel.Controls.Add(clearImageButton, 0, 2);
        return panel;
    }

    private TabPage CreateGrayscaleTab()
    {
        var tab = CreateTab("1. Оттенки серого");
        runGrayscaleButton = CreateButton("Рассчитать");
        runGrayscaleButton.Enabled = false;
        runGrayscaleButton.Click += RunGrayscaleButton_Click;

        grayscaleFirstPreview = CreatePictureBox();
        grayscaleSecondPreview = CreatePictureBox();
        differencePreview = CreatePictureBox();
        grayscaleFirstHistogram = new HistogramView();
        grayscaleSecondHistogram = new HistogramView();

        var grid = CreateGrid(3, 2);
        for (var i = 0; i < 3; i++)
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / 3));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 65));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
        grid.Controls.Add(CreateLabeledView("Формула 1", grayscaleFirstPreview), 0, 0);
        grid.Controls.Add(CreateLabeledView("Формула 2", grayscaleSecondPreview), 1, 0);
        grid.Controls.Add(CreateLabeledView("Разность", differencePreview), 2, 0);
        grid.Controls.Add(CreateLabeledView("Гистограмма 1", grayscaleFirstHistogram), 0, 1);
        grid.Controls.Add(CreateLabeledView("Гистограмма 2", grayscaleSecondHistogram), 1, 1);
        tab.Controls.Add(CreateTaskLayout(grid, runGrayscaleButton));
        return tab;
    }

    private TabPage CreateChannelsTab()
    {
        var tab = CreateTab("2. Каналы RGB");
        runChannelsButton = CreateButton("Рассчитать");
        runChannelsButton.Enabled = false;
        runChannelsButton.Click += RunChannelsButton_Click;
        redChannelPreview = CreatePictureBox();
        greenChannelPreview = CreatePictureBox();
        blueChannelPreview = CreatePictureBox();
        redHistogram = new HistogramView { SeriesColor = Color.Red };
        greenHistogram = new HistogramView { SeriesColor = Color.Green };
        blueHistogram = new HistogramView { SeriesColor = Color.Blue };

        var grid = CreateGrid(3, 2);
        for (var i = 0; i < 3; i++)
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / 3));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 65));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
        grid.Controls.Add(CreateLabeledView("R", redChannelPreview), 0, 0);
        grid.Controls.Add(CreateLabeledView("G", greenChannelPreview), 1, 0);
        grid.Controls.Add(CreateLabeledView("B", blueChannelPreview), 2, 0);
        grid.Controls.Add(CreateLabeledView("Гистограмма R", redHistogram), 0, 1);
        grid.Controls.Add(CreateLabeledView("Гистограмма G", greenHistogram), 1, 1);
        grid.Controls.Add(CreateLabeledView("Гистограмма B", blueHistogram), 2, 1);
        tab.Controls.Add(CreateTaskLayout(grid, runChannelsButton));
        return tab;
    }

    private TabPage CreateHsvTab()
    {
        var tab = CreateTab("3. HSV");
        saveHsvButton = CreateButton("Сохранить...");
        saveHsvButton.Enabled = false;
        saveHsvButton.Click += SaveHsvButton_Click;
        hsvResultPreview = CreatePictureBox();
        var body = CreateGrid(2, 1);
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        body.Controls.Add(CreateHsvControls(), 0, 0);
        body.Controls.Add(CreateLabeledView("Результат", hsvResultPreview), 1, 0);
        tab.Controls.Add(CreateTaskLayout(body, saveHsvButton));
        return tab;
    }

    private Control CreateHsvControls()
    {
        var panel = CreateGrid(1, 7);
        panel.AutoScroll = true;
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var i = 0; i < 6; i++)
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        hueTrackBar = CreateTrackBar(-180, 180);
        saturationTrackBar = CreateTrackBar(-100, 100);
        valueTrackBar = CreateTrackBar(-100, 100);
        hueValueLabel = CreateValueLabel("0°");
        saturationValueLabel = CreateValueLabel("0%");
        valueValueLabel = CreateValueLabel("0%");
        hueTrackBar.ValueChanged += HsvTrackBar_ValueChanged;
        saturationTrackBar.ValueChanged += HsvTrackBar_ValueChanged;
        valueTrackBar.ValueChanged += HsvTrackBar_ValueChanged;
        panel.Controls.Add(CreateSlider("H — оттенок", hueTrackBar, hueValueLabel), 0, 0);
        panel.Controls.Add(CreateSlider("S — насыщенность", saturationTrackBar, saturationValueLabel), 0, 1);
        panel.Controls.Add(CreateSlider("V — яркость", valueTrackBar, valueValueLabel), 0, 2);
        autoPreviewCheckBox = new CheckBox
        {
            Text = "Автообновление", Checked = true, AutoSize = true, Dock = DockStyle.Fill
        };
        panel.Controls.Add(autoPreviewCheckBox, 0, 3);
        updateHsvButton = CreateButton("Обновить");
        updateHsvButton.Enabled = false;
        updateHsvButton.Click += UpdateHsvButton_Click;
        panel.Controls.Add(updateHsvButton, 0, 4);
        resetHsvButton = CreateButton("Сброс");
        resetHsvButton.Click += ResetHsvButton_Click;
        panel.Controls.Add(resetHsvButton, 0, 5);
        return panel;
    }

    private static Control CreateTaskLayout(Control content, Button button)
    {
        var layout = CreateGrid(1, 2);
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        button.Dock = DockStyle.None;
        button.Anchor = AnchorStyles.Left;
        layout.Controls.Add(button, 0, 0);
        layout.Controls.Add(content, 0, 1);
        return layout;
    }

    private static Control CreateLabeledView(string title, Control content)
    {
        var panel = CreateGrid(1, 2);
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.Controls.Add(new Label { Text = title, AutoSize = true, Dock = DockStyle.Fill }, 0, 0);
        content.Dock = DockStyle.Fill;
        panel.Controls.Add(content, 0, 1);
        return panel;
    }

    private static Control CreateSlider(string title, TrackBar slider, Label value)
    {
        var row = CreateGrid(2, 2);
        row.AutoSize = true;
        row.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        row.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        row.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        row.Controls.Add(new Label { Text = title, AutoSize = true, Dock = DockStyle.Fill }, 0, 0);
        row.Controls.Add(value, 1, 0);
        row.Controls.Add(slider, 0, 1);
        row.SetColumnSpan(slider, 2);
        return row;
    }

    private static TableLayoutPanel CreateGrid(int columns, int rows) => new()
    {
        ColumnCount = columns, RowCount = rows, Dock = DockStyle.Fill, Margin = new Padding(3)
    };

    private static TabPage CreateTab(string title) => new()
    {
        Text = title, BackColor = SystemColors.Control, Padding = new Padding(3)
    };

    private static Button CreateButton(string text) => new()
    {
        Text = text, AutoSize = true, MinimumSize = new Size(100, 28),
        Dock = DockStyle.Fill, UseVisualStyleBackColor = true
    };

    private static PictureBox CreatePictureBox() => new()
    {
        Dock = DockStyle.Fill,
        SizeMode = PictureBoxSizeMode.Zoom,
        BorderStyle = BorderStyle.FixedSingle,
        BackColor = SystemColors.Window,
        MinimumSize = new Size(100, 80),
        TabStop = false
    };

    private static TrackBar CreateTrackBar(int minimum, int maximum) => new ResettableTrackBar()
    {
        Minimum = minimum, Maximum = maximum, Value = 0,
        TickStyle = TickStyle.None, Dock = DockStyle.Fill
    };

    private static Label CreateValueLabel(string text) => new()
    {
        Text = text, AutoSize = true, TextAlign = ContentAlignment.MiddleRight,
        Anchor = AnchorStyles.Top | AnchorStyles.Right
    };
}
