#pragma warning disable IDE1006

using static Jolt.NzxtCam.MathF;

namespace Jolt.NzxtCam;

record struct int2(int X, int Y)
{
    public int X = X, Y = Y;
    public static implicit operator int2((int X, int Y) a) => new(a.X, a.Y);
    public long ToInt64 => HiLo(X, Y);
    public int this[int i] {
        get => i switch {
            0 => X, 1 => Y, _ => throw new ArgumentException(nameof(i))
        };
        set { 
            switch(i) {
                case 0: X = value; break;
                case 1: Y = value; break;
                default: throw new ArgumentException(nameof(i));
            }
        }
    }

    public override string ToString() => "{}";
}