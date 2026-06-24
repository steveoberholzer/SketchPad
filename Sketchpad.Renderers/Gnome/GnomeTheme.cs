using System.Windows.Media;

namespace Sketchpad.Renderers.Gnome;

public static class GnomeTheme
{
    public static readonly Brush PageBg       = new SolidColorBrush(Color.FromRgb(0xF6, 0xF5, 0xF4));
    public static readonly Brush CardBg       = Brushes.White;
    public static readonly Brush CardBorder   = new SolidColorBrush(Color.FromRgb(0xDE, 0xDD, 0xDA));
    public static readonly Brush HeaderbarBg  = new SolidColorBrush(Color.FromRgb(0x2D, 0x2D, 0x2D));
    public static readonly Brush HeaderbarText= Brushes.White;
    public static readonly Brush Accent       = new SolidColorBrush(Color.FromRgb(0x35, 0x84, 0xE4));
    public static readonly Brush AccentText   = Brushes.White;
    public static readonly Brush Destructive  = new SolidColorBrush(Color.FromRgb(0xC0, 0x1C, 0x28));
    public static readonly Brush ButtonBg     = new SolidColorBrush(Color.FromRgb(0xED, 0xED, 0xED));
    public static readonly Brush ButtonBorder = new SolidColorBrush(Color.FromRgb(0xC8, 0xC7, 0xC7));
    public static readonly Brush InputBg      = Brushes.White;
    public static readonly Brush InputBorder  = new SolidColorBrush(Color.FromRgb(0xC8, 0xC7, 0xC7));
    public static readonly Brush SidebarBg    = new SolidColorBrush(Color.FromRgb(0xED, 0xED, 0xED));
    public static readonly Brush DarkText     = new SolidColorBrush(Color.FromRgb(0x2E, 0x2E, 0x2E));
    public static readonly Brush MutedText    = new SolidColorBrush(Color.FromRgb(0x77, 0x76, 0x74));
    public static readonly Brush Separator    = new SolidColorBrush(Color.FromRgb(0xDE, 0xDD, 0xDA));
    public static readonly Brush SuccessBg    = new SolidColorBrush(Color.FromRgb(0xD6, 0xEF, 0xD8));
    public static readonly Brush WarningBg    = new SolidColorBrush(Color.FromRgb(0xFE, 0xF3, 0xD2));
    public static readonly Brush ErrorBg      = new SolidColorBrush(Color.FromRgb(0xF8, 0xD8, 0xDA));
    public static readonly Brush ActiveItemBg = new SolidColorBrush(Color.FromRgb(0x35, 0x84, 0xE4));

    public static readonly FontFamily Font = new("Cantarell, Ubuntu, Segoe UI");

    public const double FontSize      = 11;
    public const double HeaderHeight  = 46;
    public const double CornerRadius  = 6;
    public const double CardRadius    = 12;
    public const double Pad           = 12;
    public const double Gap           = 8;
}
