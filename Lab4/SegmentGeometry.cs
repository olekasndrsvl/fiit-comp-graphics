using Lab4;
using Microsoft.VisualBasic;
using System.Numerics;

public sealed class SegmentGeometryService
{
    public PointF? FindIntersection(Segment first, Segment second)
    {
        var a = new Vector2(first.A.X, first.A.Y);
        var b = new Vector2(first.B.X, first.B.Y);

        var c = new Vector2(second.A.X, second.A.Y);
        var d = new Vector2(second.B.X, second.B.Y);

        var dc = d - c; 

        var n = new Vector2(-dc.Y, dc.X); 

        var ab = b - a; 

        var den = Vector2.Dot(n, ab);

        if (MathF.Abs(den) < 1e-6f)
            return null;

        var num = Vector2.Dot(n, a - c);

        var t = -num / den;
        if (t < 0 || t > 1)
            return null;

        var Pt = a + t * ab;

        var cd = d - c;

        var u = Vector2.Dot(Pt - c, cd)
            / Vector2.Dot(cd, cd);
        if (u < 0 || u > 1)
            return null;

        return new PointF(Pt.X, Pt.Y);
    }

    public PointSide ClassifyPoint(Segment edge, PointF point)
    {
        var a = new Vector2(edge.A.X, edge.A.Y);
        var b = new Vector2(edge.B.X, edge.B.Y);
        var p = new Vector2(point.X, point.Y);

        var ab = b - a;
        var ap = p - a;

        var cross = ab.X * ap.Y - ab.Y * ap.X;

        if (MathF.Abs(cross) < 1e-6f)
            return PointSide.OnSegment;

        if (cross > 0)
            return PointSide.Left;

        return PointSide.Right;
    }
}
