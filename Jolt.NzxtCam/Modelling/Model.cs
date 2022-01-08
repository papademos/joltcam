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
        var w = Vector3.Normalize(Vector3.Cross(u, v));
        return w;
    }
    public override string ToString() => "{}";
}

record struct Model4(List<Vector4> Positions, List<Face> Faces)
{
    public Vector3 CalculateNormal(int faceIndex) => CalculateNormal(Faces[faceIndex]);
    public Vector3 CalculateNormal(Face face) {
        var p = Positions;
        var (i0, i1, i2) = (face[0], face[1], face[^1]);
        var (p0, p1, p2) = (V3(p[i0]), V3(p[i1]), V3(p[i2]));
        var u = Vector3.Normalize(p1 - p0);
        var v = Vector3.Normalize(p2 - p0);
        var w = Vector3.Normalize(Vector3.Cross(u, v));
        return w;
    }
    public Plane CalculatePlane(int faceIndex) => CalculatePlane(Faces[faceIndex]);
    public Plane CalculatePlane(Face face) {
        var p = Positions;
        var (i0, i1, i2) = (face[0], face[1], face[^1]);
        var (p0, p1, p2) = (p[i0], p[i1], p[i2]);
        var plane = Plane.CreateFromVertices(V3(p0), V3(p1), V3(p2));
        if (
            float.IsNaN(plane.Normal.X) ||
            float.IsNaN(plane.Normal.Y) ||
            float.IsNaN(plane.Normal.Z) ||
            float.IsNaN(plane.D)) {
            throw new InvalidOperationException();
        }
        return plane;
    }
    public bool IsClockwise(int faceIndex) {
        var p = Positions;
        var face = Faces[faceIndex];
        var (i0, i1, i2) = (face[0], face[1], face[^1]);
        var (p0, p1, p2) = (V2(p[i0]), V2(p[i1]), V2(p[i2]));
        var u = p1 - p0;
        var v = p2 - p0;
        var w = Cross(u, v);
        return w > 0;
    }    
    public Polygon4 CreatePolygon4(int faceIndex) => CreatePolygon4(Faces[faceIndex]);
    public Polygon4 CreatePolygon4(Face face) {
        var positions = Positions;
        return new(face.Select(i => positions[i]));
    }
    public override string ToString() => "{}";
}