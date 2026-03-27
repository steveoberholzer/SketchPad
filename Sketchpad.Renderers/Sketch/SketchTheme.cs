using System.Windows.Media;

namespace Sketchpad.Renderers.Sketch;

public static class SketchTheme
{
    public static readonly Brush Background      = new SolidColorBrush(Color.FromRgb(0xF8, 0xF8, 0xF6));
    public static readonly Brush Border          = new SolidColorBrush(Color.FromRgb(0x9A, 0x9A, 0x9A));
    public static readonly Brush DarkText        = new SolidColorBrush(Color.FromRgb(0x2A, 0x2A, 0x2A));
    public static readonly Brush MutedText       = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
    public static readonly Brush White           = Brushes.White;
    public static readonly Brush PrimaryFill     = new SolidColorBrush(Color.FromRgb(0x2A, 0x2A, 0x2A));
    public static readonly Brush DangerFill      = new SolidColorBrush(Color.FromRgb(0xC0, 0x39, 0x2B));
    public static readonly Brush WarningFill     = new SolidColorBrush(Color.FromRgb(0xFF, 0xF3, 0xCD));
    public static readonly Brush WarningBorder   = new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x07));
    public static readonly Brush AvatarFill      = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));
    public static readonly Brush NavbarFill      = new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xEE));
    public static readonly Brush InputBackground = Brushes.White;
    public static readonly Brush SuccessFill     = new SolidColorBrush(Color.FromRgb(0xD4, 0xED, 0xDA));
    public static readonly Brush InfoFill        = new SolidColorBrush(Color.FromRgb(0xCC, 0xE5, 0xFF));

    public static readonly FontFamily BodyFont  = new("Segoe UI");
    public static readonly FontFamily CodeFont  = new("Consolas");

    public const double GapSmall  = 6;
    public const double GapMedium = 8;
    public const double PadCard   = 12;
    public const double PadRow    = 8;
}
