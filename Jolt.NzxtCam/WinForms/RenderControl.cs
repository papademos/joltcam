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
        var t = viewModel.ElapsedSeconds;
        //var t = 8.721934f;
        //var t = 13.9439611f;
        //var t = 0.15f;
        var context = new RenderContext(g, new(w, h), t);
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

    protected override void OnMouseDown(MouseEventArgs args) {
        base.OnMouseDown(args);
        if (viewModel.Watch.IsRunning) {
            viewModel.Watch.Stop();
        } else {
            viewModel.Watch.Start();
        }
    }

    private void UpdateDimensions() {
        Size = viewModel.Size * viewModel.ScaleFactor;
    }
}
