using System.Collections.Concurrent;
using static Jolt.NzxtCam.MathF;
namespace Jolt.NzxtCam;

public delegate void RenderAction(RenderContext context);
record RenderState(float T, float dt, int Frame);

class GifRenderer {
    private readonly int width;
    private readonly int height;
    private readonly RenderAction render;
    private int readIndex = 0;
    private readonly List<Bitmap?> frames = new();
    private readonly GifEncoder encoder;
    private readonly object frameLock = new();
    private ManualResetEvent doneWaitHandle = new(false);
    private int remainingFrameCount = 0;
    private ConcurrentStack<CBuffer> cbuffers = new();
    private ConcurrentStack<ZBuffer> zbuffers = new();

    public GifRenderer(int width, int height, GifEncoder encoder, RenderAction render) {
        this.width = width;
        this.height = height;
        this.encoder = encoder;
        this.render = render;
    }

    public void RenderFrames(float t0, float t1, int frameCount) {
        remainingFrameCount = frameCount;
        doneWaitHandle.Reset();
        for (int i = 0; i < frameCount; i++) {
            var t = Lerp(t0, t1, (float)i / frameCount);
            var dt = (t1 - t0) / frameCount;
            ThreadPool.QueueUserWorkItem(Render, new RenderState(t, dt, i));
        }
        doneWaitHandle.WaitOne();
        doneWaitHandle.Reset();
    }

    private void Render(object? args) {
        // Get/create buffers.
        if (!cbuffers.TryPop(out var cbuffer)) {
            cbuffer = new(new(width, height));
        }
        if (!zbuffers.TryPop(out var zbuffer)) {
            zbuffer = new(new(width, height));
        }

        try {
            var (t, dt, writeIndex) = (RenderState)args!;
             // TODO: Perhaps specify pixelformat?
             // TODO: Remove the need for this bitmap, use the cbuffer bitmap instead.
            var bitmap = new Bitmap(width, height);
            using var graphics = Graphics.FromImage(bitmap);
            render(new(graphics, bitmap.Size, t, cbuffer, zbuffer));

            lock (frameLock) {
                while (frames.Count <= writeIndex) {
                    frames.Add(null);
                }
                frames[writeIndex] = bitmap;
                while (readIndex < frames.Count && frames[readIndex] != null) {
                    var frame = frames[readIndex];
                    if (frame == null) {
                        throw new NullReferenceException();
                    }
                    frames[readIndex++] = null;
                    encoder.AddFrame(frame, 0, 0, TimeSpan.FromSeconds(dt));
                    if (Interlocked.Decrement(ref remainingFrameCount) == 0) {
                        doneWaitHandle.Set();
                    }
                }
            }

        }
        finally {
            // Release buffers
            cbuffers.Push(cbuffer);
            zbuffers.Push(zbuffer);
        }
    }
}
