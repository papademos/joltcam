namespace Jolt.NzxtCam;

static class GdiRenderer
{
    public static void RenderLine(this Graphics graphics, Pen pen, VectorArg from, VectorArg to)
        => graphics.DrawLine(pen, from, to);

    public static void RenderLine(this Graphics graphics, Pen pen, Line2 line)
        => graphics.RenderLine(pen, line.From, line.To);

    public static void RenderPolyline(this Graphics graphics, Pen pen, Polygon2 polygon)
        => graphics.DrawPolygon(pen, polygon.Select(v => new PointF(v.X, v.Y)).ToArray());
}
