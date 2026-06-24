using System.Windows.Media;

namespace Sketchpad.Renderers.Win95;

public static class Win95Theme
{
    public static readonly Brush ButtonFace      = new SolidColorBrush(Color.FromRgb(0xC0, 0xC0, 0xC0));
    public static readonly Brush ButtonLight     = new SolidColorBrush(Color.FromRgb(0xDF, 0xDF, 0xDF));
    public static readonly Brush Highlight       = Brushes.White;
    public static readonly Brush Shadow          = new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80));
    public static readonly Brush DarkShadow      = Brushes.Black;
    public static readonly Brush InputFill       = Brushes.White;
    public static readonly Brush TitleBar        = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x80));
    public static readonly Brush TitleBarText    = Brushes.White;
    public static readonly Brush DarkText        = Brushes.Black;
    public static readonly Brush GrayText        = new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80));
    public static readonly Brush SelectFill      = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x80));
    public static readonly Brush SelectText      = Brushes.White;
    public static readonly Brush ProgressFill    = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x80));

    public static readonly FontFamily Font = new("Microsoft Sans Serif, Tahoma, Segoe UI");

    public const double FontSize      = 11;
    public const double TitleHeight   = 22;
    public const double ControlHeight = 22;
    public const double Pad           = 8;
}
