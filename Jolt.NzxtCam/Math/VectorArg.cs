using System.Numerics;

namespace Jolt.NzxtCam;

record struct VectorArg(in Vector4 Vector)
{
    public static implicit operator VectorArg(in Vector4 v4) => new(v4);
    public static implicit operator VectorArg(in Vector3 v3) => new(new(v3, 0));
    public static implicit operator VectorArg(in Vector2 v2) => new(new(v2, 0, 0));
    public static implicit operator PointF(in VectorArg a) => new(a.Vector.X, a.Vector.Y);
    public override string ToString() => "{}";
}