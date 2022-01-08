using System.Numerics;
using static Jolt.NzxtCam.MathF;
namespace Jolt.NzxtCam;

public record struct Line3(Vector3 From, Vector3 To)
{
    public bool IsEmpty => IsEqual(From, To);
    public Vector3 Direction => IsEmpty ? Vector3.Zero : Vector3.Normalize(To - From);
    public Line3 Reversed => new(To, From);
    public Ray3 ToRay() => new(From, Direction);
    public float Length => (To - From).Length();
    public Vector3 this[int i] {
        get => i switch { 0 => From, 1 => To, _ => throw new ArgumentOutOfRangeException(nameof(i)) };
    }
    public static implicit operator Ray3(Line3 line) => line.ToRay();
    public override string ToString() => "{}";
}
