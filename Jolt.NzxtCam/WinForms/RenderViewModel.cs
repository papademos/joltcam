using System.Diagnostics;

namespace Jolt.NzxtCam;

class RenderViewModel
{
    public Stopwatch Watch { get; } = new();
    public int ScaleFactor { get; set; } = 1;
    public Size Size { get; set; } = new(250, 250);
    public List<IEffect> Effects { get; } = new();
    public float ElapsedSeconds => (float)Watch.Elapsed.TotalSeconds;

    // TODO: This should probably be outside the view model.
    public void RenderGif(string path, Size size, float t0, float t1, int frameCount) {
        var w = size.Width;
        var h = size.Height;
        using var stream = new MemoryStream();
        using var encoder = new GifEncoder(stream, w, h);
        var renderer = new GifRenderer(w, h, encoder, Render);
        renderer.RenderFrames(t0, t1, frameCount);
        if (stream.Length >= 20 * 1024 * 1024) {
            throw new InvalidOperationException();
        }
        var fileStream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        stream.WriteTo(fileStream);
        return;
    }

    // TODO: This should probably be outside the view model.
    public void Render(RenderContext context) {
        foreach (var effect in Effects) {
            effect.Render(context);
        }
    }
}
