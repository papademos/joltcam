using System.Numerics;
namespace Jolt.NzxtCam;

public static class PolygonRenderer
{
    public static void Render(CBuffer cbuffer, ZBuffer? zbuffer, Polygon2 polygon, int color) {
        //
        var f = polygon;
        var c = f.Count;
        if (c == 3) {
            TriangleRenderer.Render(cbuffer, null, f[0], f[1], f[2], color);
            return;
        }

        //
        for (int i = 1; i < c - 1; i++) {
            TriangleRenderer.Render(cbuffer, null, f[0], f[i], f[i + 1], color);
        }
    }
}
