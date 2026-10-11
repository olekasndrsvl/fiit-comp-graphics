#nullable enable

namespace Task1;

partial class Form1
{
    private System.ComponentModel.IContainer? components;
    private DoubleBufferedPanel drawingPanel = null!;
    private Label pointsValueLabel = null!, hullValueLabel = null!, statusLabel = null!;
    private Button buildButton = null!, clearButton = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        SuspendLayout();
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = new Font("Segoe UI", 9F);
        ClientSize = new Size(1120, 720);
        MinimumSize = new Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "QuickHull";

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(CreateQuickHullTab());
        Controls.Add(tabs);
        ResumeLayout(true);
    }

    private TabPage CreateQuickHullTab()
    {
        drawingPanel = new DoubleBufferedPanel
        {
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Cursor = Cursors.Cross
        };
        drawingPanel.Paint += DrawingPanel_Paint;
        drawingPanel.MouseClick += DrawingPanel_MouseClick;

        pointsValueLabel = ValueLabel("Выбрано точек: 0");
        hullValueLabel = ValueLabel("Вершин оболочки: 0");
        statusLabel = new Label
        {
            Text = string.Empty,
            AutoSize = false,
            Height = 54,
            TextAlign = ContentAlignment.TopLeft
        };

        buildButton = ActionButton("Построить оболочку", BuildButton_Click);
        clearButton = ActionButton("Очистить", ClearButton_Click);

        return TaskTab("Алгоритм QuickHull", drawingPanel,
            pointsValueLabel,
            hullValueLabel,
            buildButton,
            clearButton,
            statusLabel);
    }

    private static TabPage TaskTab(string title, Control canvas, params Control[] controls)
    {
        var tab = new TabPage(title)
        {
            Padding = new Padding(3),
            BackColor = SystemColors.Control
        };
        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var sidebar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true
        };
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

    private static Label ValueLabel(string text) => new()
    {
        Text = text,
        AutoSize = true
    };

    private static Button ActionButton(string text, EventHandler action)
    {
        var button = new Button
        {
            Text = text,
            Height = 32,
            UseVisualStyleBackColor = true
        };
        button.Click += action;
        return button;
    }
}
