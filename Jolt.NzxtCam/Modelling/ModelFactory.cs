using System.Numerics;
using static Jolt.NzxtCam.MathF;

namespace Jolt.NzxtCam;

internal static class ModelFactory {
    public static Model4 CreateCuboid4(Color color)
        => CreateCuboid4(1, color);

    public static Model4 CreateCuboid4(float scale, Color color)
        => CreateCuboid4(2 * scale, 2 * scale, 2 * scale, color);

    public static Model4 CreateCuboid4(float w, float h, float d, Color color)
        => CreateCuboid4(-w / 2, w / 2, -h / 2, h / 2, -d / 2, d / 2, color);

    public static Model4 CreateCuboid4(float x0, float x1, float y0, float y1, float z0, float z1, Color color) {
        var positions = new List<Vector4>() {
            new(x0, y0, z1, 1),
            new(x1, y0, z1, 1),
            new(x1, y1, z1, 1),
            new(x0, y1, z1, 1),
            new(x0, y0, z0, 1),
            new(x1, y0, z0, 1),
            new(x1, y1, z0, 1),
            new(x0, y1, z0, 1),
        };
        var faces = new List<Face> {
            new Face(0, 1, 2, 3) with { Color = color, Normal = new( 0, 0,-1) }, // front
            new Face(1, 5, 6, 2) with { Color = color, Normal = new( 1, 0, 0) }, // right
            new Face(5, 4, 7, 6) with { Color = color, Normal = new( 0, 0, 1) }, // back
            new Face(4, 0, 3, 7) with { Color = color, Normal = new(-1, 0, 0) }, // left
            new Face(4, 5, 1, 0) with { Color = color, Normal = new( 0,-1, 0) }, // top
            new Face(3, 2, 6, 7) with { Color = color, Normal = new( 0, 1, 0) }, // bottom
        };
        return new(positions, faces);
    }
}