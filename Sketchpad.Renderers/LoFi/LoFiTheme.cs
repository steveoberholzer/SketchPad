using System.Windows.Media;

namespace Sketchpad.Renderers.LoFi;

public static class LoFiTheme
{
    // Palette — greyscale only
    public static readonly Brush Page        = new SolidColorBrush(Color.FromRgb(0xFF, 0xFD, 0xF0)); // warm off-white
    public static readonly Brush ElementFill = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
    public static readonly Brush LightFill   = new SolidColorBrush(Color.FromRgb(0xF2, 0xF2, 0xF2));
    public static readonly Brush MidFill     = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));
    public static readonly Brush DarkFill    = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
    public static readonly Brush Stroke      = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
    public static readonly Brush DarkText    = new SolidColorBrush(Color.FromRgb(0x22, 0x22, 0x22));
    public static readonly Brush MutedText   = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
    public static readonly Brush White       = Brushes.White;

    // Placeholder line colours for body-text blocks
    public static readonly Brush LineFull  = new SolidColorBrush(Color.FromRgb(0xBB, 0xBB, 0xBB));
    public static readonly Brush LineShort = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));

    // Font — handwritten style
    public static readonly FontFamily Font = new("Segoe Print, Comic Sans MS, Segoe UI");

    public const double StrokeWeight = 1.5;
    public const double Gap          = 8;
    public const double Pad          = 12;
    public const double PadSmall     = 8;
}
