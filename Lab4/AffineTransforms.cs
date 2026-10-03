using System.Drawing.Drawing2D;

namespace Lab4;

public interface IAffineTransformService
{
    Matrix CreateTranslation(float dx, float dy);
    Matrix CreateRotation(float degrees, PointF pivot);
    Matrix CreateScaling(float sx, float sy, PointF pivot);
    PointF GetCenter(PolygonShape polygon);
    void Apply(PolygonShape polygon, Matrix transform);
}

public sealed class AffineTransformService : IAffineTransformService
{
    // TODO 1: implement translation, rotation about a pivot, and scaling about a pivot as matrices.
    public Matrix CreateTranslation(float dx, float dy)
    {
        throw new NotImplementedException("TODO 1: translation matrix");
    }

    public Matrix CreateRotation(float degrees, PointF pivot)
    {
        throw new NotImplementedException("TODO 1: pivot rotation matrix");
    }

    public Matrix CreateScaling(float sx, float sy, PointF pivot)
    {
        throw new NotImplementedException("TODO 1: pivot scaling matrix");
    }

    // TODO 1: define and document the polygon-center convention, including one- and two-vertex polygons.
    public PointF GetCenter(PolygonShape polygon)
    {
        throw new NotImplementedException("TODO 1: polygon center");
    }

    // TODO 1: multiply every vertex by the supplied matrix; do not transform only the raster drawing.
    public void Apply(PolygonShape polygon, Matrix transform)
    {
        throw new NotImplementedException("TODO 1: apply matrix to vertices");
    }
}
