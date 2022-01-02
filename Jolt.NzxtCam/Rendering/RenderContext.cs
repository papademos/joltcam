namespace Jolt.NzxtCam;

public class RenderContext
{
    public Graphics Graphics { get; }
    public Size Size { get; }
    public float ElapsedSeconds { get; }
    public CBuffer? Cbuffer { get; }
    public ZBuffer? Zbuffer { get; }

    public RenderContext(Graphics graphics, Size size, float elapsedSeconds, 
        CBuffer? cbuffer = null,
        ZBuffer? zbuffer = null) {
        Graphics = graphics;
        Size = size;
        ElapsedSeconds = elapsedSeconds;
        Cbuffer = cbuffer;
        Zbuffer = zbuffer;
    }

    public void Deconstruct(out Graphics Graphics, out Size Size, out float ElapsedSeconds)
        => (Graphics, Size, ElapsedSeconds) = (this.Graphics, this.Size, this.ElapsedSeconds);
}