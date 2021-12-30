using static Jolt.NzxtCam.MathF;
namespace Jolt.NzxtCam;

class LineSet
{
    private readonly HashSet<long> set = new HashSet<long>();
    public void Add(int2 line) => Add(line[0], line[1]);
    public void Add(int i0, int i1) {
        set.Add(HiLo(i0, i1));
        set.Add(HiLo(i1, i0));
    }
    public void AddLines(IReadOnlyList<int> indices, bool closed = true) {
        for (int i = 0; i < indices.Count - 1; i++) {
            Add(indices[i], indices[i + 1]);
        }
        if (closed && indices.Count > 2) {
            Add(indices[^1], indices[0]);
        }
    }
    public bool Contains(int i0, int i1)
        => set.Contains(HiLo(i0, i1));
    public bool ContainsAny(int[] indices, bool closed = true) {
        for (int i = 0; i < indices.Length - 1; i++) {
            if (Contains(indices[i], indices[i + 1])) {
                return true;
            }
        }
        if (closed && indices.Length > 2) {
            if (Contains(indices[^1], indices[0])) {
                return true;
            }
        }
        return false;
    }
    public void Remove(int i0, int i1) {
        set.Remove(HiLo(i0, i1));
        set.Remove(HiLo(i1, i0));
    }
    public void RemoveAll(int[] indices, bool closed = true) {
        for (int i = 0; i < indices.Length - 1; i++) {
            Remove(indices[i], indices[i + 1]);
        }
        if (closed && indices.Length > 2) {
            Remove(indices[^1], indices[0]);
        }
    }
}
