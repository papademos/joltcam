using System.Drawing.Drawing2D;
using System.Numerics;
using static Jolt.NzxtCam.MathF;
using static System.Numerics.Matrix4x4;
namespace Jolt.NzxtCam;

class CubePolygonEffect : EffectBase
{
    private readonly Model4 model = default(Model4) with {
        Positions = new [] {
            V(-50, -50, - -50, 1),
            V( 50, -50, - -50, 1),
            V( 50,  50, - -50, 1),
            V(-50,  50, - -50, 1),
            V(-50, -50, -  50, 1),
            V( 50, -50, -  50, 1),
            V( 50,  50, -  50, 1),
            V(-50,  50, -  50, 1),
        },
        Faces = new Face[] {
            new(0, 1, 2, 3), // front
            new(1, 5, 6, 2), // right
            new(5, 4, 7, 6), // back
            new(4, 0, 3, 7), // left
            new(4, 5, 1, 0), // top
            new(3, 2, 6, 7), // bottom
        },
        Color = Color.RebeccaPurple,
    };

    private readonly CBuffer cbuffer = new(new(250, 250));
    private readonly ZBuffer zbuffer = new(new(250, 250));

    public override void Render(RenderContext context) {
        //
        cbuffer.Clear();
        zbuffer.Fill(float.MaxValue);

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
            model with {
                Positions = model.Positions.ToArray()
            },
            model with {
                Positions = model.Positions.ToArray()
            }
        };

        // Transformations.
        for (int i = 0; i < models.Length; i++) {
            var m = objectMatrices[i] * viewMatrix * projectionMatrix;
            var p = models[i].Positions;
            Transform(p, m);
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
                var z = Vector3.Normalize(Vector3.Cross(V3(f[1] - f[0]), V3(f[2] - f[1]))).Z;
                if (z < 0) {
                    continue;
                }
                var a = 255;
                var r = (int)Abs(z * colors[i].R);
                var g = (int)Abs(z * colors[i].G);
                var b = (int)Abs(z * colors[i].B);
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
