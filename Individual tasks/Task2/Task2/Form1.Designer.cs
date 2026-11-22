namespace Task2;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        mainSplit = new SplitContainer();
        renderViewport = new PictureBox();
        controlsPanel = new FlowLayoutPanel();
        objectGroup = new GroupBox();
        objectSelector = new ComboBox();
        mirrorOnButton = new Button();
        mirrorOffButton = new Button();
        objectStateLabel = new Label();
        transparencyGroup = new GroupBox();
        transparencyOnButton = new Button();
        transparencyOffButton = new Button();
        wallGroup = new GroupBox();
        wallSelector = new ComboBox();
        wallMirrorOnButton = new Button();
        wallMirrorOffButton = new Button();
        wallStateLabel = new Label();
        lightGroup = new GroupBox();
        lightSelector = new ComboBox();
        lightOnButton = new Button();
        lightOffButton = new Button();
        xLabel = new Label();
        yLabel = new Label();
        zLabel = new Label();
        lightX = new NumericUpDown();
        lightY = new NumericUpDown();
        lightZ = new NumericUpDown();
        lightStateLabel = new Label();
        actionsGroup = new GroupBox();
        renderButton = new Button();
        resetButton = new Button();
        renderStatusLabel = new Label();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        ((System.ComponentModel.ISupportInitialize)mainSplit).BeginInit();
        mainSplit.Panel1.SuspendLayout();
        mainSplit.Panel2.SuspendLayout();
        mainSplit.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)renderViewport).BeginInit();
        controlsPanel.SuspendLayout();
        objectGroup.SuspendLayout();
        transparencyGroup.SuspendLayout();
        wallGroup.SuspendLayout();
        lightGroup.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)lightX).BeginInit();
        ((System.ComponentModel.ISupportInitialize)lightY).BeginInit();
        ((System.ComponentModel.ISupportInitialize)lightZ).BeginInit();
        actionsGroup.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();

        // mainSplit
        mainSplit.Dock = DockStyle.Fill;
        mainSplit.FixedPanel = FixedPanel.Panel2;
        mainSplit.Location = new Point(0, 0);
        mainSplit.Name = "mainSplit";
        mainSplit.Panel1.Controls.Add(renderViewport);
        mainSplit.Panel1.Padding = new Padding(12);
        mainSplit.Panel2.Controls.Add(controlsPanel);
        mainSplit.Panel2.Padding = new Padding(8, 12, 12, 12);
        mainSplit.Size = new Size(1360, 790);
        mainSplit.SplitterDistance = 1010;
        mainSplit.TabIndex = 0;

        // renderViewport
        renderViewport.BackColor = Color.FromArgb(24, 24, 28);
        renderViewport.BorderStyle = BorderStyle.FixedSingle;
        renderViewport.Dock = DockStyle.Fill;
        renderViewport.Location = new Point(12, 12);
        renderViewport.Name = "renderViewport";
        renderViewport.Size = new Size(986, 766);
        renderViewport.SizeMode = PictureBoxSizeMode.Zoom;
        renderViewport.TabIndex = 0;
        renderViewport.TabStop = false;
        renderViewport.Paint += RenderViewport_Paint;

        // controlsPanel
        controlsPanel.AutoScroll = true;
        controlsPanel.Controls.Add(objectGroup);
        controlsPanel.Controls.Add(transparencyGroup);
        controlsPanel.Controls.Add(wallGroup);
        controlsPanel.Controls.Add(lightGroup);
        controlsPanel.Controls.Add(actionsGroup);
        controlsPanel.Dock = DockStyle.Fill;
        controlsPanel.FlowDirection = FlowDirection.TopDown;
        controlsPanel.Location = new Point(8, 12);
        controlsPanel.Name = "controlsPanel";
        controlsPanel.Size = new Size(326, 766);
        controlsPanel.TabIndex = 0;
        controlsPanel.WrapContents = false;

        // objectGroup
        objectGroup.Controls.Add(objectSelector);
        objectGroup.Controls.Add(mirrorOnButton);
        objectGroup.Controls.Add(mirrorOffButton);
        objectGroup.Controls.Add(objectStateLabel);
        objectGroup.Location = new Point(3, 3);
        objectGroup.Name = "objectGroup";
        objectGroup.Size = new Size(300, 134);
        objectGroup.TabIndex = 0;
        objectGroup.TabStop = false;
        objectGroup.Text = "Объект и зеркальность";

        objectSelector.DropDownStyle = ComboBoxStyle.DropDownList;
        objectSelector.Location = new Point(14, 27);
        objectSelector.Name = "objectSelector";
        objectSelector.Size = new Size(270, 23);
        objectSelector.TabIndex = 0;
        objectSelector.SelectedIndexChanged += ObjectSelector_SelectedIndexChanged;

        mirrorOnButton.Location = new Point(14, 61);
        mirrorOnButton.Name = "mirrorOnButton";
        mirrorOnButton.Size = new Size(130, 31);
        mirrorOnButton.TabIndex = 1;
        mirrorOnButton.Text = "Включить";
        mirrorOnButton.UseVisualStyleBackColor = true;
        mirrorOnButton.Click += MirrorOnButton_Click;

        mirrorOffButton.Location = new Point(154, 61);
        mirrorOffButton.Name = "mirrorOffButton";
        mirrorOffButton.Size = new Size(130, 31);
        mirrorOffButton.TabIndex = 2;
        mirrorOffButton.Text = "Выключить";
        mirrorOffButton.UseVisualStyleBackColor = true;
        mirrorOffButton.Click += MirrorOffButton_Click;

        objectStateLabel.AutoSize = true;
        objectStateLabel.Location = new Point(14, 104);
        objectStateLabel.Name = "objectStateLabel";
        objectStateLabel.Size = new Size(118, 15);
        objectStateLabel.TabIndex = 3;
        objectStateLabel.Text = "Зеркальность: выкл.";

        // transparencyGroup
        transparencyGroup.Controls.Add(transparencyOnButton);
        transparencyGroup.Controls.Add(transparencyOffButton);
        transparencyGroup.Location = new Point(3, 143);
        transparencyGroup.Name = "transparencyGroup";
        transparencyGroup.Size = new Size(300, 77);
        transparencyGroup.TabIndex = 1;
        transparencyGroup.TabStop = false;
        transparencyGroup.Text = "Прозрачность выбранного объекта";

        transparencyOnButton.Location = new Point(14, 29);
        transparencyOnButton.Name = "transparencyOnButton";
        transparencyOnButton.Size = new Size(130, 31);
        transparencyOnButton.TabIndex = 0;
        transparencyOnButton.Text = "Включить";
        transparencyOnButton.UseVisualStyleBackColor = true;
        transparencyOnButton.Click += TransparencyOnButton_Click;

        transparencyOffButton.Location = new Point(154, 29);
        transparencyOffButton.Name = "transparencyOffButton";
        transparencyOffButton.Size = new Size(130, 31);
        transparencyOffButton.TabIndex = 1;
        transparencyOffButton.Text = "Выключить";
        transparencyOffButton.UseVisualStyleBackColor = true;
        transparencyOffButton.Click += TransparencyOffButton_Click;

        // wallGroup
        wallGroup.Controls.Add(wallSelector);
        wallGroup.Controls.Add(wallMirrorOnButton);
        wallGroup.Controls.Add(wallMirrorOffButton);
        wallGroup.Controls.Add(wallStateLabel);
        wallGroup.Location = new Point(3, 226);
        wallGroup.Name = "wallGroup";
        wallGroup.Size = new Size(300, 134);
        wallGroup.TabIndex = 2;
        wallGroup.TabStop = false;
        wallGroup.Text = "Зеркальная стена";

        wallSelector.DropDownStyle = ComboBoxStyle.DropDownList;
        wallSelector.Location = new Point(14, 27);
        wallSelector.Name = "wallSelector";
        wallSelector.Size = new Size(270, 23);
        wallSelector.TabIndex = 0;

        wallMirrorOnButton.Location = new Point(14, 61);
        wallMirrorOnButton.Name = "wallMirrorOnButton";
        wallMirrorOnButton.Size = new Size(130, 31);
        wallMirrorOnButton.TabIndex = 1;
        wallMirrorOnButton.Text = "Включить";
        wallMirrorOnButton.UseVisualStyleBackColor = true;
        wallMirrorOnButton.Click += WallMirrorOnButton_Click;

        wallMirrorOffButton.Location = new Point(154, 61);
        wallMirrorOffButton.Name = "wallMirrorOffButton";
        wallMirrorOffButton.Size = new Size(130, 31);
        wallMirrorOffButton.TabIndex = 2;
        wallMirrorOffButton.Text = "Выключить";
        wallMirrorOffButton.UseVisualStyleBackColor = true;
        wallMirrorOffButton.Click += WallMirrorOffButton_Click;

        wallStateLabel.AutoSize = true;
        wallStateLabel.Location = new Point(14, 104);
        wallStateLabel.Name = "wallStateLabel";
        wallStateLabel.Size = new Size(139, 15);
        wallStateLabel.TabIndex = 3;
        wallStateLabel.Text = "Зеркальная стена: нет";

        // lightGroup
        lightGroup.Controls.Add(lightSelector);
        lightGroup.Controls.Add(lightOnButton);
        lightGroup.Controls.Add(lightOffButton);
        lightGroup.Controls.Add(xLabel);
        lightGroup.Controls.Add(yLabel);
        lightGroup.Controls.Add(zLabel);
        lightGroup.Controls.Add(lightX);
        lightGroup.Controls.Add(lightY);
        lightGroup.Controls.Add(lightZ);
        lightGroup.Controls.Add(lightStateLabel);
        lightGroup.Location = new Point(3, 366);
        lightGroup.Name = "lightGroup";
        lightGroup.Size = new Size(300, 190);
        lightGroup.TabIndex = 3;
        lightGroup.TabStop = false;
        lightGroup.Text = "Источники света";

        lightSelector.DropDownStyle = ComboBoxStyle.DropDownList;
        lightSelector.Location = new Point(14, 27);
        lightSelector.Name = "lightSelector";
        lightSelector.Size = new Size(270, 23);
        lightSelector.TabIndex = 0;
        lightSelector.SelectedIndexChanged += LightSelector_SelectedIndexChanged;

        lightOnButton.Location = new Point(14, 61);
        lightOnButton.Name = "lightOnButton";
        lightOnButton.Size = new Size(130, 31);
        lightOnButton.TabIndex = 1;
        lightOnButton.Text = "Включить";
        lightOnButton.UseVisualStyleBackColor = true;
        lightOnButton.Click += LightOnButton_Click;

        lightOffButton.Location = new Point(154, 61);
        lightOffButton.Name = "lightOffButton";
        lightOffButton.Size = new Size(130, 31);
        lightOffButton.TabIndex = 2;
        lightOffButton.Text = "Выключить";
        lightOffButton.UseVisualStyleBackColor = true;
        lightOffButton.Click += LightOffButton_Click;

        xLabel.AutoSize = true;
        xLabel.Location = new Point(14, 108);
        xLabel.Text = "X";
        yLabel.AutoSize = true;
        yLabel.Location = new Point(108, 108);
        yLabel.Text = "Y";
        zLabel.AutoSize = true;
        zLabel.Location = new Point(202, 108);
        zLabel.Text = "Z";

        ConfigureCoordinateInput(lightX, 34, 104);
        ConfigureCoordinateInput(lightY, 128, 104);
        ConfigureCoordinateInput(lightZ, 222, 104);
        lightX.ValueChanged += LightPosition_ValueChanged;
        lightY.ValueChanged += LightPosition_ValueChanged;
        lightZ.ValueChanged += LightPosition_ValueChanged;

        lightStateLabel.AutoSize = true;
        lightStateLabel.Location = new Point(14, 157);
        lightStateLabel.Name = "lightStateLabel";
        lightStateLabel.Size = new Size(68, 15);
        lightStateLabel.TabIndex = 9;
        lightStateLabel.Text = "Выключен";

        // actionsGroup
        actionsGroup.Controls.Add(renderButton);
        actionsGroup.Controls.Add(resetButton);
        actionsGroup.Controls.Add(renderStatusLabel);
        actionsGroup.Location = new Point(3, 562);
        actionsGroup.Name = "actionsGroup";
        actionsGroup.Size = new Size(300, 126);
        actionsGroup.TabIndex = 4;
        actionsGroup.TabStop = false;
        actionsGroup.Text = "Сцена";

        renderButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        renderButton.Location = new Point(14, 27);
        renderButton.Name = "renderButton";
        renderButton.Size = new Size(166, 38);
        renderButton.TabIndex = 0;
        renderButton.Text = "Рендер";
        renderButton.UseVisualStyleBackColor = true;
        renderButton.Click += RenderButton_Click;

        resetButton.Location = new Point(190, 27);
        resetButton.Name = "resetButton";
        resetButton.Size = new Size(94, 38);
        resetButton.TabIndex = 1;
        resetButton.Text = "Сбросить";
        resetButton.UseVisualStyleBackColor = true;
        resetButton.Click += ResetButton_Click;

        renderStatusLabel.Location = new Point(14, 77);
        renderStatusLabel.Name = "renderStatusLabel";
        renderStatusLabel.Size = new Size(270, 37);
        renderStatusLabel.TabIndex = 2;
        renderStatusLabel.Text = "Выберите параметры и запустите рендер.";

        // statusStrip
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
        statusStrip.Location = new Point(0, 790);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1360, 22);
        statusStrip.TabIndex = 1;
        statusLabel.Name = "statusLabel";
        statusLabel.Text = "Готово";

        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1360, 812);
        Controls.Add(mainSplit);
        Controls.Add(statusStrip);
        MinimumSize = new Size(1100, 720);
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Корнуэльская комната — трассировка лучей";
        mainSplit.Panel1.ResumeLayout(false);
        mainSplit.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)mainSplit).EndInit();
        mainSplit.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)renderViewport).EndInit();
        controlsPanel.ResumeLayout(false);
        objectGroup.ResumeLayout(false);
        objectGroup.PerformLayout();
        transparencyGroup.ResumeLayout(false);
        wallGroup.ResumeLayout(false);
        wallGroup.PerformLayout();
        lightGroup.ResumeLayout(false);
        lightGroup.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)lightX).EndInit();
        ((System.ComponentModel.ISupportInitialize)lightY).EndInit();
        ((System.ComponentModel.ISupportInitialize)lightZ).EndInit();
        actionsGroup.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private static void ConfigureCoordinateInput(NumericUpDown input, int x, int y)
    {
        input.DecimalPlaces = 1;
        input.Increment = 0.5m;
        input.Location = new Point(x, y);
        input.Maximum = 10m;
        input.Minimum = -10m;
        input.Size = new Size(62, 23);
    }

    #endregion

    private SplitContainer mainSplit;
    private PictureBox renderViewport;
    private FlowLayoutPanel controlsPanel;
    private GroupBox objectGroup;
    private ComboBox objectSelector;
    private Button mirrorOnButton;
    private Button mirrorOffButton;
    private Label objectStateLabel;
    private GroupBox transparencyGroup;
    private Button transparencyOnButton;
    private Button transparencyOffButton;
    private GroupBox wallGroup;
    private ComboBox wallSelector;
    private Button wallMirrorOnButton;
    private Button wallMirrorOffButton;
    private Label wallStateLabel;
    private GroupBox lightGroup;
    private ComboBox lightSelector;
    private Button lightOnButton;
    private Button lightOffButton;
    private Label xLabel;
    private Label yLabel;
    private Label zLabel;
    private NumericUpDown lightX;
    private NumericUpDown lightY;
    private NumericUpDown lightZ;
    private Label lightStateLabel;
    private GroupBox actionsGroup;
    private Button renderButton;
    private Button resetButton;
    private Label renderStatusLabel;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
}
