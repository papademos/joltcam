using System.Collections;
using System.Numerics;
using static Jolt.NzxtCam.MathF;
namespace Jolt.NzxtCam;

record class Face : IReadOnlyList<int>
{
    public IReadOnlyList<int> Indices { get; init; }
    public Color Color { get; set; } = default;
    public Vector3 Normal { get; set; } = default;
    public Face(IReadOnlyList<int> indices) {
        this.Indices = (indices?.Count ?? 0) >= 3 
            ? indices!
            : throw new ArgumentException(nameof(indices));
    }
    public Face(params int[] indices) : this((IReadOnlyList<int>)indices) {
    }
    public Face(Color color, params int[] indices) : this((IReadOnlyList<int>)indices) {
        Color = color;
    }
    public int this[int i] => Indices[Mod(i, Indices.Count)];
    public int Count => Indices.Count;
    public int2[] AsLines(bool closed = true) {
        var lineCount = closed
            ? Indices.Count
            : Indices.Count - 1;
        var lines = new int2[lineCount];
        for (int i = 0; i < Indices.Count - 1; i++) {
            lines[i] = (Indices[i], Indices[i + 1]);
        }
        if (closed) {
            lines[^1] = (Indices[^1], Indices[0]);
        }
        return lines;
    }
    public IEnumerator<int> GetEnumerator() => ((IEnumerable<int>)Indices).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Indices.GetEnumerator();
    public override string ToString() => "{}";
}
