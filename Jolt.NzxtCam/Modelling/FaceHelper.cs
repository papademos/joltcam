using System.Numerics;
using static Jolt.NzxtCam.MathF;
namespace Jolt.NzxtCam;

internal static class FaceHelper
{
    public static (List<Face> Neg, List<Face> Pos) Split(List<Vector3> p, List<Face> faces, Plane plane) {
        var posFaces = new List<Face>();
        var negFaces = new List<Face>();
        var pos = new List<int>(8);
        var neg = new List<int>(8);
        foreach (var face in faces) {
            // Get first sign.
            var sign = 0;
            for (int i = 0; sign == 0 && i <= face.Count; i++) {
                sign = Sign(Vector3.Dot(p[face[i]], plane.Normal) + plane.D);
            }
            if (sign == 0) {
                sign = 1;
            }

            //
            for (int i = 0; i < face.Count; i++) {
                var i0 = face[i];
                var i2 = face[i + 1];
                if (sign == 0) {
                    throw new InvalidOperationException();
                }
                var nextSign = Sign(Vector3.Dot(p[i2], plane.Normal) + plane.D);
                (sign < 0 ? neg : pos).Add(i0);
                if (nextSign == sign) {                    
                }
                else if (nextSign == 0) {
                    nextSign = sign;
                    //throw new NotImplementedException();
                }
                else {
                    var line = new Line3(p[i0], p[i2]);
                    var ray = line.ToRay();
                    var p1 = Intersection(ray, plane);
                    var i1 = p.Count;
                    p.Add(p1);
                    neg.Add(i1);
                    pos.Add(i1);
                }
                sign = nextSign;
            }

            //
            if (pos.Count > 0) {
                if (pos.Count >= 3) { // TODO :Fix this hack
                    posFaces.Add(new Face(pos.ToArray()));
                }
                pos.Clear();
            }
            if (neg.Count > 0) {
                if (neg.Count >= 3) { // TODO :Fix this hack
                    negFaces.Add(new Face(neg.ToArray()));
                }
                neg.Clear();
            }
        }
        return (posFaces, negFaces);

        //    var previous = pending;
        //    for (int i = 0; i <= face.Count; i++) {
        //        var i2 = face[i];
        //        var p2 = positions[i2];

        //        // Point is on the plane, so let's choose the same side as the previous point.
        //        var pp = ClosestPoint(p2, plane);
        //        var pv = p2 - pp;
        //        //var distance = Distance(p2, plane);
        //        if (pv.Length().IsZero()) {
        //            previous.Add(i2);
        //            continue;
        //        }

        //        // Since we have pending points, the current point and the previous point are implicitly on the same side of the plane.

        //        //var current = (distance < 0) ? back : front;
        //        var current = Vector3.Dot(plane.Normal, pv) < 0 ? back : front;
        //        if (pending.Count > 0) {
        //            current.AddRange(pending);
        //            current.Add(i2);
        //            pending.Clear();
        //            previous = current;
        //            continue;
        //        }

        //        //
        //        if (previous == pending) {
        //            previous = current;
        //        }

        //        // The current point and the previous point are on the same side of the plane.
        //        if (current == previous) {
        //            current.Add(i2);
        //            continue;
        //        }

        //        // Intersection detected.
        //        var i0 = previous.Last();
        //        var p0 = positions[i0];
        //        var i1 = positions.Count;
        //        var p1 = Intersection(new Line3(p0, p2).ToRay(), plane);
        //        positions.Add(p1);
        //        previous.Add(i1);
        //        current.Add(i1);
        //        current.Add(i2);
        //        previous = current;
        //    }

        //    //
        //    previous.RemoveAt(previous.Count - 1);

        //    //
        //    if (pending.Count > 0) {
        //        if (front.Count > 0 || back.Count > 0) {
        //            throw new InvalidOperationException();
        //        }
        //        (front, pending) = (pending, front);
        //    }
        //    if (front.Count > 0) {
        //        frontFaces.Add(new Face(front.ToArray()));
        //        front.Clear();
        //    }
        //    if (back.Count > 0) {
        //        backFaces.Add(new Face(back.ToArray()));
        //        back.Clear();
        //    }
        //}
        //return (frontFaces, backFaces);
    }

    public static Vector3 Intersection(in Ray3 ray, in Plane plane)
        => ray.Position + ray.Direction * Distance(ray, plane).AssertFinite();

    public static float Distance(in Ray3 ray, in Plane plane) {
        var direction = Vector3.Dot(plane.Normal, ray.Direction);
        if (direction == 0) {
        //if (direction.IsZero()) {
            return float.PositiveInfinity;
        }

        var from = Vector3.Dot(plane.Normal, ray.Position);
        var distance = (-plane.D - from) / direction;
        if (distance.IsZero()) {
            distance = 0;
        }
        return distance;
    }

    public static Vector3 ClosestPoint(Vector3 point, Plane plane) {
        var dot = Vector3.Dot(plane.Normal, point);
        float t = dot - plane.D;
        return point - (t * plane.Normal);
    }

    public static float Distance(Vector3 point, Plane plane)
        //=> Vector3.Dot(plane.Normal, point) - plane.D;
        => (ClosestPoint(point, plane) - point).Length();
}