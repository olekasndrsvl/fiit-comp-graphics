using System.ComponentModel;
using Lab6.Core.Scene;
using Lab6.Rendering;

namespace Lab6.UI;

public sealed class DrawingCanvas : Panel
{
    private readonly WireframeRenderer renderer = new();
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
    public bool ZBufferEnabled { get; set; }

    public DrawingCanvas()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.White;
        BorderStyle = BorderStyle.FixedSingle;
    }

    public void RefreshFrame()
    {
        // WinForms объединяет запросы обновления и вызывает Paint.
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (Scene is null || Scene.Objects.Count == 0 || ClientSize.Width <= 0 || ClientSize.Height <= 0)
            return;
        try
        {
            renderer.Render(e.Graphics, ClientSize, Scene.Objects, Scene.SelectedObject, Perspective, ShowAxes, ZBufferEnabled);
        }
        catch (NotImplementedException ex)
        {
            // Каркас может запускаться, даже если проецирование ещё не реализовано.
            RenderPending?.Invoke(ex.Message);
        }
    }
}

