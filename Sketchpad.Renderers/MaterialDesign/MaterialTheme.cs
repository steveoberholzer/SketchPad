using System.Windows.Media;

namespace Sketchpad.Renderers.MaterialDesign;

/// Material Design 3 "Baseline" purple colour scheme.
public static class MaterialTheme
{
    public static readonly Brush Primary            = new SolidColorBrush(Color.FromRgb(0x67, 0x50, 0xA4));
    public static readonly Brush OnPrimary          = System.Windows.Media.Brushes.White;
    public static readonly Brush PrimaryContainer   = new SolidColorBrush(Color.FromRgb(0xEA, 0xDD, 0xFF));
    public static readonly Brush OnPrimaryContainer = new SolidColorBrush(Color.FromRgb(0x21, 0x00, 0x5D));
    public static readonly Brush Secondary          = new SolidColorBrush(Color.FromRgb(0x62, 0x5B, 0x71));
    public static readonly Brush SecondaryContainer = new SolidColorBrush(Color.FromRgb(0xE8, 0xDE, 0xF8));
    public static readonly Brush OnSecondaryContainer = new SolidColorBrush(Color.FromRgb(0x1D, 0x19, 0x2B));
    public static readonly Brush Tertiary           = new SolidColorBrush(Color.FromRgb(0x7D, 0x52, 0x60));
    public static readonly Brush TertiaryContainer  = new SolidColorBrush(Color.FromRgb(0xFF, 0xD8, 0xE4));
    public static readonly Brush Surface            = new SolidColorBrush(Color.FromRgb(0xFF, 0xFB, 0xFE));
    public static readonly Brush SurfaceVariant     = new SolidColorBrush(Color.FromRgb(0xE7, 0xE0, 0xEC));
    public static readonly Brush SurfaceContainer   = new SolidColorBrush(Color.FromRgb(0xF3, 0xED, 0xF7));
    public static readonly Brush SurfaceContainerHigh = new SolidColorBrush(Color.FromRgb(0xEC, 0xE6, 0xF0));
    public static readonly Brush SurfaceContainerLow  = new SolidColorBrush(Color.FromRgb(0xF7, 0xF2, 0xFA));
    public static readonly Brush OnSurface          = new SolidColorBrush(Color.FromRgb(0x1C, 0x1B, 0x1F));
    public static readonly Brush OnSurfaceVariant   = new SolidColorBrush(Color.FromRgb(0x49, 0x45, 0x4F));
    public static readonly Brush Outline            = new SolidColorBrush(Color.FromRgb(0x79, 0x74, 0x7E));
    public static readonly Brush OutlineVariant     = new SolidColorBrush(Color.FromRgb(0xCA, 0xC4, 0xD0));
    public static readonly Brush Error              = new SolidColorBrush(Color.FromRgb(0xB3, 0x26, 0x1E));
    public static readonly Brush ErrorContainer     = new SolidColorBrush(Color.FromRgb(0xF9, 0xDE, 0xDC));
    public static readonly Brush OnErrorContainer   = new SolidColorBrush(Color.FromRgb(0x41, 0x0E, 0x0B));
    public static readonly Brush InverseSurface     = new SolidColorBrush(Color.FromRgb(0x31, 0x30, 0x33));
    public static readonly Brush InverseOnSurface   = new SolidColorBrush(Color.FromRgb(0xF4, 0xEF, 0xFA));
    public static readonly Brush SuccessContainer   = new SolidColorBrush(Color.FromRgb(0xC8, 0xE6, 0xC9));
    public static readonly Brush WarningContainer   = new SolidColorBrush(Color.FromRgb(0xFF, 0xEC, 0xB3));

    public static readonly FontFamily Font = new("Roboto, Segoe UI");
    public const double TitleLarge   = 22;
    public const double TitleMedium  = 16;
    public const double BodyLarge    = 16;
    public const double BodyMedium   = 14;
    public const double LabelLarge   = 14;
    public const double LabelMedium  = 12;
    public const double LabelSmall   = 11;

    public const double AppBarHeight  = 64;
    public const double Pad           = 16;
    public const double Gap           = 8;
    public const double CardRadius    = 12;
    public const double ButtonRadius  = 20;   // stadium / "full" shape
    public const double InputRadius   = 4;
    public const double ChipRadius    = 8;
    public const double NavPillRadius = 28;   // navigation drawer pill
}
