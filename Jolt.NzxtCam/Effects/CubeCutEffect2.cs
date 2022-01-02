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
        // Update
        var (g, _, t) = context;
        t *= 0.1f;
        var mObj = new[] {
            RotZ(0.02f * t) *
            RotY(0.01f * t) *
            RotX(-0.3f * t),
            //Matrix4x4.Identity,
            RotZ(0.005f * t) *
            RotY(0.25f * t) *
            RotX(0.015f * t),
            //Matrix4x4.Identity,
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
            Transform(model.Positions, viewProjectionMatrix);
            TransformToScreen(model.Positions, znear, zfar, context.Size.Width, context.Size.Height);
        }

        //
        foreach (var model in models) {
            model.Cull(Matrix4x4.Identity);
        }

        // Render
        using var pen = new Pen(Color.White, 2);
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = SmoothingMode.HighQuality;
        foreach (var model in models) {
            foreach (var face in model.Faces) {
                var positions = face
                    .Select(index => model.Positions[index])
                    .Select(V2)
                    .ToList();
                for (int i = 0; i < positions.Count; i++) {
                    g.RenderLine(pen, positions[i], positions[(i + 1) % positions.Count]);
                }
            }
        }
    }
}

record FaceLine(Vector3 From, Vector3 To, int ObjectId, Vector3 ViewNormal, Vector3 ProjNormal);
