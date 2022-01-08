using System.Numerics;
using static Jolt.NzxtCam.MathF;
namespace Jolt.NzxtCam;

internal static class FaceHelper
{
    public static (Face? Neg, Face? Zero, Face? Pos) Split(List<Vector4> modelPositions, Face face, Plane plane) {
        var c = face.Count;
        var facePositions = face.Select(i => V3(modelPositions[i])).ToArray();
        static int S(float f) => Abs(f) < 1e-3 ? 0 : Sign(f);
        float D(int i) => Distance(facePositions[i], plane);
        var distances = Enumerable.Range(0, face.Count).Select(D).ToArray();

        //
        var lastSign = 0;
        var last = c;
        while (--last >= 0 && (lastSign = S(D(last))) == 0) ;
        if (last < 0) {
            return (null, face, null);
        }

        //
        var i3 = last;
        while (--i3 >= 0 && S(D(i3)) != -lastSign) ;
        if (i3 < 0) {
            return lastSign < 0
                ? (null, null, face)
                : (face, null, null);
        }

        //
        var i0 = i3;
        while (--i0 >= 0 && S(D(i0)) != lastSign) ;
        if (i0 < 0) {
            i0 = last;
        }

        // First split: Turning i0-i2 into i0-i1-i2.
        var i2 = (i0 + 1) % c;
        var line0 = new Line3(facePositions[i0], facePositions[i2]);
        if (!Intersects(line0, plane, out var p1)) {
            // We usually get here when something is to small to be intersected.
            var distanceSum = face.Sum(i => Distance(V3(modelPositions[i]), plane));
            return 
                IsZero(distanceSum) ? (null, face, null) : 
                distanceSum < 0 ? (face, null, null) : 
                (null, null, face);
        }

        // Second split: Turning i3-i5 into i3-i4-i5.
        var i5 = i3 + 1; // i3 < i6 < c.
        var line1 = new Line3(facePositions[i3], facePositions[i5]);
        if (!Intersects(line1, plane, out var p4)) {
            // We usually get here when something is to small to be intersected.
            var distanceSum = face.Sum(i => Distance(V3(modelPositions[i]), plane));
            return 
                IsZero(distanceSum) ? (null, face, null) : 
                distanceSum < 0 ? (face, null, null) : 
                (null, null, face);
        }

        // Add the split points to the position buffer.
        int pi1;
        if (IsEqual(facePositions[i0], p1)) {
            pi1 = face[i0];
        } else if (IsEqual(facePositions[i2], p1)) {
            pi1 = face[i2];
        } else {
            pi1 = modelPositions.Count;
            modelPositions.Add(V4(p1, 1));
        }

        int pi4;
        if (IsEqual(facePositions[i3], p4)) {
            pi4 = face[i3];
        } else if (IsEqual(facePositions[i5], p4)) {
            pi4 = face[i5];
        } else {
            pi4 = modelPositions.Count;
            modelPositions.Add(V4(p4, 1));
        }

        //
        static Face CreateFace(Face face, int pi0, int fi0, int fi1, int pi1) {
            var indices = new List<int>();
            int fi = fi0;
            if (pi0 != face[fi0]) {
                indices.Add(pi0);
            }
            for (; fi != fi1; fi = ++fi % face.Count) {
                indices.Add(face[fi]);
            }
            indices.Add(face[fi]);
            if (pi1 != face[fi]) {
                indices.Add(pi1);
            }
            return face with { Indices = indices };
        }

        var face0 = CreateFace(face, pi1, i2, i3, pi4);
        var face1 = CreateFace(face, pi4, i5, i0, pi1);

        // Sanity checks.
        for (int i = 0; i < face0.Count; i++) {
            for (int j = i + 1; j < face0.Count; j++) {
                if (IsEqual(V3(modelPositions[face0[i]]), V3(modelPositions[face0[j]]))) {
                    throw new InvalidOperationException();
                }
            }
        }
        for (int i = 0; i < face1.Count; i++) {
            for (int j = i + 1; j < face1.Count; j++) {
                if (IsEqual(V3(modelPositions[face1[i]]), V3(modelPositions[face1[j]]))) {
                    throw new InvalidOperationException();
                }
            }
        }

        //
        return lastSign < 0
            ? (face0, null, face1)
            : (face1, null, face0);
    }

    public static bool Intersects(in Line3 line, in Plane plane, out Vector3 position) {
        var (d0, d1) = (Distance(line[0], plane), Distance(line[1], plane));
        if (Sign(d0) == Sign(d1)) {
            position = default;
            return false;
        }

        var direction = Plane.DotNormal(plane, line.Direction);
        var korv = Plane.DotNormal(plane, line[0]);
        var distance = (-plane.D - korv) / direction;
        var t = Saturate(distance / line.Length);
        position = Vector3.Lerp(line[0], line[1], t);
        return true;
    }

    public static bool Intersects(in Ray3 ray, in Plane plane, out float distance) {
        var direction = Plane.DotNormal(plane, ray.Direction);
        if (direction == Zero) {
            distance = 0f;
            return false;
        }

        var position = Plane.DotNormal(plane, ray.Position);
        distance = (-plane.D - position) / direction;
        if (distance < 0f) {
            distance = 0f;
            return false;
        }

        return true;
    }

    public static float Distance(Vector3 point, Plane plane)
        => Plane.DotCoordinate(plane, point);
}