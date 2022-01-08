using System.Numerics;
namespace Jolt.NzxtCam;

public record struct Ray2(Vector2 Position, Vector2 Direction)
{
    public Line2 ToLine(float length) => new(Position, Position + Direction * length);
    public override string ToString() => "{}";
}
