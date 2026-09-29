namespace Lab3;
// умная обертка над Panel для работы с пикселями и изображениями
public sealed class DrawingCanvas : Panel
{
    private Bitmap bitmap = new(1, 1);
    private Point[] markers = [];
    public Bitmap Image => bitmap;

    public DrawingCanvas()
    {
        DoubleBuffered = true;
        BackColor = Color.White;
        Cursor = Cursors.Cross;
        BorderStyle = BorderStyle.FixedSingle;
        ResizeRedraw = true;
    }

    public void SetImage(Bitmap image)
    {
        var newBitmap = CreateCanvasBitmap();
        using (var graphics = Graphics.FromImage(newBitmap))
        {
            graphics.Clear(Color.White);
            graphics.DrawImageUnscaled(image, 0, 0);
        }
        
        
        image.Dispose();
        bitmap.Dispose();
        
        bitmap = newBitmap;
        markers = [];
        
        Invalidate();
    }

    public void Clear()
    {
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.White);
        markers = [];
        Invalidate();
    }

    public void SetMarkers(IEnumerable<Point> points)
    {
        markers = points.ToArray();
        Invalidate();
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        
        var newBitmap = CreateCanvasBitmap();
        
        using (var graphics = Graphics.FromImage(newBitmap))
        {
            graphics.Clear(Color.White);
            graphics.DrawImageUnscaled(bitmap, 0, 0);
        }
        bitmap.Dispose();
        
        bitmap = newBitmap;
        
        Invalidate();
    }

    private Bitmap CreateCanvasBitmap() => new(Math.Max(1, ClientSize.Width), Math.Max(1, ClientSize.Height));

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        
        e.Graphics.DrawImageUnscaled(bitmap, 0, 0);
        
        // просто рисование точек поставленных мышкой
        for (var i = 0; i < markers.Length; i++)
        {
            var point = markers[i];
            e.Graphics.FillEllipse(Brushes.Black, point.X - 2, point.Y - 2, 5, 5);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) 
            bitmap.Dispose();
        
        base.Dispose(disposing);
    }
}
