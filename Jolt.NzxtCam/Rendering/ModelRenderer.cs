using System.Drawing.Drawing2D;
using System.Numerics;
using static Jolt.NzxtCam.MathF;
namespace Jolt.NzxtCam;

class LineManager
{
    private Dictionary<int, HashSet<int>> xIndex = new();
    private Dictionary<int, HashSet<int>> yIndex = new();
    private Dictionary<int, HashSet<int>> zIndex = new();
    private const float precision = 10f;

    public List<Vector3> Positions = new();
    public Dictionary<int2, List<Vector3>> Normals { get; } = new();
    public Dictionary<int2, List<Color>> Colors { get; } = new();

    private int Add(Vector3 p) {
        var xKey = (int)Math.Round(precision * p.X, MidpointRounding.AwayFromZero);
        var yKey = (int)Math.Round(precision * p.Y, MidpointRounding.AwayFromZero);
        var zKey = (int)Math.Round(precision * p.Z, MidpointRounding.AwayFromZero);
        xIndex.TryGetValue(xKey, out var xValues);
        yIndex.TryGetValue(yKey, out var yValues);
        zIndex.TryGetValue(zKey, out var zValues);
        if (xValues != null && yValues != null && zValues != null) {
            var set = new HashSet<int>();
            set.UnionWith(xValues);
            set.IntersectWith(yValues);
            set.IntersectWith(zValues);
            if (set.Count > 1) {
                throw new InvalidOperationException();
            }
            if (set.Count == 1) {
                return set.First();
            }
        }
        xValues ??= xIndex[xKey] = new();
        yValues ??= yIndex[yKey] = new();
        zValues ??= zIndex[zKey] = new();
        xValues.Add(Positions.Count);
        yValues.Add(Positions.Count);
        zValues.Add(Positions.Count);
        Positions.Add(p);
        return Positions.Count - 1;
    }

    public void Add(Line3 line, Vector3 normal, Color color) {
        var i0 = Add(line.From);
        var i1 = Add(line.To);
        if (i0 > i1) {
            (i0, i1) = (i1, i0);
        }
        
        //
        if (!Normals.TryGetValue((i0, i1), out var normals)) {
            Normals[(i0, i1)] = normals = new();
        }
        normals.Add(normal);

        //
        if (!Colors.TryGetValue((i0, i1), out var colors)) {
            Colors[(i0, i1)] = colors = new();
        }
        colors.Add(color);
    }

    public void Add(Model model) {
        var p = model.Positions;
        foreach (var face in model.Faces) {
            // TODO: We actually need 2 different normals;
            //       one post-projection for culling, and
            //       one pre-projection for correct angle calculations.
            var normal = model.CalculateNormal(face);

            // Culling
            //if (normal.Z < -Epsilon) {
            //if (normal.Z < -0.015f) {
            if (normal.Z > 0) {
                continue;
            }

            //
            for (int i = 0; i < face.Count; i++) {
                var p0 = p[face[i]];
                var p1 = p[face[i + 1]];
                var line = new Line3(p[face[i]], p[face[i + 1]]);
                Add(line, normal, model.Color);
            }
        }
    }
}

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
