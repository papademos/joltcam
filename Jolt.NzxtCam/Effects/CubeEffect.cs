using System.Numerics;
using static Jolt.NzxtCam.MathF;
using static System.Numerics.Matrix4x4;
namespace Jolt.NzxtCam;

class CubeEffect : EffectBase
{
    private readonly Model model = default(Model) with {
        Positions = new Vector3[] {
            V(-50, -50, - -50),
            V( 50, -50, - -50),
            V( 50,  50, - -50),
            V(-50,  50, - -50),
            V(-50, -50, -  50),
            V( 50, -50, -  50),
            V( 50,  50, -  50),
            V(-50,  50, -  50),
        },
        Faces = new Face[] {
            new(0, 1, 2, 3), // front
            new(1, 5, 6, 2), // right
            new(5, 4, 7, 6), // back
            new(4, 0, 3, 7), // left
            new(4, 5, 1, 0), // top
            new(3, 2, 6, 7), // bottom
        },
        Color = Color.RebeccaPurple,
    };

    public override void Render(RenderContext context) {
        // Update
        var (g, size, t) = context;
        var m =
            RotZ(0.02f * t) *
            RotY(0.01f * t) *
            RotX(-0.3f * t);
        var viewMatrix = CreateLookAt(
            cameraPosition: V(0, 0, -150),
            cameraTarget: V(0, 0, 0),
            cameraUpVector: V(0, -1, 0));
        var znear = 10f;
        var zfar = 1001f;
        var projectionMatrix = CreatePerspective(250, 250, znear, zfar);
        m *= viewMatrix;
        m *= projectionMatrix;
        var positions = model.Positions.ToArray();
        Transform(positions, m);
        TransformToScreen(positions, znear, zfar, context.Size.Width, context.Size.Height);

        //
        var lineManager = new LineManager();
        ModelRenderer.RenderFaceLines(context, lineManager, model with { Positions = positions });
    }
}
