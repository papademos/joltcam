namespace Jolt.NzxtCam;

interface IEffect
{
    public void Render(RenderContext context);
    public bool Enabled { get; set; }
}

abstract class EffectBase : IEffect
{
    public abstract void Render(RenderContext context);
    public virtual bool Enabled { get; set; }
}