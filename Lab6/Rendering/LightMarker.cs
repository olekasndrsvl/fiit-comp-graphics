using System.Drawing.Drawing2D;
using Lab6.Core.Geometry;
using Lab6.Core.Mathematics;
using MathNet.Numerics.LinearAlgebra;

namespace Lab6.Rendering;

/*
 Тут у нас рисуется солнышко, рисуется оно в экранных координатах поверх кадра по сути,
 благодаря чему он всегда и виден.
 */
internal static class LightMarker
{
    private const double MinimumW = 1e-6;
    private const float MarkerMargin = 28f;
    private const float GlowRadius = 18f;
    private const float CoreRadius = 9f;
    private const float RayStart = 12f;
    private const float RayEnd = 22f;
    private const int RayCount = 8;

    public static void Draw(Graphics graphics, Size viewport, Matrix<double> view, Matrix<double> projection,
        bool perspective, WireframeRenderer.ScreenFrame frame, Point3D position, Color lightColor)
    {
        if (!TryLocate(viewport, view, view * projection, perspective, frame, position,
                out var center, out var onScreen))
        {
            return;
        }

        var smoothing = graphics.SmoothingMode;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        try
        {
            // Палитра «солнышка» фиксированная и тёплая: маркер должен быть виден на белом
            // фоне при любом цвете источника. Цвет источника показываем внутренним диском.
            var sunEdge = Color.FromArgb(226, 150, 20);
            var sunCore = Color.FromArgb(255, 205, 60);
            const float tintRadius = 4f;

            using var glowBrush = new SolidBrush(Color.FromArgb(onScreen ? 56 : 40, sunEdge));
            graphics.FillEllipse(glowBrush,
                center.X - GlowRadius, center.Y - GlowRadius, GlowRadius * 2, GlowRadius * 2);

            using var rayPen = new Pen(sunEdge, 2f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };

            for (var i = 0; i < RayCount; i++)
            {
                var angle = Math.PI * 2 * i / RayCount;
                var dx = (float)Math.Cos(angle);
                var dy = (float)Math.Sin(angle);

                graphics.DrawLine(
                    rayPen,
                    center.X + dx * RayStart, center.Y + dy * RayStart,
                    center.X + dx * RayEnd, center.Y + dy * RayEnd);
            }

            using var coreBrush = new SolidBrush(sunCore);
            using var outlinePen = new Pen(sunEdge, 1.5f);

            graphics.FillEllipse(coreBrush,
                center.X - CoreRadius, center.Y - CoreRadius, CoreRadius * 2, CoreRadius * 2);
            graphics.DrawEllipse(outlinePen,
                center.X - CoreRadius, center.Y - CoreRadius, CoreRadius * 2, CoreRadius * 2);

            using var tintBrush = new SolidBrush(Color.FromArgb(lightColor.R, lightColor.G, lightColor.B));
            graphics.FillEllipse(tintBrush,
                center.X - tintRadius, center.Y - tintRadius, tintRadius * 2, tintRadius * 2);
        }
        finally
        {
            graphics.SmoothingMode = smoothing;
        }
    }

    /// Экранная позиция маркера источника (для перетаскивания мышью)
    internal static bool TryGetScreenPosition(
        Size viewport,
        Matrix<double> view,
        Matrix<double> projection,
        bool perspective,
        WireframeRenderer.ScreenFrame frame,
        Point3D position,
        out PointF screen,
        out bool onScreen) =>
        TryLocate(viewport, view, view * projection, perspective, frame, position, out screen, out onScreen);

    /// Проекция источника в экранные координаты. Если точка вне кадра, координата
    /// прижимается к границе, а onScreen = false (ореол рисуется слабее)
    private static bool TryLocate(Size viewport, Matrix<double> view, Matrix<double> viewProjection, bool perspective,
        WireframeRenderer.ScreenFrame frame, Point3D position, out PointF screen, out bool onScreen)
    {
        screen = PointF.Empty;
        onScreen = false;

        var (x, y, _, w) = TransformMatrix.TransformHomogeneous(viewProjection, position);

        double screenX;
        double screenY;

        if (perspective)
        {
            if (w <= MinimumW)
            {
                var viewPoint = TransformMatrix.Transform(view, position);
                var dx = viewPoint.X;
                var dy = -viewPoint.Y;
                var length = Math.Sqrt(dx * dx + dy * dy);

                if (length < MinimumW)
                {
                    dx = 0;
                    dy = 1;
                    length = 1;
                }

                return ClampToBorder(viewport, dx / length, dy / length, out screen);
            }

            screenX = viewport.Width / 2d + x / w * viewport.Width / 2d;
            screenY = viewport.Height / 2d - y / w * viewport.Height / 2d;
        }
        else
        {
            screenX = viewport.Width / 2d + (x - frame.CenterX) * frame.Scale;
            screenY = viewport.Height / 2d - (y - frame.CenterY) * frame.Scale;
        }

        if (!double.IsFinite(screenX) || !double.IsFinite(screenY))
            return false;

        onScreen =
            screenX >= MarkerMargin && screenX <= viewport.Width - MarkerMargin &&
            screenY >= MarkerMargin && screenY <= viewport.Height - MarkerMargin;

        var maxX = Math.Max(MarkerMargin, viewport.Width - MarkerMargin);
        var maxY = Math.Max(MarkerMargin, viewport.Height - MarkerMargin);

        screen = new PointF(
            (float)Math.Clamp(screenX, MarkerMargin, maxX),
            (float)Math.Clamp(screenY, MarkerMargin, maxY));

        return true;
    }

    private static bool ClampToBorder(Size viewport, double directionX, double directionY, out PointF screen)
    {
        screen = PointF.Empty;

        var centerX = viewport.Width / 2d;
        var centerY = viewport.Height / 2d;
        var halfWidth = Math.Max(1, centerX - MarkerMargin);
        var halfHeight = Math.Max(1, centerY - MarkerMargin);

        var scale = Math.Min(
            Math.Abs(directionX) < 1e-9 ? double.MaxValue : halfWidth / Math.Abs(directionX),
            Math.Abs(directionY) < 1e-9 ? double.MaxValue : halfHeight / Math.Abs(directionY));

        if (!double.IsFinite(scale))
            scale = Math.Min(halfWidth, halfHeight);

        screen = new PointF(
            (float)(centerX + directionX * scale),
            (float)(centerY + directionY * scale));

        return true;
    }
}