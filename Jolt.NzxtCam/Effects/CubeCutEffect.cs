//using System.Numerics;
//using static Jolt.NzxtCam.MathF;
//using static System.Numerics.Matrix4x4;
//namespace Jolt.NzxtCam;

//class CubeCutEffect : EffectBase {
//    private readonly Model modelA = default(Model) with {
//        Positions = new() {
//            V(-50, -50, - -50),
//            V( 50, -50, - -50),
//            V( 50,  50, - -50),
//            V(-50,  50, - -50),
//            V(-50, -50, -  50),
//            V( 50, -50, -  50),
//            V( 50,  50, -  50),
//            V(-50,  50, -  50),
//        },
//        Faces = new() {
//            new(0, 1, 2, 3), // front
//            new(1, 5, 6, 2), // right
//            new(5, 4, 7, 6), // back
//            new(4, 0, 3, 7), // left
//            new(4, 5, 1, 0), // top
//            new(3, 2, 6, 7), // bottom
//        },
//    };
//    private readonly Model modelB;

//    public CubeCutEffect() {
//        modelB = modelA with {
//            Positions = modelA.Positions.ToList(),
//            Faces = modelA.Faces.ToList()
//        };
//    }
//    public override void Render(RenderContext context) {
//        // Update
//        var (g, _, t) = context;
//        var mA =
//            RotZ(0.02f * t) *
//            RotY(0.01f * t) *
//            RotX(-0.3f * t);
//        //mA =
//        //    RotZ(0.3f * t) *
//        //    RotY(0.125f);
//        var mB =
//            RotZ(0.005f * t) *
//            RotY(0.25f * t) *
//            RotX(0.015f * t);
//        //mB = Matrix4x4.Identity;
//        //mB = RotX(0.125f);

//        //
//        var s = 50;
//        var positionsA = modelA.Positions.Select(p => Transform(p, mA)).ToList();
//        var facesA = modelA.Faces.ToList();
//        {
//            var positions = positionsA;
//            var faces = facesA;
//            (faces, var front) = FaceHelper.Split(positions, faces, Plane.Transform(new Plane(0, 0, 1, s), mB));
//            (faces, var back) = FaceHelper.Split(positions, faces, Plane.Transform(new Plane(0, 0, -1, s), mB));
//            (faces, var left) = FaceHelper.Split(positions, faces, Plane.Transform(new Plane(1, 0, 0, s), mB));
//            (faces, var right) = FaceHelper.Split(positions, faces, Plane.Transform(new Plane(-1, 0, 0, s), mB));
//            (faces, var top) = FaceHelper.Split(positions, faces, Plane.Transform(new Plane(0, -1, 0, s), mB));
//            (faces, var bottom) = FaceHelper.Split(positions, faces, Plane.Transform(new Plane(0, 1, 0, s), mB));
//            faces.Clear();
//            faces.AddRange(new[] { front, back, left, right, top, bottom }.SelectMany(_ => _));
//            //faces.AddRange(new[] { front, back, left, right }.SelectMany(_ => _));
//            facesA = faces;
//        }

//        //
//        var positionsB = modelB.Positions.Select(p => Transform(p, mB)).ToList();
//        var facesB = modelB.Faces.ToList();
//        {
//            var positions = positionsB;
//            var faces = facesB;
//            (faces, var front) = FaceHelper.Split(positions, faces, Plane.Transform(new Plane(0, 0, 1, s), mA));
//            (faces, var back) = FaceHelper.Split(positions, faces, Plane.Transform(new Plane(0, 0, -1, s), mA));
//            (faces, var left) = FaceHelper.Split(positions, faces, Plane.Transform(new Plane(1, 0, 0, s), mA));
//            (faces, var right) = FaceHelper.Split(positions, faces, Plane.Transform(new Plane(-1, 0, 0, s), mA));
//            (faces, var top) = FaceHelper.Split(positions, faces, Plane.Transform(new Plane(0, -1, 0, s), mA));
//            (faces, var bottom) = FaceHelper.Split(positions, faces, Plane.Transform(new Plane(0, 1, 0, s), mA));
//            faces.Clear();
//            faces.AddRange(new[] { front, back, left, right, top, bottom }.SelectMany(_ => _));
//            facesB = faces;
//        }

//        //
//        var viewMatrix = CreateLookAt(
//            cameraPosition: V(0, 0, -150),
//            cameraTarget: V(0, 0, 0),
//            cameraUpVector: V(0, -1, 0));
//        var znear = 10f;
//        var zfar = 1001f;
//        var projectionMatrix = CreatePerspective(250, 250, znear, zfar);
//        var m =
//            viewMatrix *
//            projectionMatrix;
//        for (int ab=0; ab < 2; ab++) {
//            var positions = ab switch {
//                0 => positionsA,
//                1 => positionsB,
//                _ => throw new InvalidOperationException() };
//            Transform(positions, m);
//            TransformToScreen(positions, znear, zfar, context.Size.Width, context.Size.Height);
//        }

//        // Render
//        var lineManager = new LineManager();
//        ModelRenderer.RenderFaceLines(context, lineManager, new(positionsA, facesA, Color.FromArgb(128, Color.DeepSkyBlue)));
//        ModelRenderer.RenderFaceLines(context, lineManager, new(positionsB, facesB, Color.FromArgb(128, Color.Orange)));
//    }
//}
