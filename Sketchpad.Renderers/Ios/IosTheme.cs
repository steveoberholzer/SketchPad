using System.Windows.Media;

namespace Sketchpad.Renderers.Ios;

public static class IosTheme
{
    // System colours (UIKit semantic palette)
    public static readonly Brush PageBg        = new SolidColorBrush(Color.FromRgb(0xF2, 0xF2, 0xF7));
    public static readonly Brush CardBg        = Brushes.White;
    public static readonly Brush NavbarBg      = Brushes.White;
    public static readonly Brush NavbarText    = Brushes.Black;
    public static readonly Brush Accent        = new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xFF));
    public static readonly Brush AccentText    = Brushes.White;
    public static readonly Brush Destructive   = new SolidColorBrush(Color.FromRgb(0xFF, 0x3B, 0x30));
    public static readonly Brush Success       = new SolidColorBrush(Color.FromRgb(0x34, 0xC7, 0x59));
    public static readonly Brush Warning       = new SolidColorBrush(Color.FromRgb(0xFF, 0x95, 0x00));
    public static readonly Brush DarkText      = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));
    public static readonly Brush SecondaryText = new SolidColorBrush(Color.FromRgb(0x8E, 0x8E, 0x93));
    public static readonly Brush Separator     = new SolidColorBrush(Color.FromRgb(0xC6, 0xC6, 0xC8));
    public static readonly Brush InputFill     = new SolidColorBrush(Color.FromRgb(0xF2, 0xF2, 0xF7));
    public static readonly Brush SwitchOn      = new SolidColorBrush(Color.FromRgb(0x34, 0xC7, 0x59));
    public static readonly Brush SwitchOff     = new SolidColorBrush(Color.FromRgb(0xE5, 0xE5, 0xEA));
    public static readonly Brush Disclosure    = new SolidColorBrush(Color.FromRgb(0xC7, 0xC7, 0xCC));
    public static readonly Brush SegmentActive = Brushes.White;
    public static readonly Brush SegmentBg    = new SolidColorBrush(Color.FromRgb(0xE5, 0xE5, 0xEA));

    public static readonly FontFamily Font = new("Helvetica Neue, Segoe UI");

    public const double Body        = 17;
    public const double Small       = 13;
    public const double Caption     = 11;
    public const double NavbarHeight = 44;
    public const double StatusHeight = 20;
    public const double CellHeight   = 44;
    public const double CornerRadius = 10;
    public const double CardRadius   = 12;
    public const double Pad          = 16;
    public const double Gap          = 8;
}
