using System.Numerics;
using static Jolt.NzxtCam.MathF;
using System.Collections;

namespace Jolt.NzxtCam;

public class Polygon2 : IList<Vector2>
{
    //
    private readonly List<Vector2> positions = new();

    //
    public Polygon2() { }
    public Polygon2(IEnumerable<Vector2> positions) {
        this.positions.AddRange(positions);
    }

    // IList<Vector2> methods
    public Vector2 this[int index] { get => ((IList<Vector2>)positions)[index]; set => ((IList<Vector2>)positions)[index] = value; }
    public int Count => ((ICollection<Vector2>)positions).Count;
    public bool IsReadOnly => ((ICollection<Vector2>)positions).IsReadOnly;
    public void Add(Vector2 item) => ((ICollection<Vector2>)positions).Add(item);
    public void Clear() => ((ICollection<Vector2>)positions).Clear();
    public bool Contains(Vector2 item) => ((ICollection<Vector2>)positions).Contains(item);
    public void CopyTo(Vector2[] array, int arrayIndex) => ((ICollection<Vector2>)positions).CopyTo(array, arrayIndex);
    public IEnumerator<Vector2> GetEnumerator() => ((IEnumerable<Vector2>)positions).GetEnumerator();
    public int IndexOf(Vector2 item) => ((IList<Vector2>)positions).IndexOf(item);
    public void Insert(int index, Vector2 item) => ((IList<Vector2>)positions).Insert(index, item);
    public bool Remove(Vector2 item) => ((ICollection<Vector2>)positions).Remove(item);
    public void RemoveAt(int index) => ((IList<Vector2>)positions).RemoveAt(index);
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)positions).GetEnumerator();

    //
    public List<Line2> AsLines() => positions.Select((p, i) => new Line2(p, positions[(i + 1) % Count])).ToList();

    // Naive implementation, only considers happy path.
    // Assumes clockwise polygon with +X to the right and +Y downwards.
    // TODO: Perhaps implement different version as in https://stackoverflow.com/questions/1109536/an-algorithm-for-inflating-deflating-offsetting-buffering-polygons
    public Polygon2 Inflate(float amount) {
        var result = new Polygon2();
        var count = Count;
        for (int i = 0; i < count; i++) {
            var p0 = this[(i + count - 1) % count];
            var p1 = this[i];
            var p2 = this[(i + 1) % count];
            var u = new Line2(p0, p1).Direction;
            var v = new Line2(p1, p2).Direction;
            var cos = Vector2.Dot(u, v);
            var area = Cross(u, v);
            var scale = Sqrt(area / Sin(Acos(cos)));
            result.Add(p1 + amount * scale * (u - v));
        }
        return result;
    }

    public Polygon2 Transform(Matrix3x2 m) => new(positions.Select(p => Vector2.Transform(p, m)));
}
