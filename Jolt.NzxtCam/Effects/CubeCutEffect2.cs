using System.Drawing.Drawing2D;
using System.Numerics;
using static Jolt.NzxtCam.FaceHelper;
using static Jolt.NzxtCam.MathF;
using static Jolt.NzxtCam.ModelFactory;
using static System.Numerics.Matrix4x4;
namespace Jolt.NzxtCam;

class CubeCutEffect2 : EffectBase {
    public override void Render(RenderContext context) {
        //
        var cbuffer = context.Cbuffer;
        cbuffer?.Clear();

        // Update
        var (graphics, size, t) = context;
        var (w, h) = (size.Width, size.Height);
        var mObj = new[] {
            RotZ(.1f + t * .02f ) *
            RotY(.2f + t * .01f ) *
            RotX(.3f - t * .3f  ),
            RotZ(.4f + t * .005f) *
            RotY(.5f + t * .25f ) *
            RotX(.6f + t * .015f),

        };
        var models = new[] {
            CreateCuboid4(50, Color.FromArgb(255, 160, 64)),
            CreateCuboid4(50, Color.FromArgb(64, 160, 255)),
        };

        //
        var viewMatrix = CreateLookAt(
            cameraPosition: V(0, 0, -150),
            cameraTarget: V(0, 0, 0),
            cameraUpVector: V(0, -1, 0));
        for (int i = 0; i < models.Length; i++) {
            var model = models[i];
            var m = mObj[i] *= viewMatrix;
            if (!Invert(m, out var mi)) {
                throw new InvalidOperationException();
            }
            var mit = Transpose(mi);
            Transform(model.Positions, m);
            var faces = model.Faces;
            for (int j = 0; j <  model.Faces.Count; j++) {
                var normal = Vector3.TransformNormal(faces[j].Normal, mit );
                faces[j] = faces[j] with { Normal = normal };
            }
        }
        
        // Calculate color based on the view angle.
        foreach (var model in models) {
            for (int i = 0; i <  model.Faces.Count; i++) {
                var face = model.Faces[i];
                var z = model.CalculateNormal(i).Z;
                if (float.IsNaN(z)) {
                    throw new InvalidOperationException();
                }

                z = Pow(Saturate(z), 2);
                var color = face.Color;
                var a = 255;
                var r = (int)(z * color.R);
                var g = (int)(z * color.G);
                var b = (int)(z * color.B);
                model.Faces[i].Color = Color.FromArgb(a, r, g, b);
            }
        }

        //
        var bsp0 = new BspTree();
        bsp0.Add(models[0]);
        bsp0.Add(models[1]);
        var modelFaces = bsp0.Iterate().ToArray();
        modelFaces = modelFaces.Where(mf => {
            var plane = mf.Model.CalculatePlane(mf.Face);
            return Sign(Plane.DotCoordinate(plane, Vector3.Zero)) > 0;
        }).ToArray();

        //
        var znear = 10f;
        var zfar = 1001f;
        var projectionMatrix = CreatePerspective(w, h, znear, zfar);
        foreach (var positions in models.Select(model => model.Positions).Distinct()) {
            Transform(positions, projectionMatrix);
            TransformToScreen(positions, znear, zfar, context.Size.Width, context.Size.Height);
        }

        // Render
        using var pen = new Pen(Color.White, 2);
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        foreach (var (model, face) in modelFaces) {
            var positions = face.Select(index => V2(model.Positions[index])).ToArray();
            PolygonRenderer.Render(cbuffer, null, new(positions), face.Color.ToArgb());
        }

        //
        var bitmap = cbuffer.UpdateBitmap();
        var state = graphics.Save();
        graphics.PixelOffsetMode = PixelOffsetMode.None;
        graphics.SmoothingMode = SmoothingMode.None;
        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        graphics.DrawImageUnscaled(bitmap, 0, 0);
        graphics.Restore(state);
    }
}

record struct ColoredPolygon2(Polygon2 Polygon, Color color)
{
    public override string ToString() => "{}";
}
