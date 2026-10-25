using Lab6.Core.Scene;
using Lab6.Core.Mathematics;

namespace Lab6.Rendering;

public sealed class WireframeRenderer
{
    public void Render(Graphics graphics, Size viewport, SceneObject item, bool perspective)
    {
        var aspect = (double)viewport.Width / viewport.Height;
        var projection = perspective ? Projections.Perspective(aspect) : Projections.Axonometric(aspect);
        var points = Project(item, viewport, projection);
        using var pen = new Pen(Color.Black);
        DrawEdges(graphics, points, item.Geometry.Edges, pen);
    }

    // TODO [3]: Model → View → Projection, отсечение до деления на w,
    // затем деление на w и перевод в пиксели. Матрицы — участник 2 и Projections.
    public List<PointF> Project(SceneObject item, Size viewport, Matrix4 projection) =>
        throw new NotImplementedException("TODO [3]: проецирование точек на экран.");

    // Готовая инфраструктура вывода: собственная растеризация линий не нужна.
    public void DrawEdges(Graphics graphics, List<PointF> points,
        List<(int Start, int End)> edges, Pen pen)
    {
        foreach (var (start, end) in edges)
            graphics.DrawLine(pen, points[start], points[end]);
    }
}
