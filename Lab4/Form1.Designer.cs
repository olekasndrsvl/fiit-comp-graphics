#nullable enable

namespace Lab4;

partial class Form1
{
    private System.ComponentModel.IContainer? components;

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
        Text = "Lab4 - Affine Geometry";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1160, 860);
        ClientSize = new Size(1320, 920);
        BuildInterface();
    }
}
