namespace Jolt.NzxtCam;

class RenderControl : Control {
    private readonly RenderViewModel viewModel;

    public RenderControl(RenderViewModel viewModel) {
        this.viewModel = viewModel;
        DoubleBuffered = true;
        UpdateDimensions();
    }

    protected override void OnPaint(PaintEventArgs args) {
        var scale = viewModel.ScaleFactor;
        var g = args.Graphics;
        var w = Width / viewModel.ScaleFactor;
        var h = Height / viewModel.ScaleFactor;
        g.ScaleTransform(scale, scale);
        g.Clear(Color.Black);
        var context = new RenderContext(g, new(w, h), viewModel.ElapsedSeconds);
        viewModel.Render(context);
    }

    protected override void OnMouseWheel(MouseEventArgs args) {
        if (Math.Sign(args.Delta) < 0) {
            viewModel.ScaleFactor = Math.Max(1, viewModel.ScaleFactor / 2);
        } else {
            viewModel.ScaleFactor = Math.Min(16, viewModel.ScaleFactor * 2);
        }
        UpdateDimensions();
    }

    private void UpdateDimensions() {
        Size = viewModel.Size * viewModel.ScaleFactor;
    }
}
