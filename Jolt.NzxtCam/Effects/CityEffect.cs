using System.Numerics;
using System.Linq;
using static Jolt.NzxtCam.MathF;
using static System.Numerics.Matrix4x4;
using System.Drawing.Drawing2D;
using System.Diagnostics;

namespace Jolt.NzxtCam;

//class City
//{
//    public List<Block> Blocks { get; } = new();
//}

//record class Junction(Vector2 Position);

//record class Road(Junction J0, Junction J1);

record class Building(Vector2 Position, Vector3 Size, float Rotation) {
}

class Block
{
    public Polygon2 Bounds { get; } = new();
    public List<Building> Buildings { get; } = new();
    public Block(Polygon2 bounds) {
        Bounds = bounds;
    }
}

//class Building
//{
//}

class RadialCity
{
    //public List<Line2> Lines { get; } = new();
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
        var g = context.Graphics;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = SmoothingMode.HighQuality;
        using var pen = new Pen(Color.Orange, 2);
        using var pen1 = new Pen(Color.BlueViolet, 1.5f);
        var w = 250;
        var h = 250;
        var o = new Vector2(w / 2, h / 2);
        var t = context.ElapsedSeconds;
        var m =
            Matrix3x2.CreateRotation(0.3f * t) *
            Matrix3x2.CreateScale(2 - Cos(0.2f*t)) *
            Matrix3x2.CreateTranslation(o);

        foreach (var block in city.Blocks) {
            g.RenderPolyline(pen, block.Bounds.Transform(m));
            foreach (var building in block.Buildings) {
                var sx = building.Size.X / 2;
                var sy = building.Size.Y / 2;
                var p = new Polygon2(new Vector2[] { new(-sx, -sy), new(sx, -sy), new(sx, sy), new(-sx, sy) });
                var m2 =
                    Matrix3x2.CreateRotation(RevToRad(building.Rotation)) *
                    Matrix3x2.CreateTranslation(building.Position) *
                    m;
                g.RenderPolyline(pen1, p.Transform(m2));
            }
        }
    }
}
