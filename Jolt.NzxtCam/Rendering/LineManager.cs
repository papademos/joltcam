using System.Numerics;
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
