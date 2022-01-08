using System.Numerics;
using static Jolt.NzxtCam.MathF;
namespace Jolt.NzxtCam;

public static class TriangleRenderer
{
    public static void Render(CBuffer cbuffer, ZBuffer? zbuffer, Vector2 p0, Vector2 p1, Vector2 p2, int color) {
        if (zbuffer != null) {
            throw new ArgumentException(nameof(zbuffer));
        }
        Render(cbuffer, zbuffer, V4(p0), V4(p1), V4(p2), color);
    }

    public static void Render(CBuffer cbuffer, ZBuffer? zbuffer, Vector4 p0, Vector4 p1, Vector4 p2, int color) {
        //
        int I(float f) => (int)(f + 0.5f);
        float F(int i) => (float)i;

        // Sort
        var i0 = p1.Y < p0.Y
            ? p2.Y < p1.Y ? 2 : 1
            : p2.Y < p0.Y ? 2 : 0;
        if (i0 == 1) (p0, p1, p2) = (p1, p2, p0);
        else if (i0 == 2) (p0, p1, p2) = (p2, p0, p1);

        // TODO: clip?

        // Guard clauses.
        var (w, h) = (cbuffer.Size.Width, cbuffer.Size.Height);
        var (y0, y1, y2) = (I(p0.Y), I(p1.Y), I(p2.Y));
        // Top.
        if (y0 < 0) {
            // Fail silent for now
            return;
        }
        // Bottom.
        if (y1 > h || y2 > h) {
            // Fail silent for now
            return;
        }
        // Left.
        if (I(p0.X) < 0 | I(p1.X) < 0 || I(p2.X) < 0) {
            // Fail silent for now
            return;
        }
        // Right.
        if (I(p0.X) > w | I(p1.X) > w || I(p2.X) > w) {
            // Fail silent for now
            return;
        }

        // "split"
        Vector4 p3;
        int y3;
        if (y1 < y2) {
            var t = F(y1 - y0) / (y2 - y0);
            p3 = Vector4.Lerp(p0, p2, t);
            y3 = I(p3.Y);
        } else {
            p3 = p2; y3 = y2;
            p2 = p1; y2 = y1;
            var t = F(y3 - y0) / (y2 - y0);
            p1 = Vector4.Lerp(p0, p2, t);
            y1 = I(p1.Y);
        }

        //
        Render(cbuffer, zbuffer, p0, p0, p3, p1, color);
        Render(cbuffer, zbuffer, p3, p1, p2, p2, color);
    }

    public static void Render(CBuffer cbuffer, ZBuffer? zbuffer, Vector4 upperLeft, Vector4 upperRight, Vector4 lowerLeft, Vector4 lowerRight, int color) {
        if (upperRight.X < upperLeft.X || lowerRight.X < lowerLeft.X) {
            return;
            //// Only needed if we want to support both clockwise and anti-clockwise.
            //(upperLeft, upperRight) = (upperRight, upperLeft);
            //(lowerLeft, lowerRight) = (lowerRight, lowerLeft);
        }

        //
        int I(float f) => (int)(f + 0.5f);
        float F(int i) => (float)i;
        var y0 = I(upperLeft.Y);
        var yn = I(lowerLeft.Y);
        var dy = yn - y0;
        for (int y = y0; y < yn; y++) {
            //
            var t = F(y - y0) / dy;
            var left = Vector4.Lerp(upperLeft, lowerLeft, t);
            var right = Vector4.Lerp(upperRight, lowerRight, t);
            
            //
            var x0 = I(left.X);
            var xn = I(right.X);
            var dx = xn - x0;

            // 
            if (zbuffer == null) {
                for (var x = x0; x < xn; x++) {
                    cbuffer[x, y] = color;
                }
                continue;
            }

            //
            for (var x = x0; x < xn; x++) {
                t = F(x - x0) / dx;
                var z = Lerp(left.Z, right.Z, t);
                if (z < zbuffer[x, y]) {
                    zbuffer[x, y] = z;
                    cbuffer[x, y] = color;
                }
            }
        }
    }
}
