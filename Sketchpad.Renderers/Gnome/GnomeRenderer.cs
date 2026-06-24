using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using Sketchpad.Core.Ast;
using Sketchpad.Core.Rendering;

namespace Sketchpad.Renderers.Gnome;

/// <summary>
/// GNOME / Adwaita renderer.
/// Dark headerbars, rounded cards, and a blue accent colour.
/// </summary>
public class GnomeRenderer : IUiRenderer<UIElement>
{
    public string DisplayName => "GNOME / GTK";

    public UIElement Render(UiDocument document)
    {
        var root = new StackPanel
        {
            Orientation         = Orientation.Vertical,
            Background          = GnomeTheme.PageBg,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        foreach (var node in document.Roots)
            root.Children.Add(RenderNode(node));

        if (document.HasErrors)
        {
            var errPanel = new StackPanel { Margin = new Thickness(GnomeTheme.Pad) };
            foreach (var err in document.Errors)
                errPanel.Children.Add(MakeText($"Line {err.Line}: {err.Message}", GnomeTheme.Destructive));
            root.Children.Insert(0, errPanel);
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
                ElementType.Spacer   => new Border { Height = GnomeTheme.Gap },

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

                ElementType.Label    => MakeText(node.Label ?? "", GnomeTheme.MutedText, 10),
                ElementType.Text     => MakeText(node.Label ?? ""),
                ElementType.Heading  => RenderHeading(node),
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
        catch
        {
            return RenderPlaceholder(node);
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static TextBlock MakeText(string text, Brush? fg = null, double size = GnomeTheme.FontSize,
        FontWeight? weight = null)
        => new()
        {
            Text         = text,
            FontFamily   = GnomeTheme.Font,
            FontSize     = size,
            Foreground   = fg ?? GnomeTheme.DarkText,
            FontWeight   = weight ?? FontWeights.Normal,
            TextWrapping = TextWrapping.Wrap,
        };

    private Border InputBox(UIElement content, double? height = null) => new()
    {
        Background      = GnomeTheme.InputBg,
        BorderBrush     = GnomeTheme.InputBorder,
        BorderThickness = new Thickness(1),
        CornerRadius    = new CornerRadius(GnomeTheme.CornerRadius),
        Padding         = new Thickness(8, 5, 8, 5),
        Height          = height ?? double.NaN,
        Margin          = new Thickness(0, 3, 0, 0),
        Child           = content,
    };

    private StackPanel RenderChildren(UiNode node, Orientation orientation, double gap = GnomeTheme.Gap)
    {
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
        return p.Length == 2 &&
               int.TryParse(p[0], out var w) &&
               int.TryParse(p[1], out var h)
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

        // GNOME headerbar: dark, window buttons on LEFT, title centred
        var hb = new Border
        {
            Background = GnomeTheme.HeaderbarBg,
            Height     = GnomeTheme.HeaderHeight,
            Padding    = new Thickness(8, 0, 8, 0),
        };

        var hbGrid = new Grid();
        hbGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        hbGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        hbGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // Window buttons (left side — GNOME default)
        var winBtns = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        winBtns.Children.Add(GnomeWindowBtn(GnomeTheme.Destructive));
        winBtns.Children.Add(GnomeWindowBtn(GnomeTheme.ButtonBorder));
        winBtns.Children.Add(GnomeWindowBtn(GnomeTheme.ButtonBorder));

        var title = new TextBlock
        {
            Text                = node.Label ?? "Window",
            FontFamily          = GnomeTheme.Font,
            FontSize            = GnomeTheme.FontSize,
            FontWeight          = FontWeights.SemiBold,
            Foreground          = GnomeTheme.HeaderbarText,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment   = VerticalAlignment.Center,
        };

        Grid.SetColumn(winBtns, 0);
        Grid.SetColumn(title, 1);
        hbGrid.Children.Add(winBtns);
        hbGrid.Children.Add(title);
        hb.Child = hbGrid;

        stack.Children.Add(hb);
        stack.Children.Add(new Border
        {
            Background = GnomeTheme.PageBg,
            Padding    = new Thickness(GnomeTheme.Pad),
            Child      = RenderChildren(node, Orientation.Vertical),
        });

        return new Border
        {
            Child               = stack,
            Width               = width,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin              = new Thickness(8),
            CornerRadius        = new CornerRadius(GnomeTheme.CardRadius),
            ClipToBounds        = true,
            BorderBrush         = GnomeTheme.CardBorder,
            BorderThickness     = new Thickness(1),
        };
    }

    private static UIElement GnomeWindowBtn(Brush fill) => new Ellipse
    {
        Width             = 12,
        Height            = 12,
        Fill              = fill,
        Margin            = new Thickness(3, 0, 3, 0),
        VerticalAlignment = VerticalAlignment.Center,
    };

    private UIElement RenderPanel(UiNode node)
    {
        var content = RenderChildren(node, Orientation.Vertical);
        content.Margin = new Thickness(GnomeTheme.Pad);

        if (node.Label == null) return content;

        var stack = new StackPanel { Orientation = Orientation.Vertical };
        stack.Children.Add(new Border
        {
            Background      = GnomeTheme.HeaderbarBg,
            Padding         = new Thickness(GnomeTheme.Pad, 6, GnomeTheme.Pad, 6),
            Child           = MakeText(node.Label, GnomeTheme.HeaderbarText, weight: FontWeights.SemiBold),
        });
        stack.Children.Add(content);

        return new Border
        {
            Child         = stack,
            BorderBrush   = GnomeTheme.CardBorder,
            BorderThickness = new Thickness(1),
            CornerRadius  = new CornerRadius(GnomeTheme.CardRadius),
            ClipToBounds  = true,
            Margin        = new Thickness(4),
        };
    }

    private UIElement RenderCard(UiNode node)
    {
        var inner = new StackPanel { Orientation = Orientation.Vertical };

        if (node.Label != null)
            inner.Children.Add(new Border
            {
                Padding         = new Thickness(0, 0, 0, GnomeTheme.Gap),
                BorderBrush     = GnomeTheme.Separator,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Margin          = new Thickness(0, 0, 0, GnomeTheme.Gap),
                Child           = MakeText(node.Label, weight: FontWeights.SemiBold),
            });

        foreach (var child in node.Children)
            inner.Children.Add(new Border
            {
                Margin = new Thickness(0, 0, 0, GnomeTheme.Gap / 2),
                Child  = RenderNode(child),
            });

        return new Border
        {
            Background      = GnomeTheme.CardBg,
            BorderBrush     = GnomeTheme.CardBorder,
            BorderThickness = new Thickness(1),
            CornerRadius    = new CornerRadius(GnomeTheme.CardRadius),
            Padding         = new Thickness(GnomeTheme.Pad),
            Margin          = new Thickness(4),
            Child           = inner,
        };
    }

    private UIElement RenderRow(UiNode node)
    {
        if (node.HasModifier("vertical"))
            return RenderChildren(node, Orientation.Vertical);

        var grid = new UniformGrid { Rows = 1, Columns = node.Children.Count };
        foreach (var child in node.Children)
            grid.Children.Add(new Border
            {
                Padding = new Thickness(GnomeTheme.Gap / 2, 0, GnomeTheme.Gap / 2, 0),
                Child   = RenderNode(child),
            });
        return grid;
    }

    // ── Navigation ───────────────────────────────────────────────────────────

    private UIElement RenderNavbar(UiNode node)
    {
        var left  = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        var right = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };

        foreach (var child in node.Children)
        {
            if (child.Type == ElementType.Menu && child.HasModifier("right"))
                foreach (var sub in child.Children) right.Children.Add(RenderNode(sub));
            else
                left.Children.Add(RenderNode(child));
        }

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        Grid.SetColumn(left, 0);
        Grid.SetColumn(right, 2);
        grid.Children.Add(left);
        grid.Children.Add(right);

        return new Border
        {
            Background = GnomeTheme.HeaderbarBg,
            Height     = GnomeTheme.HeaderHeight,
            Padding    = new Thickness(GnomeTheme.Pad, 0, GnomeTheme.Pad, 0),
            Child      = grid,
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
            Background      = GnomeTheme.SidebarBg,
            BorderBrush     = GnomeTheme.CardBorder,
            BorderThickness = new Thickness(0, 0, 1, 0),
            Child           = inner,
        };
    }

    private UIElement RenderMenu(UiNode node)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal };
        foreach (var child in node.Children)
            panel.Children.Add(RenderNode(child));
        return panel;
    }

    private UIElement RenderItem(UiNode node)
    {
        bool active = node.HasModifier("active");
        return new Border
        {
            Background   = active ? GnomeTheme.ActiveItemBg : Brushes.Transparent,
            CornerRadius = new CornerRadius(GnomeTheme.CornerRadius),
            Padding      = new Thickness(8, 5, 8, 5),
            Margin       = new Thickness(2, 1, 2, 1),
            Child        = MakeText(node.Label ?? "",
                fg: active ? GnomeTheme.AccentText : GnomeTheme.DarkText),
        };
    }

    private UIElement RenderTabs(UiNode node)
    {
        var strip = new StackPanel { Orientation = Orientation.Horizontal };
        foreach (var child in node.Children) strip.Children.Add(RenderNode(child));
        return new Border
        {
            BorderBrush     = GnomeTheme.Separator,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Child           = strip,
        };
    }

    private UIElement RenderTab(UiNode node)
    {
        bool active = node.HasModifier("active");
        return new Border
        {
            Padding         = new Thickness(16, 8, 16, 8),
            BorderBrush     = GnomeTheme.Accent,
            BorderThickness = new Thickness(0, 0, 0, active ? 3 : 0),
            Margin          = new Thickness(0, 0, 4, 0),
            Child           = MakeText(node.Label ?? "",
                fg: active ? GnomeTheme.Accent : GnomeTheme.MutedText,
                weight: active ? FontWeights.SemiBold : FontWeights.Normal),
        };
    }

    private UIElement RenderBrand(UiNode node) => new Border
    {
        Padding = new Thickness(0, 0, 16, 0),
        Child   = MakeText(node.Label ?? "Brand", GnomeTheme.HeaderbarText, weight: FontWeights.Bold),
    };

    // ── Form ─────────────────────────────────────────────────────────────────

    private UIElement RenderField(UiNode node)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical };
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, GnomeTheme.DarkText, 10));
        stack.Children.Add(InputBox(MakeText(node.Value ?? "", GnomeTheme.MutedText)));
        return stack;
    }

    private UIElement RenderTextarea(UiNode node)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical };
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, GnomeTheme.DarkText, 10));
        stack.Children.Add(InputBox(new Border(), height: 72));
        return stack;
    }

    private UIElement RenderCheckbox(UiNode node)
    {
        bool chk = node.HasModifier("checked");
        var row = new StackPanel { Orientation = Orientation.Horizontal };

        var box = new Border
        {
            Width           = 16,
            Height          = 16,
            CornerRadius    = new CornerRadius(4),
            BorderBrush     = chk ? GnomeTheme.Accent : GnomeTheme.InputBorder,
            BorderThickness = new Thickness(1.5),
            Background      = chk ? GnomeTheme.Accent : GnomeTheme.InputBg,
            Margin          = new Thickness(0, 0, 6, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Child = chk ? new TextBlock
            {
                Text = "✓", FontSize = 10, Foreground = GnomeTheme.AccentText,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment   = VerticalAlignment.Center,
            } : null,
        };

        row.Children.Add(box);
        row.Children.Add(MakeText(node.Label ?? ""));
        return row;
    }

    private UIElement RenderRadio(UiNode node)
    {
        bool chk = node.HasModifier("checked");
        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(new Ellipse
        {
            Width             = 16,
            Height            = 16,
            Stroke            = chk ? GnomeTheme.Accent : GnomeTheme.InputBorder,
            StrokeThickness   = 1.5,
            Fill              = chk ? GnomeTheme.Accent : GnomeTheme.InputBg,
            Margin            = new Thickness(0, 0, 6, 0),
            VerticalAlignment = VerticalAlignment.Center,
        });
        row.Children.Add(MakeText(node.Label ?? ""));
        return row;
    }

    private UIElement RenderSelect(UiNode node)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical };
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, GnomeTheme.DarkText, 10));

        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var val  = MakeText(node.Value ?? "", GnomeTheme.DarkText);
        var chev = MakeText("▾", GnomeTheme.MutedText);
        Grid.SetColumn(val, 0); Grid.SetColumn(chev, 1);
        row.Children.Add(val); row.Children.Add(chev);

        stack.Children.Add(InputBox(row));
        return stack;
    }

    private UIElement RenderToggle(UiNode node)
    {
        bool on = node.HasModifier("on") || node.HasModifier("checked");
        var track = new Border
        {
            Width        = 38,
            Height       = 20,
            CornerRadius = new CornerRadius(10),
            Background   = on ? GnomeTheme.Accent : GnomeTheme.ButtonBorder,
            Margin       = new Thickness(0, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Center,
        };

        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(track);
        row.Children.Add(MakeText(node.Label ?? ""));
        return row;
    }

    private UIElement RenderSlider(UiNode node)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical };
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, GnomeTheme.DarkText, 10));

        var track = new Border
        {
            Height       = 4,
            Background   = GnomeTheme.CardBorder,
            CornerRadius = new CornerRadius(2),
            Margin       = new Thickness(0, 8, 0, 0),
        };
        var filled = new Border
        {
            Height       = 4,
            Background   = GnomeTheme.Accent,
            CornerRadius = new CornerRadius(2),
            HorizontalAlignment = HorizontalAlignment.Left,
            Width        = 80,
        };

        var knob = new Ellipse
        {
            Width  = 16,
            Height = 16,
            Fill   = GnomeTheme.Accent,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(72, 0, 0, 0),
        };

        var canvas = new Grid { Height = 16, Margin = new Thickness(0, 4, 0, 0) };
        canvas.Children.Add(track);
        canvas.Children.Add(filled);
        canvas.Children.Add(knob);

        stack.Children.Add(canvas);
        return stack;
    }

    private UIElement RenderButton(UiNode node)
    {
        bool primary  = node.HasModifier("primary");
        bool danger   = node.HasModifier("danger");
        bool disabled = node.HasModifier("disabled");

        Brush bg = danger   ? GnomeTheme.Destructive
                 : primary  ? GnomeTheme.Accent
                 : GnomeTheme.ButtonBg;
        Brush fg = (primary || danger) ? GnomeTheme.AccentText : GnomeTheme.DarkText;
        if (disabled) { bg = GnomeTheme.CardBorder; fg = GnomeTheme.MutedText; }

        return new Border
        {
            Background      = bg,
            BorderBrush     = danger || primary ? Brushes.Transparent : GnomeTheme.ButtonBorder,
            BorderThickness = new Thickness(1),
            CornerRadius    = new CornerRadius(GnomeTheme.CornerRadius),
            Padding         = new Thickness(14, 6, 14, 6),
            Margin          = new Thickness(0, 0, GnomeTheme.Gap, 0),
            Child           = new TextBlock
            {
                Text                = node.Label ?? "Button",
                FontFamily          = GnomeTheme.Font,
                FontSize            = GnomeTheme.FontSize,
                Foreground          = fg,
                HorizontalAlignment = HorizontalAlignment.Center,
            },
        };
    }

    // ── Display ──────────────────────────────────────────────────────────────

    private UIElement RenderHeading(UiNode node) =>
        MakeText(node.Label ?? "", size: 16, weight: FontWeights.Bold);

    private UIElement RenderAvatar(UiNode node)
    {
        bool circle = node.HasModifier("circle");
        var size = 40;
        if (circle)
            return new Ellipse
            {
                Width = size, Height = size,
                Fill = GnomeTheme.Accent, Margin = new Thickness(0, 0, 0, 4),
            };
        return new Rectangle
        {
            Width = size, Height = size,
            Fill = GnomeTheme.Accent, RadiusX = GnomeTheme.CardRadius, RadiusY = GnomeTheme.CardRadius,
            Margin = new Thickness(0, 0, 0, 4),
        };
    }

    private UIElement RenderImage(UiNode node)
    {
        double w = 200, h = 150;
        foreach (var m in node.Modifiers) { var (mw, mh) = ParseWxH(m); if (mw > 0) { w = mw; h = mh; } }

        var canvas = new Canvas { Width = w, Height = h };
        canvas.Children.Add(new Rectangle
        {
            Width = w, Height = h, Fill = GnomeTheme.SidebarBg,
            RadiusX = GnomeTheme.CornerRadius, RadiusY = GnomeTheme.CornerRadius,
        });
        canvas.Children.Add(new Line { X1 = 0, Y1 = 0, X2 = w, Y2 = h, Stroke = GnomeTheme.CardBorder, StrokeThickness = 1 });
        canvas.Children.Add(new Line { X1 = w, Y1 = 0, X2 = 0, Y2 = h, Stroke = GnomeTheme.CardBorder, StrokeThickness = 1 });
        if (node.Label != null)
        {
            var lbl = MakeText(node.Label, GnomeTheme.MutedText, 10);
            Canvas.SetLeft(lbl, 4); Canvas.SetTop(lbl, 4);
            canvas.Children.Add(lbl);
        }
        return canvas;
    }

    private UIElement RenderBadge(UiNode node) => new Border
    {
        Background   = GnomeTheme.Accent,
        CornerRadius = new CornerRadius(10),
        Padding      = new Thickness(7, 2, 7, 2),
        Margin       = new Thickness(0, 0, 4, 0),
        Child        = MakeText(node.Label ?? "", GnomeTheme.AccentText, 10),
    };

    private UIElement RenderTag(UiNode node)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(MakeText(node.Label ?? "", size: 10));
        row.Children.Add(MakeText(" ×", GnomeTheme.MutedText, 10));
        return new Border
        {
            Background      = GnomeTheme.SidebarBg,
            BorderBrush     = GnomeTheme.CardBorder,
            BorderThickness = new Thickness(1),
            CornerRadius    = new CornerRadius(GnomeTheme.CornerRadius),
            Padding         = new Thickness(6, 2, 6, 2),
            Margin          = new Thickness(0, 0, 4, 0),
            Child           = row,
        };
    }

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
                    Background      = GnomeTheme.SidebarBg,
                    Padding         = new Thickness(8, 6, 8, 6),
                    BorderBrush     = GnomeTheme.Separator,
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    Child           = MakeText(headers[c], weight: FontWeights.SemiBold, size: 10),
                };
                Grid.SetRow(cell, 0); Grid.SetColumn(cell, c);
                grid.Children.Add(cell);
            }
            rowIdx++;
        }

        bool altRow = false;
        foreach (var row in rows)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var cells = row.Label?.Split('|').Select(s => s.Trim()).ToArray() ?? [];
            for (int c = 0; c < Math.Min(cells.Length, colCount); c++)
            {
                var cell = new Border
                {
                    Background      = altRow ? GnomeTheme.SidebarBg : GnomeTheme.CardBg,
                    Padding         = new Thickness(8, 6, 8, 6),
                    BorderBrush     = GnomeTheme.Separator,
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    Child           = MakeText(cells[c]),
                };
                Grid.SetRow(cell, rowIdx); Grid.SetColumn(cell, c);
                grid.Children.Add(cell);
            }
            rowIdx++;
            altRow = !altRow;
        }

        return new Border
        {
            BorderBrush     = GnomeTheme.CardBorder,
            BorderThickness = new Thickness(1),
            CornerRadius    = new CornerRadius(GnomeTheme.CornerRadius),
            ClipToBounds    = true,
            Margin          = new Thickness(0, 4, 0, 4),
            Child           = grid,
        };
    }

    private UIElement RenderIcon(UiNode node) => new Border
    {
        Width        = 22,
        Height       = 22,
        Background   = GnomeTheme.SidebarBg,
        BorderBrush  = GnomeTheme.CardBorder,
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(4),
        Margin       = new Thickness(0, 0, 4, 0),
        Child        = MakeText(node.Label?[..1].ToUpper() ?? "?", GnomeTheme.MutedText, 10),
    };

    // ── Feedback ─────────────────────────────────────────────────────────────

    private static UIElement RenderDivider() => new Border
    {
        Height              = 1,
        Background          = GnomeTheme.Separator,
        Margin              = new Thickness(0, 4, 0, 4),
        HorizontalAlignment = HorizontalAlignment.Stretch,
    };

    private UIElement RenderAlert(UiNode node)
    {
        bool warning = node.HasModifier("warning");
        bool danger  = node.HasModifier("danger") || node.HasModifier("error");
        bool success = node.HasModifier("success");

        Brush bg = warning ? GnomeTheme.WarningBg
                 : danger  ? GnomeTheme.ErrorBg
                 : success ? GnomeTheme.SuccessBg
                 : new SolidColorBrush(Color.FromRgb(0xCC, 0xE5, 0xFF));

        return new Border
        {
            Background      = bg,
            BorderBrush     = GnomeTheme.CardBorder,
            BorderThickness = new Thickness(1),
            CornerRadius    = new CornerRadius(GnomeTheme.CornerRadius),
            Padding         = new Thickness(GnomeTheme.Pad, 8, GnomeTheme.Pad, 8),
            Margin          = new Thickness(0, 4, 0, 4),
            Child           = MakeText(node.Label ?? ""),
        };
    }

    private UIElement RenderToast(UiNode node) => new Border
    {
        Background          = GnomeTheme.HeaderbarBg,
        CornerRadius        = new CornerRadius(GnomeTheme.CardRadius),
        Padding             = new Thickness(GnomeTheme.Pad, 8, GnomeTheme.Pad, 8),
        Margin              = new Thickness(0, 4, 0, 4),
        HorizontalAlignment = HorizontalAlignment.Center,
        Child               = MakeText(node.Label ?? "", GnomeTheme.HeaderbarText),
    };

    private static UIElement RenderSpinner() => new Ellipse
    {
        Width           = 24,
        Height          = 24,
        Stroke          = GnomeTheme.Accent,
        StrokeThickness = 3,
        StrokeDashArray = new DoubleCollection([5, 3]),
        Margin          = new Thickness(4),
    };

    private UIElement RenderProgress(UiNode node)
    {
        int pct = 0;
        foreach (var m in node.Modifiers) if (int.TryParse(m, out var v)) pct = v;
        pct = Math.Clamp(pct, 0, 100);

        var track = new Grid { Height = 6 };
        track.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(pct, GridUnitType.Star) });
        track.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100 - pct, GridUnitType.Star) });
        var fill = new Rectangle { Fill = GnomeTheme.Accent, RadiusX = 3, RadiusY = 3 };
        Grid.SetColumn(fill, 0);
        track.Children.Add(fill);

        return new Border
        {
            Background   = GnomeTheme.CardBorder,
            CornerRadius = new CornerRadius(3),
            Child        = track,
            Margin       = new Thickness(0, 4, 0, 4),
        };
    }

    // ── Date / Time ───────────────────────────────────────────────────────────

    private UIElement RenderDatePicker(UiNode node)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical };
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, GnomeTheme.DarkText, 10));

        var row = new Grid { Margin = new Thickness(0, 3, 0, 0) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var inner = new Grid();
        inner.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        inner.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var dateTxt = MakeText(node.Value ?? "MM / DD / YYYY", GnomeTheme.MutedText);
        var calIco  = MakeText("▦", GnomeTheme.MutedText);
        Grid.SetColumn(dateTxt, 0); Grid.SetColumn(calIco, 1);
        inner.Children.Add(dateTxt); inner.Children.Add(calIco);

        var inputBorder = InputBox(inner);
        Grid.SetColumn(inputBorder, 0);
        row.Children.Add(inputBorder);
        stack.Children.Add(row);
        return stack;
    }

    private UIElement RenderDateTimePicker(UiNode node)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical };
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, GnomeTheme.DarkText, 10));

        var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 3, 0, 0) };
        row.Children.Add(new Border { MinWidth = 140, Child = InputBox(MakeText("MM / DD / YYYY", GnomeTheme.MutedText)) });
        row.Children.Add(new Border { Width = GnomeTheme.Gap });
        row.Children.Add(new Border { MinWidth = 90, Child = InputBox(MakeText("HH : MM", GnomeTheme.MutedText)) });

        stack.Children.Add(row);
        return stack;
    }

    private UIElement RenderCalendar(UiNode node)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical };

        var header = new StackPanel
        {
            Orientation         = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin              = new Thickness(0, 0, 0, GnomeTheme.Gap),
        };
        header.Children.Add(new Border
        {
            Background = GnomeTheme.ButtonBg, CornerRadius = new CornerRadius(GnomeTheme.CornerRadius),
            Padding = new Thickness(6, 2, 6, 2), Margin = new Thickness(0, 0, 8, 0),
            Child = MakeText("◀", GnomeTheme.MutedText),
        });
        header.Children.Add(MakeText("  April 2024  ", weight: FontWeights.SemiBold));
        header.Children.Add(new Border
        {
            Background = GnomeTheme.ButtonBg, CornerRadius = new CornerRadius(GnomeTheme.CornerRadius),
            Padding = new Thickness(6, 2, 6, 2), Margin = new Thickness(8, 0, 0, 0),
            Child = MakeText("▶", GnomeTheme.MutedText),
        });
        stack.Children.Add(header);

        var dowGrid = new UniformGrid { Columns = 7, Rows = 1 };
        foreach (var d in new[] { "Su", "Mo", "Tu", "We", "Th", "Fr", "Sa" })
            dowGrid.Children.Add(GnomeCalCell(d, GnomeTheme.MutedText, FontWeights.SemiBold));
        stack.Children.Add(dowGrid);

        var dayGrid = new UniformGrid { Columns = 7, Rows = 5 };
        var cells = new[]
        {
            "", "1", "2", "3", "4", "5", "6",
            "7", "8", "9", "10", "11", "12", "13",
            "14", "15", "16", "17", "18", "19", "20",
            "21", "22", "23", "24", "25", "26", "27",
            "28", "29", "30", "", "", "", "",
        };

        foreach (var cell in cells)
        {
            bool today = cell == "15";
            var tb = GnomeCalCell(cell, today ? GnomeTheme.AccentText : GnomeTheme.DarkText);
            dayGrid.Children.Add(today
                ? new Border
                  {
                      Background   = GnomeTheme.Accent,
                      CornerRadius = new CornerRadius(GnomeTheme.CornerRadius),
                      Child        = tb,
                      Margin       = new Thickness(2),
                  }
                : tb);
        }
        stack.Children.Add(dayGrid);

        return new Border
        {
            Background      = GnomeTheme.CardBg,
            BorderBrush     = GnomeTheme.CardBorder,
            BorderThickness = new Thickness(1),
            CornerRadius    = new CornerRadius(GnomeTheme.CardRadius),
            Padding         = new Thickness(GnomeTheme.Pad),
            Margin          = new Thickness(4),
            Child           = stack,
        };
    }

    private static TextBlock GnomeCalCell(string text, Brush? fg = null, FontWeight? weight = null) => new()
    {
        Text                = text,
        FontFamily          = GnomeTheme.Font,
        FontSize            = GnomeTheme.FontSize,
        Foreground          = fg ?? GnomeTheme.DarkText,
        FontWeight          = weight ?? FontWeights.Normal,
        TextAlignment       = TextAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment   = VerticalAlignment.Center,
        Padding             = new Thickness(2, 4, 2, 4),
        MinWidth            = 28,
    };

    private UIElement RenderPlaceholder(UiNode node) => new Border
    {
        Background      = GnomeTheme.SidebarBg,
        BorderBrush     = GnomeTheme.CardBorder,
        BorderThickness = new Thickness(1),
        CornerRadius    = new CornerRadius(GnomeTheme.CornerRadius),
        Padding         = new Thickness(6, 3, 6, 3),
        Margin          = new Thickness(2),
        Child           = MakeText($"[{node.Type}]", GnomeTheme.MutedText),
    };
}
