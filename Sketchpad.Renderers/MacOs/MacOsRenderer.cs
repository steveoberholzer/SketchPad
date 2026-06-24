using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using Sketchpad.Core.Ast;
using Sketchpad.Core.Rendering;

namespace Sketchpad.Renderers.MacOs;

/// <summary>
/// macOS / Aqua renderer (Big Sur / Ventura style).
/// Traffic lights on the left, unified gray title bar, white card surfaces,
/// system blue accent (#007AFF), green UISwitch, and a window drop shadow.
/// </summary>
public class MacOsRenderer : IUiRenderer<UIElement>
{
    public string DisplayName => "macOS";

    public UIElement Render(UiDocument document)
    {
        var root = new StackPanel
        {
            Orientation         = Orientation.Vertical,
            Background          = MacOsTheme.ContentBg,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        foreach (var node in document.Roots)
            root.Children.Add(RenderNode(node));

        if (document.HasErrors)
        {
            var err = new StackPanel { Margin = new Thickness(MacOsTheme.Pad) };
            foreach (var e in document.Errors)
                err.Children.Add(MakeText($"Line {e.Line}: {e.Message}", MacOsTheme.Destructive));
            root.Children.Insert(0, err);
        }

        return root;
    }

    // ── Dispatch ─────────────────────────────────────────────────────────────

    private UIElement RenderNode(UiNode node)
    {
        try
        {
            return node.Type switch
            {
                ElementType.Window   => RenderWindow(node),
                ElementType.Panel    => RenderPanel(node),
                ElementType.Card     => RenderCard(node),
                ElementType.Row      => RenderRow(node),
                ElementType.Col      => RenderChildren(node, Orientation.Vertical),
                ElementType.Divider  => RenderDivider(),
                ElementType.Spacer   => new Border { Height = MacOsTheme.Gap },

                ElementType.Navbar   => RenderNavbar(node),
                ElementType.Sidebar  => RenderSidebar(node),
                ElementType.Menu     => RenderMenu(node),
                ElementType.Nav      => RenderChildren(node, Orientation.Vertical, 2),
                ElementType.Item     => RenderItem(node),
                ElementType.Tabs     => RenderTabs(node),
                ElementType.Tab      => RenderTab(node),
                ElementType.Brand    => RenderBrand(node),

                ElementType.Field    => RenderField(node),
                ElementType.Textarea => RenderTextarea(node),
                ElementType.Checkbox => RenderCheckbox(node),
                ElementType.Radio    => RenderRadio(node),
                ElementType.Select   => RenderSelect(node),
                ElementType.Toggle   => RenderToggle(node),
                ElementType.Slider   => RenderSlider(node),
                ElementType.Button   => RenderButton(node),

                ElementType.Label    => MakeText(node.Label ?? "", MacOsTheme.SecText, MacOsTheme.FontSize - 2),
                ElementType.Text     => MakeText(node.Label ?? ""),
                ElementType.Heading  => MakeText(node.Label ?? "", size: 20, weight: FontWeights.SemiBold),
                ElementType.Avatar   => RenderAvatar(node),
                ElementType.Image    => RenderImage(node),
                ElementType.Badge    => RenderBadge(node),
                ElementType.Tag      => RenderTag(node),
                ElementType.Table    => RenderTable(node),
                ElementType.Icon     => RenderIcon(node),

                ElementType.Alert    => RenderAlert(node),
                ElementType.Toast    => RenderToast(node),
                ElementType.Spinner  => RenderSpinner(),
                ElementType.Progress => RenderProgress(node),

                ElementType.DatePicker     => RenderDatePicker(node),
                ElementType.DateTimePicker => RenderDateTimePicker(node),
                ElementType.Calendar       => RenderCalendar(node),

                _ => RenderPlaceholder(node),
            };
        }
        catch { return RenderPlaceholder(node); }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static TextBlock MakeText(string text, Brush? fg = null, double size = MacOsTheme.FontSize,
        FontWeight? weight = null) => new()
    {
        Text         = text,
        FontFamily   = MacOsTheme.Font,
        FontSize     = size,
        Foreground   = fg ?? MacOsTheme.DarkText,
        FontWeight   = weight ?? FontWeights.Normal,
        TextWrapping = TextWrapping.Wrap,
    };

    private static Border InputBox(UIElement content, double? height = null) => new()
    {
        Background      = MacOsTheme.InputBg,
        BorderBrush     = MacOsTheme.InputBorder,
        BorderThickness = new Thickness(1),
        CornerRadius    = new CornerRadius(MacOsTheme.CornerRadius / 1.5),
        Padding         = new Thickness(8, 5, 8, 5),
        Height          = height ?? double.NaN,
        Margin          = new Thickness(0, 3, 0, 0),
        Child           = content,
    };

    private StackPanel RenderChildren(UiNode node, Orientation orientation, double gap = -1)
    {
        if (gap < 0) gap = MacOsTheme.Gap;
        var panel = new StackPanel { Orientation = orientation };
        bool first = true;
        foreach (var child in node.Children)
        {
            if (!first && gap > 0)
                panel.Children.Add(new Border
                {
                    Width  = orientation == Orientation.Horizontal ? gap : double.NaN,
                    Height = orientation == Orientation.Vertical   ? gap : double.NaN,
                });
            first = false;
            panel.Children.Add(RenderNode(child));
        }
        return panel;
    }

    private static (int w, int h) ParseWxH(string m)
    {
        var p = m.Split('x');
        return p.Length == 2 && int.TryParse(p[0], out var w) && int.TryParse(p[1], out var h)
            ? (w, h) : (0, 0);
    }

    private static int ParsePx(string m) =>
        int.TryParse(m.Replace("px", "").Trim(), out var v) ? v : 0;

    // ── Layout ───────────────────────────────────────────────────────────────

    private UIElement RenderWindow(UiNode node)
    {
        double width = 800;
        foreach (var m in node.Modifiers) { var (w, _) = ParseWxH(m); if (w > 0) width = w; }

        var stack = new StackPanel { Orientation = Orientation.Vertical };

        // Unified title bar with traffic lights on the LEFT
        var tbGrid = new Grid();
        tbGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // traffic lights
        tbGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // title
        tbGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // right controls

        var lights = new StackPanel
        {
            Orientation       = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin            = new Thickness(8, 0, 0, 0),
        };
        lights.Children.Add(TrafficLight(MacOsTheme.TrafficRed));
        lights.Children.Add(TrafficLight(MacOsTheme.TrafficYellow));
        lights.Children.Add(TrafficLight(MacOsTheme.TrafficGreen));

        var title = new TextBlock
        {
            Text                = node.Label ?? "Window",
            FontFamily          = MacOsTheme.Font,
            FontSize            = MacOsTheme.FontSize,
            FontWeight          = FontWeights.SemiBold,
            Foreground          = MacOsTheme.DarkText,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment   = VerticalAlignment.Center,
        };

        Grid.SetColumn(lights, 0); Grid.SetColumn(title, 1);
        tbGrid.Children.Add(lights); tbGrid.Children.Add(title);

        var titleBar = new Border
        {
            Background      = MacOsTheme.TitlebarBg,
            Height          = MacOsTheme.TitleHeight,
            BorderBrush     = MacOsTheme.Separator,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Child           = tbGrid,
        };

        stack.Children.Add(titleBar);
        stack.Children.Add(new Border
        {
            Background = MacOsTheme.ContentBg,
            Padding    = new Thickness(MacOsTheme.Pad),
            Child      = RenderChildren(node, Orientation.Vertical),
        });

        return new Border
        {
            Child               = stack,
            Width               = width,
            Background          = MacOsTheme.WindowBg,
            CornerRadius        = new CornerRadius(MacOsTheme.WinRadius),
            ClipToBounds        = true,
            BorderBrush         = MacOsTheme.CardBorder,
            BorderThickness     = new Thickness(1),
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin              = new Thickness(12),
            Effect              = new DropShadowEffect
            {
                BlurRadius   = 24,
                ShadowDepth  = 4,
                Opacity      = 0.18,
                Color        = Colors.Black,
                Direction    = 270,
            },
        };
    }

    private static Ellipse TrafficLight(Brush fill) => new()
    {
        Width             = 13,
        Height            = 13,
        Fill              = fill,
        Margin            = new Thickness(4, 0, 4, 0),
        VerticalAlignment = VerticalAlignment.Center,
    };

    private UIElement RenderPanel(UiNode node)
    {
        var content = RenderChildren(node, Orientation.Vertical);
        content.Margin = new Thickness(MacOsTheme.Pad);

        if (node.Label == null) return content;

        var stack = new StackPanel();
        stack.Children.Add(new Border
        {
            Background  = MacOsTheme.TitlebarBg,
            Padding     = new Thickness(MacOsTheme.Pad, 6, MacOsTheme.Pad, 6),
            BorderBrush = MacOsTheme.Separator, BorderThickness = new Thickness(0, 0, 0, 1),
            Child       = MakeText(node.Label, MacOsTheme.DarkText, weight: FontWeights.SemiBold),
        });
        stack.Children.Add(content);

        return new Border
        {
            Child           = stack,
            Background      = MacOsTheme.CardBg,
            BorderBrush     = MacOsTheme.CardBorder,
            BorderThickness = new Thickness(1),
            CornerRadius    = new CornerRadius(MacOsTheme.CornerRadius),
            ClipToBounds    = true,
            Margin          = new Thickness(4),
        };
    }

    private UIElement RenderCard(UiNode node)
    {
        var inner = new StackPanel();
        if (node.Label != null)
            inner.Children.Add(new Border
            {
                Padding         = new Thickness(0, 0, 0, MacOsTheme.Gap),
                BorderBrush     = MacOsTheme.Separator,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Margin          = new Thickness(0, 0, 0, MacOsTheme.Gap),
                Child           = MakeText(node.Label, weight: FontWeights.SemiBold),
            });
        foreach (var child in node.Children)
            inner.Children.Add(new Border { Margin = new Thickness(0, 2, 0, 2), Child = RenderNode(child) });

        return new Border
        {
            Background      = MacOsTheme.CardBg,
            BorderBrush     = MacOsTheme.CardBorder,
            BorderThickness = new Thickness(1),
            CornerRadius    = new CornerRadius(MacOsTheme.CornerRadius),
            Padding         = new Thickness(MacOsTheme.Pad),
            Margin          = new Thickness(4),
            Child           = inner,
        };
    }

    private UIElement RenderRow(UiNode node)
    {
        if (node.HasModifier("vertical")) return RenderChildren(node, Orientation.Vertical);
        var grid = new UniformGrid { Rows = 1, Columns = node.Children.Count };
        foreach (var child in node.Children)
            grid.Children.Add(new Border
            {
                Padding = new Thickness(MacOsTheme.Gap / 2, 0, MacOsTheme.Gap / 2, 0),
                Child   = RenderNode(child),
            });
        return grid;
    }

    // ── Navigation ───────────────────────────────────────────────────────────

    private UIElement RenderNavbar(UiNode node)
    {
        var left  = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        var right = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        TextBlock? centreTitle = null;

        foreach (var child in node.Children)
        {
            if (child.Type == ElementType.Brand)
            {
                centreTitle = MakeText(child.Label ?? "", weight: FontWeights.SemiBold);
                centreTitle.HorizontalAlignment = HorizontalAlignment.Center;
                centreTitle.VerticalAlignment   = VerticalAlignment.Center;
            }
            else if (child.Type == ElementType.Menu && child.HasModifier("right"))
            {
                foreach (var sub in child.Children)
                    right.Children.Add(new Border
                    {
                        Padding = new Thickness(8, 0, 0, 0),
                        Child   = MakeText(sub.Label ?? "", MacOsTheme.Accent),
                    });
            }
            else
            {
                left.Children.Add(RenderNode(child));
            }
        }

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        Grid.SetColumn(left, 0);
        if (centreTitle != null) { Grid.SetColumn(centreTitle, 1); grid.Children.Add(centreTitle); }
        Grid.SetColumn(right, 2);
        grid.Children.Add(left); grid.Children.Add(right);

        return new Border
        {
            Background      = MacOsTheme.TitlebarBg,
            Height          = MacOsTheme.TitleHeight,
            BorderBrush     = MacOsTheme.Separator,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding         = new Thickness(MacOsTheme.Pad, 0, MacOsTheme.Pad, 0),
            Child           = grid,
        };
    }

    private UIElement RenderSidebar(UiNode node)
    {
        double width = 200;
        foreach (var m in node.Modifiers) { var px = ParsePx(m); if (px > 0) width = px; }

        var inner = RenderChildren(node, Orientation.Vertical, 2);
        inner.Margin = new Thickness(4);

        return new Border
        {
            Width           = width,
            Background      = MacOsTheme.SidebarBg,
            BorderBrush     = MacOsTheme.Separator,
            BorderThickness = new Thickness(0, 0, 1, 0),
            Child           = inner,
        };
    }

    private UIElement RenderMenu(UiNode node)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal };
        foreach (var child in node.Children)
            panel.Children.Add(new Border
            {
                Padding = new Thickness(0, 0, MacOsTheme.Pad, 0),
                Child   = MakeText(child.Label ?? "", MacOsTheme.Accent),
            });
        return panel;
    }

    private UIElement RenderItem(UiNode node)
    {
        bool active = node.HasModifier("active");
        return new Border
        {
            Background   = active ? MacOsTheme.Accent : Brushes.Transparent,
            CornerRadius = new CornerRadius(MacOsTheme.CornerRadius / 2),
            Padding      = new Thickness(8, 5, 8, 5),
            Margin       = new Thickness(4, 2, 4, 2),
            Child        = MakeText(node.Label ?? "", fg: active ? MacOsTheme.AccentText : MacOsTheme.DarkText),
        };
    }

    private UIElement RenderTabs(UiNode node)
    {
        // macOS segmented control style
        var strip = new StackPanel { Orientation = Orientation.Horizontal };
        foreach (var child in node.Children) strip.Children.Add(RenderNode(child));
        return new Border
        {
            Background   = MacOsTheme.SidebarBg,
            CornerRadius = new CornerRadius(MacOsTheme.Gap),
            Padding      = new Thickness(2),
            Margin       = new Thickness(0, MacOsTheme.Gap, 0, MacOsTheme.Gap),
            Child        = strip,
        };
    }

    private UIElement RenderTab(UiNode node)
    {
        bool active = node.HasModifier("active");
        return new Border
        {
            Background   = active ? MacOsTheme.WindowBg : Brushes.Transparent,
            CornerRadius = new CornerRadius(MacOsTheme.Gap - 1),
            Padding      = new Thickness(12, 4, 12, 4),
            Margin       = new Thickness(1),
            Child        = MakeText(node.Label ?? "", size: MacOsTheme.FontSize - 1,
                weight: active ? FontWeights.SemiBold : FontWeights.Normal),
        };
    }

    private UIElement RenderBrand(UiNode node) => new Border
    {
        Padding = new Thickness(0, 0, MacOsTheme.Pad, 0),
        Child   = MakeText(node.Label ?? "App", weight: FontWeights.SemiBold),
    };

    // ── Form ─────────────────────────────────────────────────────────────────

    private UIElement RenderField(UiNode node)
    {
        var stack = new StackPanel();
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, MacOsTheme.DarkText, MacOsTheme.FontSize - 1, FontWeights.Medium));
        stack.Children.Add(InputBox(MakeText(node.Value ?? "", MacOsTheme.SecText)));
        return stack;
    }

    private UIElement RenderTextarea(UiNode node)
    {
        var stack = new StackPanel();
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, MacOsTheme.DarkText, MacOsTheme.FontSize - 1, FontWeights.Medium));
        stack.Children.Add(InputBox(new Border(), height: 72));
        return stack;
    }

    private UIElement RenderCheckbox(UiNode node)
    {
        bool chk = node.HasModifier("checked");
        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(new Border
        {
            Width = 16, Height = 16,
            Background = chk ? MacOsTheme.Accent : MacOsTheme.InputBg,
            BorderBrush = chk ? Brushes.Transparent : MacOsTheme.InputBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Margin = new Thickness(0, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center,
            Child = chk ? new TextBlock
            {
                Text = "✓", FontSize = 10, Foreground = MacOsTheme.AccentText,
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
            } : null,
        });
        row.Children.Add(MakeText(node.Label ?? ""));
        return row;
    }

    private UIElement RenderRadio(UiNode node)
    {
        bool chk = node.HasModifier("checked");
        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(new Ellipse
        {
            Width = 16, Height = 16,
            Stroke = chk ? MacOsTheme.Accent : MacOsTheme.InputBorder, StrokeThickness = 1.5,
            Fill   = chk ? MacOsTheme.Accent : MacOsTheme.InputBg,
            Margin = new Thickness(0, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center,
        });
        row.Children.Add(MakeText(node.Label ?? ""));
        return row;
    }

    private UIElement RenderSelect(UiNode node)
    {
        var stack = new StackPanel();
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, MacOsTheme.DarkText, MacOsTheme.FontSize - 1, FontWeights.Medium));
        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var val  = MakeText(node.Value ?? "", MacOsTheme.DarkText);
        var chev = MakeText("⌃⌄", MacOsTheme.SecText, 9);
        Grid.SetColumn(val, 0); Grid.SetColumn(chev, 1);
        row.Children.Add(val); row.Children.Add(chev);
        stack.Children.Add(InputBox(row));
        return stack;
    }

    private UIElement RenderToggle(UiNode node)
    {
        bool on = node.HasModifier("on") || node.HasModifier("checked");
        var row = new StackPanel { Orientation = Orientation.Horizontal };
        var knob = new Ellipse
        {
            Width = 20, Height = 20, Fill = Brushes.White,
            HorizontalAlignment = on ? HorizontalAlignment.Right : HorizontalAlignment.Left,
            VerticalAlignment   = VerticalAlignment.Center,
            Margin              = new Thickness(1),
            Effect              = new DropShadowEffect { BlurRadius = 4, ShadowDepth = 1, Opacity = 0.25, Color = Colors.Black },
        };
        var track = new Border
        {
            Width = 38, Height = 22, CornerRadius = new CornerRadius(11),
            Background = on ? MacOsTheme.SwitchOn : MacOsTheme.SwitchOff,
            Margin = new Thickness(0, 0, 8, 0), VerticalAlignment = VerticalAlignment.Center,
            Child = knob,
        };
        row.Children.Add(track);
        row.Children.Add(MakeText(node.Label ?? ""));
        return row;
    }

    private UIElement RenderSlider(UiNode node)
    {
        var stack = new StackPanel();
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, MacOsTheme.DarkText, MacOsTheme.FontSize - 1, FontWeights.Medium));

        var track = new Grid { Height = 20, Margin = new Thickness(0, 4, 0, 0) };
        track.Children.Add(new Border
        {
            Height = 4, Background = MacOsTheme.InputBorder,
            CornerRadius = new CornerRadius(2), VerticalAlignment = VerticalAlignment.Center,
        });
        track.Children.Add(new Border
        {
            Height = 4, Width = 100, Background = MacOsTheme.Accent,
            CornerRadius = new CornerRadius(2), VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
        });
        track.Children.Add(new Ellipse
        {
            Width = 18, Height = 18, Fill = Brushes.White,
            Stroke = MacOsTheme.InputBorder, StrokeThickness = 0.5,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(90, 0, 0, 0),
            Effect = new DropShadowEffect { BlurRadius = 4, ShadowDepth = 1, Opacity = 0.2, Color = Colors.Black },
        });
        stack.Children.Add(track);
        return stack;
    }

    private UIElement RenderButton(UiNode node)
    {
        bool primary  = node.HasModifier("primary");
        bool danger   = node.HasModifier("danger");
        bool disabled = node.HasModifier("disabled");

        Brush bg  = danger ? MacOsTheme.Destructive : primary ? MacOsTheme.Accent : MacOsTheme.ButtonBg;
        Brush fg  = (primary || danger) ? MacOsTheme.AccentText : MacOsTheme.DarkText;
        Brush bdr = (primary || danger) ? Brushes.Transparent : MacOsTheme.ButtonBorder;
        if (disabled) { bg = MacOsTheme.CardBorder; fg = MacOsTheme.SecText; bdr = Brushes.Transparent; }

        return new Border
        {
            Background      = bg,
            BorderBrush     = bdr,
            BorderThickness = new Thickness(1),
            CornerRadius    = new CornerRadius(MacOsTheme.CornerRadius / 1.5),
            Padding         = new Thickness(14, 6, 14, 6),
            Margin          = new Thickness(0, 0, MacOsTheme.Gap, 0),
            Child           = new TextBlock
            {
                Text                = node.Label ?? "Button",
                FontFamily          = MacOsTheme.Font,
                FontSize            = MacOsTheme.FontSize,
                FontWeight          = FontWeights.Medium,
                Foreground          = fg,
                HorizontalAlignment = HorizontalAlignment.Center,
            },
        };
    }

    // ── Display ──────────────────────────────────────────────────────────────

    private UIElement RenderAvatar(UiNode node)
    {
        bool circle = node.HasModifier("circle");
        int size = 36;
        return circle
            ? (UIElement)new Ellipse { Width = size, Height = size, Fill = MacOsTheme.Accent }
            : new Rectangle
            {
                Width = size, Height = size, Fill = MacOsTheme.Accent,
                RadiusX = MacOsTheme.CornerRadius / 2, RadiusY = MacOsTheme.CornerRadius / 2,
            };
    }

    private UIElement RenderImage(UiNode node)
    {
        double w = 200, h = 150;
        foreach (var m in node.Modifiers) { var (mw, mh) = ParseWxH(m); if (mw > 0) { w = mw; h = mh; } }
        var canvas = new Canvas { Width = w, Height = h };
        canvas.Children.Add(new Rectangle
        {
            Width = w, Height = h, Fill = MacOsTheme.SidebarBg,
            RadiusX = MacOsTheme.CornerRadius, RadiusY = MacOsTheme.CornerRadius,
        });
        canvas.Children.Add(new Line { X1 = 0, Y1 = 0, X2 = w, Y2 = h, Stroke = MacOsTheme.CardBorder, StrokeThickness = 1 });
        canvas.Children.Add(new Line { X1 = w, Y1 = 0, X2 = 0, Y2 = h, Stroke = MacOsTheme.CardBorder, StrokeThickness = 1 });
        return canvas;
    }

    private UIElement RenderBadge(UiNode node) => new Border
    {
        Background   = MacOsTheme.Accent,
        CornerRadius = new CornerRadius(10),
        Padding      = new Thickness(7, 2, 7, 2),
        Margin       = new Thickness(0, 0, 4, 0),
        Child        = MakeText(node.Label ?? "", MacOsTheme.AccentText, MacOsTheme.FontSize - 2),
    };

    private UIElement RenderTag(UiNode node) => new Border
    {
        Background      = MacOsTheme.SidebarBg,
        BorderBrush     = MacOsTheme.CardBorder,
        BorderThickness = new Thickness(1),
        CornerRadius    = new CornerRadius(MacOsTheme.CornerRadius / 2),
        Padding         = new Thickness(8, 3, 8, 3),
        Margin          = new Thickness(0, 0, 4, 0),
        Child           = MakeText(node.Label ?? "", size: MacOsTheme.FontSize - 1),
    };

    private UIElement RenderTable(UiNode node)
    {
        var grid = new Grid();
        var colsNode = node.Children.FirstOrDefault(c => c.Type == ElementType.Columns);
        var rows     = node.Children.Where(c => c.Type == ElementType.Row).ToList();
        var headers  = colsNode?.Label?.Split('|').Select(s => s.Trim()).ToArray() ?? [];
        int colCount = Math.Max(headers.Length, rows.Count > 0 ? rows.Max(r => r.Label?.Split('|').Length ?? 0) : 0);

        for (int i = 0; i < colCount; i++)
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        int rowIdx = 0;
        if (headers.Length > 0)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (int c = 0; c < headers.Length; c++)
            {
                var cell = new Border
                {
                    Background = MacOsTheme.SidebarBg, Padding = new Thickness(MacOsTheme.Pad, 6, MacOsTheme.Pad, 6),
                    BorderBrush = MacOsTheme.Separator, BorderThickness = new Thickness(0, 0, 0, 1),
                    Child = MakeText(headers[c], MacOsTheme.SecText, weight: FontWeights.SemiBold),
                };
                Grid.SetRow(cell, 0); Grid.SetColumn(cell, c); grid.Children.Add(cell);
            }
            rowIdx++;
        }

        bool alt = false;
        foreach (var row in rows)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var cells = row.Label?.Split('|').Select(s => s.Trim()).ToArray() ?? [];
            for (int c = 0; c < Math.Min(cells.Length, colCount); c++)
            {
                var cell = new Border
                {
                    Background = alt ? MacOsTheme.AltRow : MacOsTheme.CardBg,
                    Padding = new Thickness(MacOsTheme.Pad, 8, MacOsTheme.Pad, 8),
                    BorderBrush = MacOsTheme.Separator, BorderThickness = new Thickness(0, 0, 0, 1),
                    Child = MakeText(cells[c]),
                };
                Grid.SetRow(cell, rowIdx); Grid.SetColumn(cell, c); grid.Children.Add(cell);
            }
            rowIdx++; alt = !alt;
        }

        return new Border
        {
            BorderBrush = MacOsTheme.CardBorder, BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(MacOsTheme.CornerRadius), ClipToBounds = true,
            Margin = new Thickness(0, 4, 0, 4), Child = grid,
        };
    }

    private UIElement RenderIcon(UiNode node) => new Border
    {
        Width = 28, Height = 28, Background = MacOsTheme.Accent,
        CornerRadius = new CornerRadius(7), Margin = new Thickness(0, 0, 4, 0),
        Child = MakeText(node.Label?[..1].ToUpper() ?? "?", MacOsTheme.AccentText, 11),
        HorizontalAlignment = HorizontalAlignment.Left,
    };

    // ── Feedback ─────────────────────────────────────────────────────────────

    private static UIElement RenderDivider() => new Border
    {
        Height = 1, Background = MacOsTheme.Separator,
        Margin = new Thickness(0, 4, 0, 4), HorizontalAlignment = HorizontalAlignment.Stretch,
    };

    private UIElement RenderAlert(UiNode node)
    {
        bool warning = node.HasModifier("warning");
        bool danger  = node.HasModifier("danger") || node.HasModifier("error");
        bool success = node.HasModifier("success");

        Brush bg = warning ? MacOsTheme.WarningBg
                 : danger  ? MacOsTheme.ErrorBg
                 : success ? MacOsTheme.SuccessBg
                 : new SolidColorBrush(Color.FromRgb(0xCC, 0xE5, 0xFF));

        return new Border
        {
            Background = bg, BorderBrush = MacOsTheme.CardBorder, BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(MacOsTheme.CornerRadius / 1.5),
            Padding = new Thickness(MacOsTheme.Pad, 8, MacOsTheme.Pad, 8), Margin = new Thickness(0, 4, 0, 4),
            Child = MakeText(node.Label ?? ""),
        };
    }

    private UIElement RenderToast(UiNode node) => new Border
    {
        Background          = new SolidColorBrush(Color.FromArgb(0xEE, 0x1C, 0x1C, 0x1E)),
        CornerRadius        = new CornerRadius(MacOsTheme.CornerRadius),
        Padding             = new Thickness(MacOsTheme.Pad, MacOsTheme.Gap, MacOsTheme.Pad, MacOsTheme.Gap),
        Margin              = new Thickness(MacOsTheme.Pad * 2, 4, MacOsTheme.Pad * 2, 4),
        HorizontalAlignment = HorizontalAlignment.Center,
        Child               = MakeText(node.Label ?? "", Brushes.White, MacOsTheme.FontSize - 1),
    };

    private static UIElement RenderSpinner() => new Ellipse
    {
        Width = 22, Height = 22, Stroke = MacOsTheme.SecText, StrokeThickness = 2.5,
        StrokeDashArray = new DoubleCollection([5, 3]), Margin = new Thickness(4),
    };

    private UIElement RenderProgress(UiNode node)
    {
        int pct = 0;
        foreach (var m in node.Modifiers) if (int.TryParse(m, out var v)) pct = v;
        pct = Math.Clamp(pct, 0, 100);

        var track = new Grid { Height = 6 };
        track.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(pct, GridUnitType.Star) });
        track.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100 - pct, GridUnitType.Star) });
        var fill = new Rectangle { Fill = MacOsTheme.Accent, RadiusX = 3, RadiusY = 3 };
        Grid.SetColumn(fill, 0);
        track.Children.Add(fill);

        return new Border
        {
            Background = MacOsTheme.CardBorder, CornerRadius = new CornerRadius(3),
            Child = track, Margin = new Thickness(0, 4, 0, 4),
        };
    }

    // ── Date / Time ───────────────────────────────────────────────────────────

    private UIElement RenderDatePicker(UiNode node)
    {
        var stack = new StackPanel();
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, MacOsTheme.DarkText, MacOsTheme.FontSize - 1, FontWeights.Medium));

        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var dateTxt = MakeText(node.Value ?? "MM / DD / YYYY", MacOsTheme.SecText);
        var calIco  = MakeText("▦", MacOsTheme.SecText);
        Grid.SetColumn(dateTxt, 0); Grid.SetColumn(calIco, 1);
        row.Children.Add(dateTxt); row.Children.Add(calIco);
        stack.Children.Add(InputBox(row));
        return stack;
    }

    private UIElement RenderDateTimePicker(UiNode node)
    {
        var stack = new StackPanel();
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, MacOsTheme.DarkText, MacOsTheme.FontSize - 1, FontWeights.Medium));
        var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 3, 0, 0) };
        row.Children.Add(new Border { MinWidth = 140, Child = InputBox(MakeText("MM / DD / YYYY", MacOsTheme.SecText)) });
        row.Children.Add(new Border { Width = MacOsTheme.Gap });
        row.Children.Add(new Border { MinWidth = 90, Child = InputBox(MakeText("HH : MM", MacOsTheme.SecText)) });
        stack.Children.Add(row);
        return stack;
    }

    private UIElement RenderCalendar(UiNode node)
    {
        var stack = new StackPanel();

        var header = new StackPanel
        {
            Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, MacOsTheme.Gap),
        };
        header.Children.Add(new Border
        {
            Background = MacOsTheme.SidebarBg, CornerRadius = new CornerRadius(5),
            Padding = new Thickness(6, 2, 6, 2), Margin = new Thickness(0, 0, 8, 0),
            Child = MakeText("‹", MacOsTheme.SecText, 14),
        });
        header.Children.Add(MakeText("April 2024", weight: FontWeights.SemiBold));
        header.Children.Add(new Border
        {
            Background = MacOsTheme.SidebarBg, CornerRadius = new CornerRadius(5),
            Padding = new Thickness(6, 2, 6, 2), Margin = new Thickness(8, 0, 0, 0),
            Child = MakeText("›", MacOsTheme.SecText, 14),
        });
        stack.Children.Add(header);

        var dowGrid = new UniformGrid { Columns = 7, Rows = 1 };
        foreach (var d in new[] { "Su", "Mo", "Tu", "We", "Th", "Fr", "Sa" })
            dowGrid.Children.Add(new TextBlock
            {
                Text = d, FontFamily = MacOsTheme.Font, FontSize = MacOsTheme.FontSize - 1,
                Foreground = MacOsTheme.SecText, TextAlignment = TextAlignment.Center,
                Padding = new Thickness(0, 2, 0, 4),
            });
        stack.Children.Add(dowGrid);

        var dayGrid = new UniformGrid { Columns = 7, Rows = 5 };
        var cells = new[] { "", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "", "", "", "" };
        foreach (var cell in cells)
        {
            bool today = cell == "15";
            var tb = new TextBlock
            {
                Text = cell, FontFamily = MacOsTheme.Font, FontSize = MacOsTheme.FontSize,
                Foreground = today ? MacOsTheme.AccentText : MacOsTheme.DarkText,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(2, 4, 2, 4), MinWidth = 30,
            };
            dayGrid.Children.Add(today
                ? new Border
                  {
                      Background = MacOsTheme.Accent, CornerRadius = new CornerRadius(14),
                      Margin = new Thickness(2), Child = tb,
                  }
                : tb);
        }
        stack.Children.Add(dayGrid);

        return new Border
        {
            Background = MacOsTheme.CardBg, BorderBrush = MacOsTheme.CardBorder, BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(MacOsTheme.CornerRadius), Padding = new Thickness(MacOsTheme.Pad),
            Margin = new Thickness(4), Child = stack,
        };
    }

    private UIElement RenderPlaceholder(UiNode node) => new Border
    {
        Background = MacOsTheme.SidebarBg, BorderBrush = MacOsTheme.CardBorder,
        BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(MacOsTheme.CornerRadius / 2),
        Padding = new Thickness(6, 3, 6, 3), Margin = new Thickness(2),
        Child = MakeText($"[{node.Type}]", MacOsTheme.SecText, MacOsTheme.FontSize - 1),
    };
}
