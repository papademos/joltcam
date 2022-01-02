using System.Numerics;

namespace Jolt.NzxtCam;

static class MathF
{
    // The smallest value which is considered to be non-zero.
    public const float Epsilon = 1e-6f;
    public static bool IsZero(this float x) => Abs(x) < Epsilon;
    public static int Sign(float x) => IsZero(x) ? 0 : Math.Sign(x);
    public static float RadToRev(float rad) => rad / 2 / (float)Math.PI;
    public static float RevToRad(float rev) => rev * 2 * (float)Math.PI;
    public static float Sin(float rev) => (float)Math.Sin(RevToRad(rev));
    public static float Cos(float rev) => (float)Math.Cos(RevToRad(rev));
    public static float Abs(float x) => x < 0 ? -x : x;
    public static float Pow(float x, float y) => (float)Math.Pow(x, y);
    public static float Sqrt(float x) => (float)Math.Sqrt(x);
    public static float Clamp(float x, float min, float max) => x < min ? min : x < max ? x : max;
    public static float Min(float x, float y) => x < y ? x : y;
    public static float Max(float x, float y) => x > y ? x : y;
    public static int Min(int x, int y) => x < y ? x : y;
    public static int Max(int x, int y) => x > y ? x : y;
    public static long Min(long x, long y) => x < y ? x : y;
    public static long Max(long x, long y) => x > y ? x : y;
    public static float Saturate(float x) => Clamp(x, 0, 1);
    public static long HiLo(int hi, int lo) => (((long)hi) << 32) | ((uint)lo);
    public static Matrix4x4 RotX(float a) => Matrix4x4.CreateRotationX(RevToRad(a));
    public static Matrix4x4 RotY(float a) => Matrix4x4.CreateRotationY(RevToRad(a));
    public static Matrix4x4 RotZ(float a) => Matrix4x4.CreateRotationZ(RevToRad(a));
    public static Vector2 Transform(Vector2 v, Matrix4x4 m) => Vector2.Transform(v, m);
    public static Vector3 Transform(Vector3 v, Matrix4x4 m) => Vector3.Transform(v, m);
    public static Vector3 TransformNormal(Vector3 v, Matrix4x4 m) => Vector3.TransformNormal(v, m);
    public static Vector4 Transform(Vector4 v, Matrix4x4 m) => Vector4.Transform(v, m);
    public static float Acos(float value) => RadToRev((float)Math.Acos(value));
    public static float Atan(float value) => RadToRev((float)Math.Atan(value));
    public static float Atan(float y, float x) => RadToRev((float)Math.Atan2(y, x));
    public static float Angle(Vector3 u, Vector3 v) => Acos(Vector3.Dot(u, v));
    public static float Lerp(float min, float max, float t) => min + t * (max - min);
    public static float Cross(Vector2 u, Vector2 v) => u.X * v.Y - u.Y * v.X; // Note: This is basically the Z result of the 3D cross product.

    //
    public static float AssertNormal(this float x) => float.IsNormal(x) ? x : throw new InvalidOperationException();
    public static float AssertFinite(this float x) => float.IsFinite(x) ? x : throw new InvalidOperationException();

    //
    public static bool IsEqual(this Vector3 a, Vector3 b, float epsilon = Epsilon) => Vector3.Distance(a, b) <= epsilon;

    //
    public static Vector2 V(float x, float y) => new(x, y);
    public static Vector3 V(float x, float y, float z) => new(x, y, z);
    public static Vector4 V(float x, float y, float z, float w) => new(x, y, z, w);
    public static Vector2 V2(Vector3 v3) => new(v3.X, v3.Y);
    public static Vector3 V3(Vector4 v4) => new(v4.X, v4.Y, v4.Z);

    //
    public static void Transform(IList<Vector3> positions, Matrix4x4 m) {
        for (int i = 0; i < positions.Count; i++) {
            positions[i] = Transform(positions[i], m);
        }
    }
    public static void Transform(IList<Vector4> positions, Matrix4x4 m) {
        for (int i = 0; i < positions.Count; i++) {
            positions[i] = Transform(positions[i], m);
        }
    }
    public static void TransformToScreen(IList<Vector3> positions, float znear, float zfar, float width, float height) {
        for (int i = 0; i < positions.Count; i++) {
            var p = positions[i];
            positions[i] = V(
                (zfar - znear) * p.X / p.Z + width / 2,
                (zfar - znear) * p.Y / p.Z + height / 2,
                p.Z);
        }
    }
    public static void TransformToScreen(IList<Vector4> positions, float znear, float zfar, float width, float height) {
        for (int i = 0; i < positions.Count; i++) {
            var p = positions[i];
            positions[i] = V(
                (zfar - znear) * p.X / p.Z + width / 2,
                (zfar - znear) * p.Y / p.Z + height / 2,
                p.Z, p.W);
        }
    }
}
