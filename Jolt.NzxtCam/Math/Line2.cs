using System.Numerics;
using static Jolt.NzxtCam.MathF;
namespace Jolt.NzxtCam;

public record struct Line2(Vector2 From, Vector2 To)
{
    public bool IsEmpty => IsEqual(From, To);
    public Vector2 Direction => Vector2.Normalize(To - From);
    public float Length => (To - From).Length();
    public Line2 Reversed => new(To, From);
    public Ray2 ToRay() => new(From, Direction);
    public static implicit operator Ray2(Line2 line) => line.ToRay();
    public static Line2 operator +(Line2 line, Vector2 p) => new(line.From + p, line.To + p);
    public override string ToString() => "{}";
}
