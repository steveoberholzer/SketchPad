using System.Windows;
using System.Windows.Media;

namespace Sketchpad.Renderers.WinXp;

public static class WinXpTheme
{
    // Luna warm-silver client area
    public static readonly Brush PageBg      = new SolidColorBrush(Color.FromRgb(0xEC, 0xE9, 0xD8));
    public static readonly Brush ButtonFace  = new SolidColorBrush(Color.FromRgb(0xEC, 0xE9, 0xD8));
    public static readonly Brush Highlight   = Brushes.White;
    public static readonly Brush Shadow      = new SolidColorBrush(Color.FromRgb(0xAC, 0xA8, 0x99));
    public static readonly Brush DarkShadow  = new SolidColorBrush(Color.FromRgb(0x71, 0x6F, 0x64));
    public static readonly Brush FrameColor  = new SolidColorBrush(Color.FromRgb(0x0A, 0x24, 0x6A));
    public static readonly Brush Accent      = new SolidColorBrush(Color.FromRgb(0x31, 0x6A, 0xC5));
    public static readonly Brush AccentText  = Brushes.White;
    public static readonly Brush DarkText    = Brushes.Black;
    public static readonly Brush MutedText   = new SolidColorBrush(Color.FromRgb(0x71, 0x6F, 0x64));
    public static readonly Brush InputBg     = Brushes.White;
    public static readonly Brush InputBorder = new SolidColorBrush(Color.FromRgb(0x7F, 0x9D, 0xB9));
    public static readonly Brush Destructive = new SolidColorBrush(Color.FromRgb(0xC4, 0x21, 0x26));
    public static readonly Brush SuccessBg   = new SolidColorBrush(Color.FromRgb(0xCC, 0xFF, 0xCC));
    public static readonly Brush WarningBg   = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xCC));
    public static readonly Brush ErrorBg     = new SolidColorBrush(Color.FromRgb(0xFF, 0xCC, 0xCC));
    public static readonly Brush GroupBorder = new SolidColorBrush(Color.FromRgb(0xAC, 0xA8, 0x99));

    // The iconic Luna blue horizontal gradient
    public static readonly LinearGradientBrush TitleBar = MakeTitleBar();

    public static readonly FontFamily Font = new("Tahoma, Segoe UI");
    public const double FontSize    = 11;
    public const double TitleHeight = 30;

    private static LinearGradientBrush MakeTitleBar()
    {
        var b = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(1, 0) };
        b.GradientStops.Add(new GradientStop(Color.FromRgb(0x0A, 0x24, 0x6A), 0.00));
        b.GradientStops.Add(new GradientStop(Color.FromRgb(0x16, 0x5D, 0xC2), 0.15));
        b.GradientStops.Add(new GradientStop(Color.FromRgb(0x36, 0x8D, 0xD4), 0.50));
        b.GradientStops.Add(new GradientStop(Color.FromRgb(0x16, 0x5D, 0xC2), 0.85));
        b.GradientStops.Add(new GradientStop(Color.FromRgb(0x0A, 0x24, 0x6A), 1.00));
        return b;
    }
}
