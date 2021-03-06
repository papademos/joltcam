using System.Drawing.Drawing2D;
using System.Numerics;
using static Jolt.NzxtCam.MathF;
using static System.Numerics.Matrix4x4;
namespace Jolt.NzxtCam;

class CubePolygonEffect : EffectBase
{
    private readonly Model4 model = ModelFactory.CreateCuboid4(100, 100, 100, Color.RebeccaPurple);

    public override void Render(RenderContext context) {
        //
        var cbuffer = context.Cbuffer;
        var zbuffer = context.Zbuffer;
        cbuffer?.Clear();
        zbuffer?.Fill(float.MaxValue);

        // Update
        var (graphics, size, t) = context;
        var objectMatrices = new[] {
            RotZ(0.02f * t) *
            RotY(0.01f * t) *
            RotX(-0.3f * t),
            RotZ(-0.013f * t) *
            RotY(0.027f * t) *
            RotX(0.09f * t)
        };
        var viewMatrix = CreateLookAt(
            cameraPosition: V(0, 0, -150),
            cameraTarget: V(0, 0, 0),
            cameraUpVector: V(0, -1, 0));
        var znear = 10f;
        var zfar = 1001f;
        var projectionMatrix = CreatePerspective(250, 250, znear, zfar);

        //
        var models = new[] {
            model with { Positions = new(model.Positions) },
            model with { Positions = new(model.Positions) }
        };

        // Transformations.
        for (int i = 0; i < models.Length; i++) {
            var p = models[i].Positions;
            Transform(p, objectMatrices[i]);
            Transform(p, viewMatrix);
            Transform(p, projectionMatrix);
            TransformToScreen(p, znear, zfar, context.Size.Width, context.Size.Height);
        }

        //
        var colors = new[] {
            Color.Orange,
            Color.SlateBlue
        };
        for (int i = 0; i < models.Length; i++) {
            var model = models[i];
            var faces = model.Faces.Select(face => face.Select(index => model.Positions[index]).ToArray()).ToArray();
            foreach (var f in faces) {
                var w = Cross(Vector2.Normalize(V2(f[1] - f[0])), Vector2.Normalize(V2(f[2] - f[1])));
                if (w < 0) {
                    continue;
                }
                var a = 255;
                var r = (int)Abs(w * colors[i].R);
                var g = (int)Abs(w * colors[i].G);
                var b = (int)Abs(w * colors[i].B);
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
    }
}
