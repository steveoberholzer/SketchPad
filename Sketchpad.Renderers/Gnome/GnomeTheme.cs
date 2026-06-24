using System.Windows.Media;

namespace Sketchpad.Renderers.Gnome;

/// Adwaita light theme — subclass and override properties for variants.
public class GnomeTheme
{
    public Brush PageBg        { get; set; } = new SolidColorBrush(Color.FromRgb(0xF6, 0xF5, 0xF4));
    public Brush CardBg        { get; set; } = Brushes.White;
    public Brush CardBorder    { get; set; } = new SolidColorBrush(Color.FromRgb(0xDE, 0xDD, 0xDA));
    public Brush HeaderbarBg   { get; set; } = new SolidColorBrush(Color.FromRgb(0x2D, 0x2D, 0x2D));
    public Brush HeaderbarText { get; set; } = Brushes.White;
    public Brush Accent        { get; set; } = new SolidColorBrush(Color.FromRgb(0x35, 0x84, 0xE4));
    public Brush AccentText    { get; set; } = Brushes.White;
    public Brush Destructive   { get; set; } = new SolidColorBrush(Color.FromRgb(0xC0, 0x1C, 0x28));
    public Brush ButtonBg      { get; set; } = new SolidColorBrush(Color.FromRgb(0xED, 0xED, 0xED));
    public Brush ButtonBorder  { get; set; } = new SolidColorBrush(Color.FromRgb(0xC8, 0xC7, 0xC7));
    public Brush InputBg       { get; set; } = Brushes.White;
    public Brush InputBorder   { get; set; } = new SolidColorBrush(Color.FromRgb(0xC8, 0xC7, 0xC7));
    public Brush SidebarBg     { get; set; } = new SolidColorBrush(Color.FromRgb(0xED, 0xED, 0xED));
    public Brush DarkText      { get; set; } = new SolidColorBrush(Color.FromRgb(0x2E, 0x2E, 0x2E));
    public Brush MutedText     { get; set; } = new SolidColorBrush(Color.FromRgb(0x77, 0x76, 0x74));
    public Brush Separator     { get; set; } = new SolidColorBrush(Color.FromRgb(0xDE, 0xDD, 0xDA));
    public Brush SuccessBg     { get; set; } = new SolidColorBrush(Color.FromRgb(0xD6, 0xEF, 0xD8));
    public Brush WarningBg     { get; set; } = new SolidColorBrush(Color.FromRgb(0xFE, 0xF3, 0xD2));
    public Brush ErrorBg       { get; set; } = new SolidColorBrush(Color.FromRgb(0xF8, 0xD8, 0xDA));
    public Brush ActiveItemBg  { get; set; } = new SolidColorBrush(Color.FromRgb(0x35, 0x84, 0xE4));

    public FontFamily Font     { get; set; } = new("Cantarell, Ubuntu, Segoe UI");
    public double FontSize     { get; set; } = 11;
    public double HeaderHeight { get; set; } = 46;
    public double CornerRadius { get; set; } = 6;
    public double CardRadius   { get; set; } = 12;
    public double Pad          { get; set; } = 12;
    public double Gap          { get; set; } = 8;
}
