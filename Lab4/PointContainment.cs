namespace Lab4;

public interface IPolygonContainmentService
{
    bool ContainsPoint(PolygonShape polygon, PointF point);
}

public sealed class PolygonContainmentService : IPolygonContainmentService
{
    // TODO 2: implement point-in-polygon classification, including boundary and degenerate polygons.
    // The UI calls this repeatedly while retaining the scene and the active query mode.
    public bool ContainsPoint(PolygonShape polygon, PointF point)
    {
        throw new NotImplementedException("TODO 2: point-in-polygon test");
    }
}
