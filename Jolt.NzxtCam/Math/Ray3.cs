using System.Numerics;
namespace Jolt.NzxtCam;

public record struct Ray3(Vector3 Position, Vector3 Direction)
{
    public Line3 ToLine(float length) => new(Position, Position + Direction * length);
}
