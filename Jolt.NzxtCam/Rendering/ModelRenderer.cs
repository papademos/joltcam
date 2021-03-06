using System.Drawing.Drawing2D;
using System.Numerics;
using static Jolt.NzxtCam.MathF;
namespace Jolt.NzxtCam;

static class ModelRenderer
{
    private static float MaxAngle(IReadOnlyList<Vector3> normals) {
        if (normals.Count == 1) {
            return 1;
        }

        var maxAngle = 0f;
        for (int i = 0; i < normals.Count; i++) {
            for (int j = i + 1; j < normals.Count; j++) {
                maxAngle = Max(maxAngle, Abs(Angle(normals[i], normals[j])));
            }
        }
        return maxAngle;
    }

    public static void RenderFaceLines(RenderContext context, LineManager lineManager, Model model) {
        //
        var g = context.Graphics;
        lineManager.Add(model);
        //return;

        // Calculate visibility based on face culling.
        //var set = new LineSet();
        //foreach (var face in model.Faces) {
        //    var (i0, i1, i2) = (face[0], face[1], face[^1]);
        //    var (p0, p1, p2) = (model.Positions[i0], model.Positions[i1], model.Positions[i2]);
        //    var u = Vector3.Normalize(p1 - p0);
        //    var v = Vector3.Normalize(p2 - p0);
        //    var w = Vector3.Cross(u, v);
        //    var isVisible = w.Z >= -0.01f;
        //    if (isVisible) {
        //        set.AddLines(face);
        //    }
        //}

        // Render
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = SmoothingMode.HighQuality;
        //using var pen = new Pen(Color.White, 2);

        //foreach (var line in model.Faces.SelectMany(face => face.AsLines())) {
        //    var (i0, i1) = (line[0], line[1]);
        //    var (p0, p1) = (model.Positions[i0], model.Positions[i1]);
        //    g.RenderLine(pen, p0, p1);
        //}

        var p = lineManager.Positions;
        foreach (var (line, normals) in lineManager.Normals) {
            //if (MaxAngle(normals) > 0.2f) {
                var (i0, i1) = (line[0], line[1]);
                var (p0, p1) = (p[i0], p[i1]);
                using var pen = new Pen(lineManager.Colors[(i0, i1)][0], 3);
                g.RenderLine(pen, p0, p1);
            //}
        }

        //foreach (var line in model.Faces.SelectMany(face => face.AsLines())) {
        //    var (i0, i1) = (line[0], line[1]);
        //    var (p0, p1) = (model.Positions[i0], model.Positions[i1]);
        //    if (set.Contains(i0, i1)) {
        //        set.Remove(i0, i1);
        //        set.Remove(i1, i0);
        //        g.RenderLine(pen, p0, p1);
        //    }
        //}
    }
}
