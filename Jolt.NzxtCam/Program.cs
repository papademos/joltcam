namespace Jolt.NzxtCam;

internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main() {
        //
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // For now, this is where we toggle which effects that should be enabled.
        var viewModel = new RenderViewModel();
        viewModel.Effects.AddRange(new IEffect[] {
            //new CubeCutEffect(),
            //new CubeEffect(),
            //new TwisterEffect(),
            //new RotatingPlusEffect(),
            //new CityEffect(),
            new CubePolygonEffect(),
            new BouncingTextEffect("JOLT"),
        });

        // Create Form and Control for rendering.
        var control = new RenderControl(viewModel);
        var form = new RenderForm(viewModel, control);
        viewModel.Watch.Restart();

        // Render loop.
        form.Show();
        while (control.IsHandleCreated && !control.IsDisposed) {
            control.Invalidate();
            Application.DoEvents();
        }
    }
}
