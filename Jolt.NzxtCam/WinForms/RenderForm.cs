namespace Jolt.NzxtCam;

class RenderForm : Form {
    private readonly RenderViewModel viewModel;
    private readonly RenderControl renderControl;

    public RenderForm(RenderViewModel viewModel, RenderControl renderControl) {
        this.viewModel = viewModel;
        BackColor = Color.Black;
        KeyPreview = true;
        Controls.Add(this.renderControl = renderControl);
        renderControl.SizeChanged += (sender, args) => UpdateDimensions();
        UpdateDimensions();
    }

    private void UpdateDimensions() {
        var extraSize = Size - ClientSize;
        Size = extraSize + renderControl.Size;
    }

    protected override void OnKeyDown(KeyEventArgs args) {
        switch (args.KeyCode) {
            case Keys.G:
                // TODO: This should probably be handled elsewhere.
                viewModel.RenderGif($"effect.{Guid.NewGuid()}.gif", viewModel.Size, 0, 10, 600);
                break;
        }
    }
}
