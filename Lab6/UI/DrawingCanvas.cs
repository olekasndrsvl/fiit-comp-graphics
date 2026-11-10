using System.ComponentModel;
using Lab6.Core.Scene;
using Lab6.Rendering;

namespace Lab6.UI;

public sealed class DrawingCanvas : Panel
{
    private const double RotationSpeed = 1.5; // рад/сек

    private readonly WireframeRenderer renderer = new();
    private readonly System.Windows.Forms.Timer rotationTimer;
    private readonly System.Diagnostics.Stopwatch rotationClock = System.Diagnostics.Stopwatch.StartNew();
    private double lastRotationSeconds;
    private double autoRotationAngle;

    public event Action<string>? RenderPending;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Scene? Scene { get; set; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Perspective { get; set; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ShowAxes { get; set; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Camera Camera => renderer.Camera;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool BackfaceCulling
    {
        get => renderer.BackfaceCulling;
        set { renderer.BackfaceCulling = value; Invalidate(); }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsRotating
    {
        get => rotationTimer.Enabled;
        set
        {
            if (value == rotationTimer.Enabled) return;
            if (value)
            {
                lastRotationSeconds = rotationClock.Elapsed.TotalSeconds;
                rotationTimer.Start();
            }
            else
            {
                rotationTimer.Stop();
            }
        }
    }

    public DrawingCanvas()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.White;
        BorderStyle = BorderStyle.FixedSingle;

        rotationTimer = new System.Windows.Forms.Timer { Interval = 16 };
        rotationTimer.Tick += OnRotationTick;
    }

    private void OnRotationTick(object? sender, EventArgs e)
    {
        var now = rotationClock.Elapsed.TotalSeconds;
        var delta = now - lastRotationSeconds;
        lastRotationSeconds = now;
        autoRotationAngle += delta * RotationSpeed;
        renderer.ObjectRotationAngle = autoRotationAngle;
        Invalidate();
    }

    public void RefreshFrame() => Invalidate();

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (Scene is null || Scene.Objects.Count == 0 || ClientSize.Width <= 0 || ClientSize.Height <= 0)
            return;
        try
        {
            renderer.Render(e.Graphics, ClientSize, Scene.Objects, Scene.SelectedObject, Perspective, ShowAxes);
        }
        catch (NotImplementedException ex)
        {
            RenderPending?.Invoke(ex.Message);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) rotationTimer.Dispose();
        base.Dispose(disposing);
    }
}