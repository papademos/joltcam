using System.Numerics;
using static Jolt.NzxtCam.MathF;
using static System.Numerics.Matrix4x4;
namespace Jolt.NzxtCam;

class TwisterEffect : EffectBase
{
    private const int segmentCount = 30;
    private readonly Model model;
    public TwisterEffect() {

        // Positions.
        var height = 10;
        var y0 = -height * (segmentCount - 1) / 2f;
        var positions = new Vector3[4 * segmentCount];
        for (var i = 0; i < segmentCount; i++) {
            var y = y0 + i * height;
            positions[4 * i + 0] = new (-50, y, -50);
            positions[4 * i + 1] = new ( 50, y, -50);
            positions[4 * i + 2] = new ( 50, y,  50);
            positions[4 * i + 3] = new (-50, y,  50);
        }

        //// Lines.
        //var lines = new List<int2>();
        //// XZ-lines.
        //for (var i = 0; i < segmentCount; i++) {
        //    lines.Add((4 * i + 0, 4 * i + 1));
        //    lines.Add((4 * i + 1, 4 * i + 2));
        //    lines.Add((4 * i + 2, 4 * i + 3));
        //    lines.Add((4 * i + 3, 4 * i + 0));
            
        //}
        //// Y-lines.
        //for (var i = 0; i < segmentCount - 1; i++) {
        //    lines.Add((4 * i + 0, 4 * i + 4 + 0));
        //    lines.Add((4 * i + 1, 4 * i + 4 + 1));
        //    lines.Add((4 * i + 2, 4 * i + 4 + 2));
        //    lines.Add((4 * i + 3, 4 * i + 4 + 3));
        //}

        // Faces.
        var faces = new List<Face>();
        for (var i = 0; i < segmentCount - 1; i++) {
            faces.Add(new(4 * i + 0, 4 * i + 1, 4 * i + 5, 4 * i + 4));
            faces.Add(new(4 * i + 1, 4 * i + 2, 4 * i + 6, 4 * i + 5));
            faces.Add(new(4 * i + 2, 4 * i + 3, 4 * i + 7, 4 * i + 6));
            faces.Add(new(4 * i + 3, 4 * i + 0, 4 * i + 4, 4 * i + 7));
        }

        // Model.
        this.model = new(positions, faces.ToArray(), Color.White);
    }
    private static float Spin(int seed, float t, float dy) {
        var random = new Random(seed);
        var t0 = t + (float)random.NextDouble();
        var t1 = t + (float)random.NextDouble();
        return
            0.07f * t0 + 0.1f * dy * (1 + Sin(0.1f * t)) +
            0.17f * t1 + 0.2f * dy * (1 + Sin(0.08f * t)) +
            0;
    }
    public override void Render(RenderContext context) {
        // Update
        var (g, size, t) = context;
        //var positions = model.Positions.ToArray();
        //var dy = (float)(i / 4) / (segmentCount - 1);
        //var a = Spin(0, t, dy);
        //var m = 
        //    RotY(a) *
        //    viewMatrix *
        //    projectionMatrix;
        //Transform(positions, m);
        var positions = new Vector3[model.Positions.Length];
        for (int i = 0; i < model.Positions.Length; i++) {
            var p = model.Positions[i];
            var dy = (float)(i / 4) / (segmentCount - 1);
            var a = Spin(0, t, dy);
            positions[i] = Vector3.Transform(p, RotY(a));
        }

        //
        //var viewMatrix = CreateLookAt(
        //    cameraPosition: V(0, 25, -75),
        //    cameraTarget: V(0, 0, 0),
        //    cameraUpVector: V(0, -1, 0));
        //var znear = 100;
        //var zfar = 300;

        var viewMatrix = CreateLookAt(
            cameraPosition: V(0, 0, -150),
            cameraTarget: V(0, 0, 0),
            cameraUpVector: V(0, -1, 0));
        var znear = 10f;
        var zfar = 1001f;

        var w = 250;
        var h = 250;
        var o = new Vector3(w / 2, h / 2, 0);
        var projectionMatrix = CreatePerspective(w, h, znear, zfar);
        var m =
            viewMatrix *
            projectionMatrix;

        Transform(positions, m);
        TransformToScreen(positions, znear, zfar, w, h);

        // Render
        var lineManager = new LineManager();
        ModelRenderer.RenderFaceLines(context, lineManager, model with { Positions = positions });
    }
}
