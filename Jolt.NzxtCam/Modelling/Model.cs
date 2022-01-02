using System.Numerics;
using static Jolt.NzxtCam.MathF;
namespace Jolt.NzxtCam;

record struct Model(List<Vector3> Positions, List<Face> Faces, Color Color)
{
    public Vector3 CalculateNormal(int faceIndex) => CalculateNormal(Faces[faceIndex]);
    public Vector3 CalculateNormal(Face face) {
        var p = Positions;
        var (i0, i1, i2) = (face[0], face[1], face[^1]);
        var (p0, p1, p2) = (p[i0], p[i1], p[i2]);
        var u = Vector3.Normalize(p1 - p0);
        var v = Vector3.Normalize(p2 - p0);
        var w = Vector3.Cross(u, v);
        return w;
    }
    public void Cull(Matrix4x4 m) {
        for (int i = Faces.Count - 1; i >= 0; i--) {
            var face = Faces[i];
            var p = Positions;
            var (i0, i1, i2) = (face[0], face[1], face[^1]);
            var (p0, p1, p2) = (p[i0], p[i1], p[i2]);
            p0 = Transform(p0, m);
            p1 = Transform(p1, m);
            p2 = Transform(p2, m);
            var u = Vector3.Normalize(p1 - p0);
            var v = Vector3.Normalize(p2 - p0);
            var w = Vector3.Cross(u, v);
            if (w.Z < 0) {
                Faces.RemoveAt(i);
            }
        }
    }
}

record struct Model4(List<Vector4> Positions, List<Face> Faces, Color Color)
{
    public Vector3 CalculateNormal(int faceIndex) => CalculateNormal(Faces[faceIndex]);
    public Vector3 CalculateNormal(Face face) {
        var p = Positions;
        var (i0, i1, i2) = (face[0], face[1], face[^1]);
        var (p0, p1, p2) = (V3(p[i0]), V3(p[i1]), V3(p[i2]));
        var u = Vector3.Normalize(p1 - p0);
        var v = Vector3.Normalize(p2 - p0);
        var w = Vector3.Cross(u, v);
        return w;
    }
}