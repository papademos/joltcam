using System.Numerics;
namespace Jolt.NzxtCam;

public record struct Line3(Vector3 From, Vector3 To)
{
    public bool IsEmpty => Vector3.Distance(From, To).IsZero();
    public Vector3 Direction => Vector3.Normalize(To - From);
    public Line3 Reversed => new(To, From);
    public Ray3 ToRay() => new(From, Direction);
    public static implicit operator Ray3(Line3 line) => line.ToRay();
}
