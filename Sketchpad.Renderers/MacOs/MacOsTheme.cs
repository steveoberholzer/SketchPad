using System.Windows.Media;

namespace Sketchpad.Renderers.MacOs;

public static class MacOsTheme
{
    // Modern macOS (Big Sur / Ventura) colours
    public static readonly Brush WindowBg     = Brushes.White;
    public static readonly Brush TitlebarBg   = new SolidColorBrush(Color.FromRgb(0xEB, 0xEB, 0xEB));
    public static readonly Brush SidebarBg    = new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0));
    public static readonly Brush ContentBg    = Brushes.White;
    public static readonly Brush CardBg       = Brushes.White;
    public static readonly Brush CardBorder   = new SolidColorBrush(Color.FromRgb(0xE5, 0xE5, 0xE5));
    public static readonly Brush ToolbarBg    = new SolidColorBrush(Color.FromRgb(0xF5, 0xF5, 0xF5));
    public static readonly Brush Accent       = new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xFF));
    public static readonly Brush AccentText   = Brushes.White;
    public static readonly Brush DarkText     = new SolidColorBrush(Color.FromRgb(0x1D, 0x1D, 0x1F));
    public static readonly Brush SecText      = new SolidColorBrush(Color.FromRgb(0x6E, 0x6E, 0x73));
    public static readonly Brush Separator    = new SolidColorBrush(Color.FromRgb(0xE5, 0xE5, 0xE5));
    public static readonly Brush InputBg      = Brushes.White;
    public static readonly Brush InputBorder  = new SolidColorBrush(Color.FromRgb(0xC7, 0xC7, 0xCC));
    public static readonly Brush ButtonBg     = Brushes.White;
    public static readonly Brush ButtonBorder = new SolidColorBrush(Color.FromRgb(0xD1, 0xD1, 0xD6));
    public static readonly Brush Destructive  = new SolidColorBrush(Color.FromRgb(0xFF, 0x3B, 0x30));
    public static readonly Brush SuccessBg    = new SolidColorBrush(Color.FromRgb(0xD4, 0xED, 0xDA));
    public static readonly Brush WarningBg    = new SolidColorBrush(Color.FromRgb(0xFF, 0xF3, 0xCD));
    public static readonly Brush ErrorBg      = new SolidColorBrush(Color.FromRgb(0xF8, 0xD7, 0xDA));
    public static readonly Brush SwitchOn     = new SolidColorBrush(Color.FromRgb(0x30, 0xD1, 0x58));
    public static readonly Brush SwitchOff    = new SolidColorBrush(Color.FromRgb(0xE5, 0xE5, 0xEA));
    public static readonly Brush AltRow       = new SolidColorBrush(Color.FromRgb(0xF9, 0xF9, 0xF9));

    // Traffic lights
    public static readonly Brush TrafficRed    = new SolidColorBrush(Color.FromRgb(0xFF, 0x5F, 0x57));
    public static readonly Brush TrafficYellow = new SolidColorBrush(Color.FromRgb(0xFF, 0xBD, 0x2E));
    public static readonly Brush TrafficGreen  = new SolidColorBrush(Color.FromRgb(0x28, 0xC8, 0x40));

    public static readonly FontFamily Font = new("SF Pro Display, Helvetica Neue, Segoe UI");
    public const double FontSize      = 13;
    public const double TitleHeight   = 28;
    public const double CornerRadius  = 10;
    public const double WinRadius     = 12;
    public const double Pad           = 16;
    public const double Gap           = 8;
}
