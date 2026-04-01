using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sketchpad.App;

/// <summary>
/// Generates the application icon programmatically as a WPF BitmapSource.
/// Design: dark charcoal rounded square with a miniature wireframe UI inside —
/// a light title bar over three placeholder content lines.
/// Used for the window chrome, splash screen, and About dialog.
/// </summary>
public static class AppIcon
{
    public static BitmapSource Generate(int size = 32)
    {
        var dv = new DrawingVisual();
        using (var dc = dv.RenderOpen())
        {
            double s = size;

            // ── Background ────────────────────────────────────────────────────
            dc.DrawRoundedRectangle(
                new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33)),
                null,
                new Rect(0, 0, s, s),
                s * 0.156, s * 0.156);

            // ── Screen area ───────────────────────────────────────────────────
            double pad = s * 0.09;
            double sw  = s - pad * 2;
            double sh  = s - pad * 2;
            double tbH = s * 0.19;   // title bar height

            // Title bar fill
            dc.DrawRectangle(
                new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC)),
                null,
                new Rect(pad, pad, sw, tbH));

            // Screen border (white outline)
            dc.DrawRoundedRectangle(
                null,
                new Pen(new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD)), s * 0.05),
                new Rect(pad, pad, sw, sh),
                s * 0.06, s * 0.06);

            // ── Content placeholder lines ─────────────────────────────────────
            var linePen = new Pen(new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88)), s * 0.05);
            double lx  = pad + s * 0.07;
            double lw  = sw  - s * 0.14;
            double ly0 = pad + tbH + s * 0.13;
            double gap = s * 0.135;

            dc.DrawLine(linePen, new Point(lx, ly0),          new Point(lx + lw * 0.72, ly0));
            dc.DrawLine(linePen, new Point(lx, ly0 + gap),    new Point(lx + lw,         ly0 + gap));
            dc.DrawLine(linePen, new Point(lx, ly0 + gap * 2),new Point(lx + lw * 0.55,  ly0 + gap * 2));
        }

        var rtb = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
        rtb.Render(dv);
        rtb.Freeze();
        return rtb;
    }
}
