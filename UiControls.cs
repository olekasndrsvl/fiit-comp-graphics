using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Lab2;

// это просто нейронкой запилил удобную функцию, что полузнок сбрасывался при двойном клике
public sealed class ResettableTrackBar : TrackBar
{
    private long? previousMouseDownTime;
    private Point previousMouseDownPosition;
    private bool suppressMouseUp;

    protected override void WndProc(ref Message m)
    {
        const int wmMouseMove = 0x0200;
        const int wmLeftButtonDown = 0x0201;
        const int wmLeftButtonUp = 0x0202;
        const int wmLeftButtonDoubleClick = 0x0203;

       
        if (m.Msg == wmLeftButtonDown || m.Msg == wmLeftButtonDoubleClick)
        {
            var position = MousePositionFromMessage(m);
            var now = Environment.TickCount64;
            if (previousMouseDownTime is long previousTime &&
                now - previousTime <= SystemInformation.DoubleClickTime &&
                IsNearPreviousPress(position))
            {
                previousMouseDownTime = null;
                suppressMouseUp = true;
                Focus();
                Value = Math.Clamp(0, Minimum, Maximum);
                return;
            }

            suppressMouseUp = false;
            previousMouseDownTime = now;
            previousMouseDownPosition = position;
        }
        else if (m.Msg == wmMouseMove && (m.WParam.ToInt64() & 1) != 0 &&
                 !IsNearPreviousPress(MousePositionFromMessage(m)))
        {
            previousMouseDownTime = null; 
        }
        else if (m.Msg == wmLeftButtonUp && suppressMouseUp)
        {
            suppressMouseUp = false;
            return;
        }

        base.WndProc(ref m);
    }

    private bool IsNearPreviousPress(Point position)
    {
        var size = SystemInformation.DoubleClickSize;
        return new Rectangle(
            previousMouseDownPosition.X - size.Width / 2,
            previousMouseDownPosition.Y - size.Height / 2,
            size.Width, size.Height).Contains(position);
    }

    private static Point MousePositionFromMessage(Message message)
    {
        var coordinates = message.LParam.ToInt64();
        return new Point(unchecked((short)coordinates), unchecked((short)(coordinates >> 16)));
    }
}



// компонент для рисования гистграмм
public sealed class HistogramView : Control
{
    private int[]? _values;

    public HistogramView()
    {
        DoubleBuffered = true;
        BackColor = SystemColors.Window;
        ForeColor = SystemColors.WindowText;
        MinimumSize = new Size(120, 90);
    }

    [DefaultValue(typeof(Color), "Black")]
    public Color SeriesColor { get; set; } = Color.Black;

    public void SetValues(IEnumerable<int>? values)
    {
        _values = values?.ToArray();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = Rectangle.Inflate(ClientRectangle, -10, -10);
        if (bounds.Width < 20 || bounds.Height < 20)
        {
            return;
        }

        DrawGrid(e.Graphics, bounds);
        if (_values is null || _values.Length == 0 || _values.Max() <= 0)
        {
            return;
        }

        var maximum = _values.Max();
        var points = new PointF[_values.Length];
        for (var i = 0; i < _values.Length; i++)
        {
            var x = bounds.Left + i * bounds.Width / (float)Math.Max(1, _values.Length - 1);
            var y = bounds.Bottom - _values[i] * bounds.Height / (float)maximum;
            points[i] = new PointF(x, y);
        }

        using var linePen = new Pen(SeriesColor);
        if (points.Length == 1)
            e.Graphics.DrawLine(linePen, points[0], new PointF(points[0].X, bounds.Bottom));
        else
            e.Graphics.DrawLines(linePen, points);
    }

    private static void DrawGrid(Graphics graphics, Rectangle bounds)
    {
        using var gridPen = new Pen(SystemColors.ControlLight);
        for (var i = 0; i <= 4; i++)
        {
            var y = bounds.Top + i * bounds.Height / 4;
            graphics.DrawLine(gridPen, bounds.Left, y, bounds.Right, y);
        }
    }
}
