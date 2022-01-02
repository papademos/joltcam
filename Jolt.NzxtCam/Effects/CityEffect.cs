using System.Numerics;
using System.Linq;
using static Jolt.NzxtCam.MathF;
using static System.Numerics.Matrix4x4;
using System.Drawing.Drawing2D;
using System.Diagnostics;

namespace Jolt.NzxtCam;

record class Building(Vector2 Position, Vector3 Size, float Rotation);

class Block
{
    public Polygon2 Bounds { get; } = new();
    public List<Building> Buildings { get; } = new();
    public Block(Polygon2 bounds) {
        Bounds = bounds;
    }
}

class RadialCity
{
    public List<Block> Blocks { get; } = new();
    public RadialCity() {
        var positions = Enumerable
            .Range(0, 6)
            .Select(i => Transform(100 * Vector2.UnitX, RotZ(i / 6f)))
            .Append(Vector2.Zero)
            .ToArray();
        var blocksPositions = Enumerable
            .Range(0, 6)
            .Select(i => new[] { 
                positions[6], 
                positions[i], 
                positions[(i + 1) % 6] })
            .ToArray();
        var blocks = blocksPositions
            .Select(p => new Polygon2(p))
            .Select(p => p.Inflate(-5))
            .Select(p => new Block(p))
            .ToArray();
        foreach (var block in blocks) {
            var s = 10f;
            var lines = block.Bounds.Inflate(-s).AsLines();
            foreach (var line in lines) {
                var count = (int)Math.Floor(line.Length / s);
                var rotation = Atan(line.Direction.Y, line.Direction.X);
                var offset = 0.5f * s * new Vector2(line.Direction.Y, -line.Direction.X);
                var size = new Vector3(s - 2f, s - 2f, 10);
                for (int i = 0; i < count; i++) {
                    var t = (i + 0.5f) / count;
                    var p = Vector2.Lerp(line.From, line.To, t) + offset;
                    block.Buildings.Add(new(p, size, rotation));
                }
            }
        }
        Blocks.AddRange(blocks);
    }
}

class CityEffect : EffectBase
{
    private RadialCity city = new();

    public unsafe CityEffect() {
    }

    public override void Render(RenderContext context) {
        //
        var (graphics, size, t) = context;
        var (w, h) = (size.Width, size.Height);

        //
        var viewMatrix = CreateLookAt(
            cameraPosition: V(0, -150, 0),
            cameraTarget: V(0, 0, 0),
            cameraUpVector: V(0, 0, -1));
        var znear = 10f;
        var zfar = 1001f;
        var projectionMatrix = CreatePerspective(w, h, znear, zfar);

        //
        var models = new List<Model4>();
        foreach (var block in city.Blocks) {
            foreach (var building in block.Buildings) {
                var sx = building.Size.X / 2;
                var sy = building.Size.Y / 2;
                var model = ModelFactory.CreateCuboid4(-sx, sx, -sy, sy, 0, 50, Color.Gray);
                var m4 =
                    CreateRotationZ(RevToRad(building.Rotation)) *
                    CreateTranslation(V3(building.Position)) *
                    viewMatrix *
                    projectionMatrix;
                Transform(model.Positions, m4);
                TransformToScreen(model.Positions, znear, zfar, w, h);
                models.Add(model);
            }
        }

        //
        var cbuffer = context.Cbuffer;
        var zbuffer = context.Zbuffer;
        cbuffer?.Clear();
        zbuffer?.Fill(float.MaxValue);

        //
        for (int i = 0; i < models.Count; i++) {
            var model = models[i];
            var faces = model.Faces.Select(face => face.Select(index => model.Positions[index]).ToArray()).ToArray();
            foreach (var f in faces) {
                var q = Cross(Vector2.Normalize(V2(f[1] - f[0])), Vector2.Normalize(V2(f[2] - f[1])));
                if (q < 0) {
                    continue;
                }
                var color = model.Color;
                var a = 255;
                var r = (int)Abs(q * color.R);
                var g = (int)Abs(q * color.G);
                var b = (int)Abs(q * color.B);
                var c = Color.FromArgb(a, r, g, b).ToArgb();
                TriangleRenderer.Render(cbuffer, zbuffer, f[0], f[1], f[2], c);
                TriangleRenderer.Render(cbuffer, zbuffer, f[2], f[3], f[0], c);
            }
        }

        //
        var bitmap = cbuffer.UpdateBitmap();
        var state = graphics.Save();
        graphics.PixelOffsetMode = PixelOffsetMode.None;
        graphics.SmoothingMode = SmoothingMode.None;
        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        graphics.DrawImageUnscaled(bitmap, 0, 0);
        graphics.Restore(state);

        //
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        using var pen = new Pen(Color.Orange, 2);
        using var pen1 = new Pen(Color.BlueViolet, 1.5f);
        var o = new Vector2(w / 2, h / 2);
        var m =
            Matrix3x2.CreateRotation(0.3f * t) *
            Matrix3x2.CreateScale(2 - Cos(0.2f*t)) *
            Matrix3x2.CreateTranslation(o);

        //
        foreach (var block in city.Blocks) {
            graphics.RenderPolyline(pen, block.Bounds.Transform(m));
            foreach (var building in block.Buildings) {
                var sx = building.Size.X / 2;
                var sy = building.Size.Y / 2;
                var p = new Polygon2(new Vector2[] { new(-sx, -sy), new(sx, -sy), new(sx, sy), new(-sx, sy) });
                var m2 =
                    Matrix3x2.CreateRotation(RevToRad(building.Rotation)) *
                    Matrix3x2.CreateTranslation(building.Position) *
                    m;
                graphics.RenderPolyline(pen1, p.Transform(m2));
            }
        }    
    }
}

internal static class ModelFactory {
    public static Model4 CreateCuboid4(float w, float h, float d, Color color)
        => CreateCuboid4(-w / 2, w / 2, -h / 2, h / 2, -d / 2, d / 2, color);

    public static Model4 CreateCuboid4(float x0, float x1, float y0, float y1, float z0, float z1, Color color)
        => new(new() {
            V(x0, y0, z1, 1),
            V(x1, y0, z1, 1),
            V(x1, y1, z1, 1),
            V(x0, y1, z1, 1),
            V(x0, y0, z0, 1),
            V(x1, y0, z0, 1),
            V(x1, y1, z0, 1),
            V(x0, y1, z0, 1),
        },
        new() {
            new(0, 1, 2, 3), // front
            new(1, 5, 6, 2), // right
            new(5, 4, 7, 6), // back
            new(4, 0, 3, 7), // left
            new(4, 5, 1, 0), // top
            new(3, 2, 6, 7), // bottom
        },
        color);
}