using System.Numerics;
using static Jolt.NzxtCam.MathF;
using static System.Numerics.Matrix4x4;
namespace Jolt.NzxtCam;

internal class RotatingPlusEffect : EffectBase
{    private readonly Model model = default(Model) with {
        Positions = new Vector3[] {
            V(-10,-10, 0), V(-10,-30, 0), V( 10,-30, 0),  
            V( 10,-10, 0), V( 30,-10, 0), V( 30, 10, 0),
            V( 10, 10, 0), V( 10, 30, 0), V(-10, 30, 0),
            V(-10, 10, 0), V(-30, 10, 0), V(-30,-10, 0),
        },
        Faces = new[] { new Face(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11) },
    };

    public override void Render(RenderContext context) {
        // Update
        var (g, size, t) = context;

        // Render
        var a = 0.01f * t;
        var m = RotZ(a) *
            CreateTranslation(125, 125, 0);
        var positions = model.Positions.ToArray();
        Transform(positions, m);

        var lineManager = new LineManager();
        ModelRenderer.RenderFaceLines(context, lineManager, new(positions.ToArray(), model.Faces.ToArray(), Color.FromArgb(128, Color.DeepSkyBlue)));
    }
}
