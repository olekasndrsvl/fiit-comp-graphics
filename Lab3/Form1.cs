namespace Lab3;

public partial class Form1 : Form
{
    private Bitmap? pattern; // изображение на все случаи жизни
    private Point? previousDrawingPoint;
    
    private Color boundaryColor = Color.FromArgb(38, 50, 56);
    private Color fillColor = Color.FromArgb(77, 182, 172);
    private Color traceColor = Color.FromArgb(255, 112, 67);
    private Color lineColor = Color.FromArgb(92, 107, 192);
    
    private readonly List<Point> linePoints = []; // для отрезка
    private readonly List<Point> trianglePoints = []; // для задания с треугольником
    private readonly Color[] vertexColors = [Color.Red, Color.Lime, Color.Blue];

    public Form1() => InitializeComponent();

    private void RunAlgorithm(Action action, DrawingCanvas canvas)
    {
        action();
        canvas.Invalidate();
    }

    private Bitmap? OpenBitmap(string title)
    {
        using var dialog = new OpenFileDialog
        {
            Title = title, Filter = "Изображения|*.png;*.jpg;*.jpeg", CheckFileExists = true
        };
        
        if (dialog.ShowDialog(this) != DialogResult.OK) return null;
        try
        {
            using var image = new Bitmap(dialog.FileName);
            return new Bitmap(image); // Отдельная копия не удерживает файл открытым.
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, exception.Message, "Ошибка загрузки", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }
    }

    private void LoadSourceImage()
    {
        var image = OpenBitmap("Изображение для заливки или обхода границы");
        if (image is not null) fillCanvas.SetImage(image);
    }

    private void LoadPattern()
    {
        var image = OpenBitmap("Рисунок для заливки");
        if (image is null) return;
        pattern?.Dispose();
        pattern = image;
    }

    private void FillCanvas_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left || !fillCanvas.ClientRectangle.Contains(e.Location))
            return;
        
        if (fillMode.SelectedIndex == 0)
        {
            fillCanvas.Capture = true;
            previousDrawingPoint = e.Location;
            fillCanvas.Image.SetPixel(e.X, e.Y, boundaryColor);
            fillCanvas.Invalidate();
            return;
        }
        
        if (fillMode.SelectedIndex == 2 && pattern is null)
            return;
        
        RunAlgorithm(() =>
        {
            switch (fillMode.SelectedIndex)
            {
                case 1: ScanlineFill.FillColor(fillCanvas.Image, e.Location, fillColor); break;
                case 2:
                    ScanlineFill.FillPattern(
                        fillCanvas.Image,
                        e.Location,
                        pattern!,
                        boundaryColor
                    );
                    break;
                case 3:
                    using (var image = new FastBitmap.FastBitmap(fillCanvas.Image))
                    {
                        var points = BoundaryTracer.Trace(image, e.Location, boundaryColor);
                        BoundaryTracer.DrawBoundary(image, points, traceColor);
                    }
                    break;
            }
        }, fillCanvas);
    }

    private void FillCanvas_MouseMove(object? sender, MouseEventArgs e)
    {
        if (previousDrawingPoint is not Point previous || e.Button != MouseButtons.Left || fillMode.SelectedIndex != 0)
            return;
        
        
        using var graphics = Graphics.FromImage(fillCanvas.Image);
        using var pen = new Pen(boundaryColor);
        
        graphics.DrawLine(pen, previous, e.Location);
        previousDrawingPoint = e.Location;
        
        fillCanvas.Invalidate();
    }

    private void LineCanvas_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left || !lineCanvas.ClientRectangle.Contains(e.Location))
            return;
        
        if (linePoints.Count == 2)
            linePoints.Clear();
        
        linePoints.Add(e.Location);
        
        UpdateLinePoints();
    }

    // для задания 2
    private void UpdateLinePoints()
    {
        linePointsLabel.Text = $"Начало: {PointText(linePoints, 0)}\nКонец: {PointText(linePoints, 1)}";
        lineCanvas.SetMarkers(linePoints);
    }

    private void DrawSelectedLine()
    {
        if (linePoints.Count != 2) return;
        RunAlgorithm(() =>
        {
            if (lineAlgorithm.SelectedIndex == 0) 
                LineRasterizer.DrawBresenham(lineCanvas.Image, linePoints[0], linePoints[1], lineColor);
            else 
                LineRasterizer.DrawWu(lineCanvas.Image, linePoints[0], linePoints[1], lineColor);
            
        }, lineCanvas);
    }

    // для треуголника
    private void TriangleCanvas_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left || !triangleCanvas.ClientRectangle.Contains(e.Location))
            return;
        
        if (trianglePoints.Count == 3)
            trianglePoints.Clear();
        
        trianglePoints.Add(e.Location);
        
        UpdateTrianglePoints();
    }

    private void UpdateTrianglePoints()
    {
        trianglePointsLabel.Text = $"A: {PointText(trianglePoints, 0)}\nB: {PointText(trianglePoints, 1)}\nC: {PointText(trianglePoints, 2)}";
        triangleCanvas.SetMarkers(trianglePoints);
    }

    private void DrawSelectedTriangle()
    {
        if (trianglePoints.Count != 3)
            return;
        
        if (vertexColors.Select(c => c.ToArgb()).Distinct().Count() != 3) 
            return;
        
        RunAlgorithm(() => TriangleRasterizer.Draw(triangleCanvas.Image,
            new ColoredVertex(trianglePoints[0], vertexColors[0]),
            new ColoredVertex(trianglePoints[1], vertexColors[1]),
            new ColoredVertex(trianglePoints[2], vertexColors[2])), triangleCanvas);
    }

    private static string PointText(List<Point> points, int index) => points.Count > index ? $"({points[index].X}, {points[index].Y})" : "—";
}
