using System.Windows.Media;

namespace Sketchpad.Renderers.Gnome;

/// Adwaita dark palette — overrides the light defaults from GnomeTheme.
public class GnomeDarkTheme : GnomeTheme
{
    public GnomeDarkTheme()
    {
        PageBg       = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x1E));
        CardBg       = new SolidColorBrush(Color.FromRgb(0x2D, 0x2D, 0x2D));
        CardBorder   = new SolidColorBrush(Color.FromRgb(0x42, 0x42, 0x42));
        HeaderbarBg  = new SolidColorBrush(Color.FromRgb(0x14, 0x14, 0x14));
        Destructive  = new SolidColorBrush(Color.FromRgb(0xFF, 0x7B, 0x63));
        ButtonBg     = new SolidColorBrush(Color.FromRgb(0x3D, 0x3D, 0x3D));
        ButtonBorder = new SolidColorBrush(Color.FromRgb(0x52, 0x52, 0x52));
        InputBg      = new SolidColorBrush(Color.FromRgb(0x25, 0x25, 0x25));
        InputBorder  = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));
        SidebarBg    = new SolidColorBrush(Color.FromRgb(0x24, 0x24, 0x24));
        DarkText     = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
        MutedText    = new SolidColorBrush(Color.FromRgb(0x87, 0x87, 0x87));
        Separator    = new SolidColorBrush(Color.FromRgb(0x42, 0x42, 0x42));
        SuccessBg    = new SolidColorBrush(Color.FromRgb(0x1A, 0x3A, 0x1F));
        WarningBg    = new SolidColorBrush(Color.FromRgb(0x3A, 0x2A, 0x00));
        ErrorBg      = new SolidColorBrush(Color.FromRgb(0x3A, 0x0A, 0x0A));
        ActiveItemBg = new SolidColorBrush(Color.FromRgb(0x35, 0x84, 0xE4));
    }
}
