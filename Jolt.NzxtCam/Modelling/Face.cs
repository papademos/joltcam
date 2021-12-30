using System.Collections;
namespace Jolt.NzxtCam;

record struct Face : IReadOnlyList<int>
{
    private readonly IReadOnlyList<int> indices;
    public Face(IReadOnlyList<int> indices) {
        this.indices = (indices?.Count ?? 0) >= 3 
            ? indices!
            : throw new ArgumentException(nameof(indices));
    }
    public Face(params int[] indices) : this((IReadOnlyList<int>)indices) {
    }
    public int this[int i] => indices[i % indices.Count];
    public int Count => indices.Count;
    public int2[] AsLines(bool closed = true) {
        var lineCount = closed
            ? indices.Count
            : indices.Count - 1;
        var lines = new int2[lineCount];
        for (int i = 0; i < indices.Count - 1; i++) {
            lines[i] = (indices[i], indices[i + 1]);
        }
        if (closed) {
            lines[^1] = (indices[^1], indices[0]);
        }
        return lines;
    }
    public IEnumerator<int> GetEnumerator() => ((IEnumerable<int>)indices).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => indices.GetEnumerator();
}
