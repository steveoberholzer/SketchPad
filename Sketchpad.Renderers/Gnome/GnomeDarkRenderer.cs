namespace Sketchpad.Renderers.Gnome;

public class GnomeDarkRenderer : GnomeRenderer
{
    public GnomeDarkRenderer() : base(new GnomeDarkTheme()) { }
    public override string DisplayName => "GNOME Dark";
}
