using System.Drawing.Drawing2D;
using System.Numerics;
using static Jolt.NzxtCam.MathF;
using static System.Numerics.Matrix4x4;
namespace Jolt.NzxtCam;

class CubeCutEffect2 : EffectBase {
    private readonly Model model = default(Model) with {
        Positions = new() {
            V(-50, -50, - -50),
            V( 50, -50, - -50),
            V( 50,  50, - -50),
            V(-50,  50, - -50),
            V(-50, -50, -  50),
            V( 50, -50, -  50),
            V( 50,  50, -  50),
            V(-50,  50, -  50),
        },
        Faces = new() {
            new(0, 1, 2, 3), // front
            new(1, 5, 6, 2), // right
            new(5, 4, 7, 6), // back
            new(4, 0, 3, 7), // left
            new(4, 5, 1, 0), // top
            new(3, 2, 6, 7), // bottom
        },
    };

    public override void Render(RenderContext context) {
        //
        var cbuffer = context.Cbuffer;
        cbuffer?.Clear();

        // Update
        var (graphics, _, t) = context;
        t *= 0.4f;
        var mObj = new[] {
            RotZ(0.02f * t) *
            RotY(0.01f * t) *
            RotX(-0.3f * t),
            RotZ(0.005f * t) *
            RotY(0.25f * t) *
            RotX(0.015f * t),
        };
        var models = new[] {
            model with { Positions = new(model.Positions), Faces = new(model.Faces) },
            model with { Positions = new(model.Positions), Faces = new(model.Faces) },
        };

        // Split the faces of the first cube.
        var s = 50;
        {
            var (j, i) = (0, 1);
            var (mThis, mThat) = (mObj[i], mObj[j]);
            var (positions, faces, color) = models[i];
            Transform(positions, mThis);
            (faces, var front) = FaceHelper.Split(positions, faces, Plane.Transform(new Plane(0, 0, 1, s), mThat));
            (faces, var back) = FaceHelper.Split(positions, faces, Plane.Transform(new Plane(0, 0, -1, s), mThat));
            (faces, var left) = FaceHelper.Split(positions, faces, Plane.Transform(new Plane(1, 0, 0, s), mThat));
            (faces, var right) = FaceHelper.Split(positions, faces, Plane.Transform(new Plane(-1, 0, 0, s), mThat));
            (faces, var top) = FaceHelper.Split(positions, faces, Plane.Transform(new Plane(0, -1, 0, s), mThat));
            (faces, var bottom) = FaceHelper.Split(positions, faces, Plane.Transform(new Plane(0, 1, 0, s), mThat));
            faces.Clear();
            faces.AddRange(new[] { front, back, left, right, top, bottom }.SelectMany(_ => _));
            models[i] = models[i] with { Faces = faces };
        }

        // Split the faces of the second cube.
        {
            var (j, i) = (1, 0);
            var (mThis, mThat) = (mObj[i], mObj[j]);
            var (positions, faces, color) = models[i];
            Transform(positions, mThis);
            (faces, var front) = FaceHelper.Split(positions, faces, Plane.Transform(new Plane(0, 0, 1, s), mThat));
            (faces, var back) = FaceHelper.Split(positions, faces, Plane.Transform(new Plane(0, 0, -1, s), mThat));
            (faces, var left) = FaceHelper.Split(positions, faces, Plane.Transform(new Plane(1, 0, 0, s), mThat));
            (faces, var right) = FaceHelper.Split(positions, faces, Plane.Transform(new Plane(-1, 0, 0, s), mThat));
            (faces, var top) = FaceHelper.Split(positions, faces, Plane.Transform(new Plane(0, -1, 0, s), mThat));
            (faces, var bottom) = FaceHelper.Split(positions, faces, Plane.Transform(new Plane(0, 1, 0, s), mThat));
            faces.Clear();
            faces.AddRange(new[] { front, back, left, right, top, bottom }.SelectMany(_ => _));
            models[i] = models[i] with { Faces = faces };
        }

        //
        var viewMatrix = CreateLookAt(
            cameraPosition: V(0, 0, -150),
            cameraTarget: V(0, 0, 0),
            cameraUpVector: V(0, -1, 0));
        var znear = 10f;
        var zfar = 1001f;
        var projectionMatrix = CreatePerspective(250, 250, znear, zfar);
        var viewProjectionMatrix =
            viewMatrix *
            projectionMatrix;
        foreach (var model in models) {
            Transform(model.Positions, viewMatrix);
        }
        foreach (var model in models) {
            Transform(model.Positions, projectionMatrix);
            TransformToScreen(model.Positions, znear, zfar, context.Size.Width, context.Size.Height);
        }

        //
        var baseColors = new[] {
            Color.Orange,
            Color.SlateBlue
        };
        var colors = baseColors.Select(_ => new List<Color>()).ToArray();
        for (int i=0; i < models.Length; i++) {
            var model = models[i];
            var color = baseColors[i];
            for (int j = model.Faces.Count - 1; j >= 0; j--) {
                var normal = model.CalculateNormal(j);
                var z = normal.Z;
                if (normal.Z < 0) {
                    model.Faces.RemoveAt(j);
                    continue;
                }
                // TODO: t = 3.18393779 causes an exception because z is NaN.
                var a = 255;
                var r = (int)Abs(z * color.R);
                var g = (int)Abs(z * color.G);
                var b = (int)Abs(z * color.B);
                var c = Color.FromArgb(a, r, g, b);
                colors[i].Insert(0, c);
            }
        }

        // Render
        using var pen = new Pen(Color.White, 2);
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        var polygons = new SortedDictionary<float, List<ColoredPolygon2>>();
        for (int i=0; i < models.Length; i++) {
            var model = models[i];
            for (int j=0; j < model.Faces.Count; j++) {
                var face = model.Faces[j];
                var color = colors[i][j];
                var z = -face
                    .Average(index => model.Positions[index].Z);
                var positions = face
                    .Select(index => model.Positions[index])
                    .Select(V2)
                    .ToList();
                if (!polygons.TryGetValue(z, out var list)) {
                    polygons[z] = list = new();
                }
                list.Add(new(new(positions), color));
            }
        }
        foreach (var (z, list) in polygons) {
            foreach (var (p, c) in list) {
                for (int i = 0; i < p.Count; i++) {
                    PolygonRenderer.Render(cbuffer, null, p, c.ToArgb());
                }
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

record struct ColoredPolygon2(Polygon2 Polygon, Color color);
