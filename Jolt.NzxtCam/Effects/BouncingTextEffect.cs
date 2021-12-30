using System.Drawing.Drawing2D;
using static Jolt.NzxtCam.MathF;
namespace Jolt.NzxtCam;

class BouncingTextEffect : EffectBase
{
    private readonly Font font = new("Arial", 50);
    private readonly string text;

    public BouncingTextEffect(string text) {
        this.text = text;
    }

    public override void Render(RenderContext context) {
        // Update
        var (g, size, t) = context;
        var y = 200 - 100 * Sqrt(Abs(Sin(t)));

        // Render
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = SmoothingMode.HighQuality;
        var s = StringFormat.GenericTypographic;
        s.Alignment = StringAlignment.Center;
        s.LineAlignment = StringAlignment.Near;
        g.DrawString(text, font, Brushes.LightGray, new RectangleF(0, y, size.Width, size.Height), s);
    }
}
