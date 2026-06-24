using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using Sketchpad.Core.Ast;
using Sketchpad.Core.Rendering;

namespace Sketchpad.Renderers.WinXp;

/// <summary>
/// Windows XP Luna renderer.
/// Iconic navy-to-sky-blue gradient title bar, warm silver (#ECE9D8) client area,
/// soft 1-px bevel controls, and the distinctive blue accent (#316AC5).
/// </summary>
public class WinXpRenderer : IUiRenderer<UIElement>
{
    public string DisplayName => "Windows XP";

    public UIElement Render(UiDocument document)
    {
        var root = new StackPanel { Orientation = Orientation.Vertical, Background = WinXpTheme.PageBg };

        foreach (var node in document.Roots)
            root.Children.Add(RenderNode(node));

        if (document.HasErrors)
        {
            var err = new StackPanel { Margin = new Thickness(8) };
            foreach (var e in document.Errors)
                err.Children.Add(MakeText($"Line {e.Line}: {e.Message}", WinXpTheme.Destructive));
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
                ElementType.Divider  => Divider(),
                ElementType.Spacer   => new Border { Height = 8 },

                ElementType.Navbar   => RenderNavbar(node),
                ElementType.Sidebar  => RenderSidebar(node),
                ElementType.Menu     => RenderMenu(node),
                ElementType.Nav      => RenderChildren(node, Orientation.Vertical, 1),
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

                ElementType.Label    => MakeText(node.Label ?? "", WinXpTheme.MutedText, 10),
                ElementType.Text     => MakeText(node.Label ?? ""),
                ElementType.Heading  => MakeText(node.Label ?? "", size: 14, weight: FontWeights.Bold),
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

                _ => Placeholder(node),
            };
        }
        catch { return Placeholder(node); }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static TextBlock MakeText(string text, Brush? fg = null, double size = WinXpTheme.FontSize,
        FontWeight? weight = null) => new()
    {
        Text         = text,
        FontFamily   = WinXpTheme.Font,
        FontSize     = size,
        Foreground   = fg ?? WinXpTheme.DarkText,
        FontWeight   = weight ?? FontWeights.Normal,
        TextWrapping = TextWrapping.Wrap,
    };

    // Raised 1-px soft bevel (lighter than Win95)
    private static UIElement Raised(UIElement content, Brush? bg = null, Thickness? padding = null)
    {
        var core = new Border
        {
            Background   = bg ?? WinXpTheme.ButtonFace,
            Padding      = padding ?? new Thickness(6, 3, 6, 3),
            CornerRadius = new CornerRadius(3),
            Child        = content,
        };
        return new Border
        {
            BorderBrush     = WinXpTheme.Highlight,
            BorderThickness = new Thickness(1, 1, 0, 0),
            CornerRadius    = new CornerRadius(3),
            Margin          = new Thickness(2),
            Child           = new Border
            {
                BorderBrush     = WinXpTheme.Shadow,
                BorderThickness = new Thickness(0, 0, 1, 1),
                CornerRadius    = new CornerRadius(3),
                Child           = core,
            },
        };
    }

    private static Border InputBox(UIElement content, double? height = null) => new()
    {
        Background      = WinXpTheme.InputBg,
        BorderBrush     = WinXpTheme.InputBorder,
        BorderThickness = new Thickness(1),
        Padding         = new Thickness(4, 3, 4, 3),
        Height          = height ?? double.NaN,
        Margin          = new Thickness(0, 2, 0, 0),
        Child           = content,
    };

    private StackPanel RenderChildren(UiNode node, Orientation orientation, double gap = 4)
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

        // Luna gradient title bar
        var titleGrid = new Grid();
        titleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        titleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        titleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var titleText = new TextBlock
        {
            Text              = node.Label ?? "Window",
            FontFamily        = WinXpTheme.Font,
            FontSize          = WinXpTheme.FontSize,
            FontWeight        = FontWeights.Bold,
            Foreground        = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center,
            Margin            = new Thickness(6, 0, 0, 0),
        };

        var btnPanel = new StackPanel
        {
            Orientation       = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin            = new Thickness(0, 0, 4, 0),
        };
        btnPanel.Children.Add(XpBtn("_", Color.FromRgb(0x1E, 0x52, 0xA0), Color.FromRgb(0x12, 0x3C, 0x8A)));
        btnPanel.Children.Add(XpBtn("□", Color.FromRgb(0x1E, 0x52, 0xA0), Color.FromRgb(0x12, 0x3C, 0x8A)));
        btnPanel.Children.Add(XpBtn("×", Color.FromRgb(0xC4, 0x21, 0x26), Color.FromRgb(0x8B, 0x10, 0x14)));

        Grid.SetColumn(titleText, 0);
        Grid.SetColumn(btnPanel, 2);
        titleGrid.Children.Add(titleText);
        titleGrid.Children.Add(btnPanel);

        var titleBar = new Border
        {
            Background = WinXpTheme.TitleBar,
            Height     = WinXpTheme.TitleHeight,
            Padding    = new Thickness(4, 0, 4, 0),
            Child      = titleGrid,
        };

        stack.Children.Add(titleBar);
        stack.Children.Add(new Border
        {
            Background = WinXpTheme.PageBg,
            Padding    = new Thickness(8),
            Child      = RenderChildren(node, Orientation.Vertical),
        });

        return new Border
        {
            Child               = stack,
            Width               = width,
            BorderBrush         = WinXpTheme.FrameColor,
            BorderThickness     = new Thickness(2),
            CornerRadius        = new CornerRadius(8, 8, 0, 0),
            ClipToBounds        = true,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin              = new Thickness(8),
        };
    }

    private static UIElement XpBtn(string glyph, Color top, Color bottom)
    {
        var grad = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(0, 1) };
        grad.GradientStops.Add(new GradientStop(top, 0));
        grad.GradientStops.Add(new GradientStop(bottom, 1));
        return new Border
        {
            Width        = 21,
            Height       = 21,
            Background   = grad,
            CornerRadius = new CornerRadius(3),
            Margin       = new Thickness(2, 0, 0, 0),
            Child        = new TextBlock
            {
                Text                = glyph,
                FontSize            = 9,
                FontWeight          = FontWeights.Bold,
                Foreground          = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment   = VerticalAlignment.Center,
            },
        };
    }

    private UIElement RenderPanel(UiNode node)
    {
        var content = RenderChildren(node, Orientation.Vertical);
        content.Margin = new Thickness(8);

        if (node.Label == null)
            return (UIElement)Raised(content, padding: new Thickness(0));

        var stack = new StackPanel();
        var caption = new Border
        {
            Background = WinXpTheme.Accent,
            Padding    = new Thickness(8, 4, 8, 4),
            Child      = MakeText(node.Label, Brushes.White, weight: FontWeights.Bold),
        };
        stack.Children.Add(caption);
        stack.Children.Add(content);

        return new Border
        {
            Child           = stack,
            BorderBrush     = WinXpTheme.GroupBorder,
            BorderThickness = new Thickness(1),
            Margin          = new Thickness(4),
        };
    }

    private UIElement RenderCard(UiNode node)
    {
        var inner = new StackPanel { Margin = new Thickness(8) };
        if (node.Label != null)
        {
            inner.Children.Add(MakeText(node.Label, weight: FontWeights.Bold));
            inner.Children.Add(Divider());
        }
        foreach (var child in node.Children)
            inner.Children.Add(new Border { Margin = new Thickness(0, 2, 0, 2), Child = RenderNode(child) });

        return (UIElement)Raised(inner, padding: new Thickness(0));
    }

    private UIElement RenderRow(UiNode node)
    {
        if (node.HasModifier("vertical"))
            return RenderChildren(node, Orientation.Vertical);

        var grid = new UniformGrid { Rows = 1, Columns = node.Children.Count };
        foreach (var child in node.Children)
            grid.Children.Add(new Border { Padding = new Thickness(4, 0, 4, 0), Child = RenderNode(child) });
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
        Grid.SetColumn(left, 0); Grid.SetColumn(right, 2);
        grid.Children.Add(left); grid.Children.Add(right);

        return new Border
        {
            Background = WinXpTheme.TitleBar,
            Height     = 28,
            Padding    = new Thickness(8, 0, 8, 0),
            Child      = grid,
        };
    }

    private UIElement RenderSidebar(UiNode node)
    {
        double width = 180;
        foreach (var m in node.Modifiers) { var px = ParsePx(m); if (px > 0) width = px; }
        var inner = RenderChildren(node, Orientation.Vertical, 1);
        inner.Margin = new Thickness(4);
        return new Border
        {
            Width           = width,
            Background      = WinXpTheme.ButtonFace,
            BorderBrush     = WinXpTheme.GroupBorder,
            BorderThickness = new Thickness(0, 0, 1, 0),
            Child           = inner,
        };
    }

    private UIElement RenderMenu(UiNode node)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal };
        foreach (var child in node.Children) panel.Children.Add(RenderNode(child));
        return panel;
    }

    private UIElement RenderItem(UiNode node)
    {
        bool active = node.HasModifier("active");
        return new Border
        {
            Background  = active ? WinXpTheme.Accent : Brushes.Transparent,
            Padding     = new Thickness(8, 4, 8, 4),
            Margin      = new Thickness(0, 1, 0, 1),
            Child       = MakeText(node.Label ?? "", fg: active ? WinXpTheme.AccentText : WinXpTheme.DarkText),
        };
    }

    private UIElement RenderTabs(UiNode node)
    {
        var strip = new StackPanel { Orientation = Orientation.Horizontal };
        foreach (var child in node.Children) strip.Children.Add(RenderNode(child));
        return new Border
        {
            BorderBrush     = WinXpTheme.GroupBorder,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Child           = strip,
        };
    }

    private UIElement RenderTab(UiNode node)
    {
        bool active = node.HasModifier("active");
        UIElement lbl = MakeText(node.Label ?? "", weight: active ? FontWeights.Bold : FontWeights.Normal);
        return active
            ? new Border
            {
                Padding         = new Thickness(12, 6, 12, 6),
                Background      = WinXpTheme.PageBg,
                BorderBrush     = WinXpTheme.GroupBorder,
                BorderThickness = new Thickness(1, 1, 1, 0),
                Margin          = new Thickness(0, 0, 2, 0),
                Child           = lbl,
            }
            : new Border
            {
                Padding  = new Thickness(12, 4, 12, 4),
                Margin   = new Thickness(0, 2, 2, 0),
                Child    = lbl,
            };
    }

    private UIElement RenderBrand(UiNode node) => new Border
    {
        Padding = new Thickness(0, 0, 12, 0),
        Child   = MakeText(node.Label ?? "Brand", Brushes.White, weight: FontWeights.Bold),
    };

    // ── Form ─────────────────────────────────────────────────────────────────

    private UIElement RenderField(UiNode node)
    {
        var stack = new StackPanel();
        if (node.Label != null) stack.Children.Add(MakeText(node.Label));
        stack.Children.Add(InputBox(MakeText(node.Value ?? "", WinXpTheme.MutedText)));
        return stack;
    }

    private UIElement RenderTextarea(UiNode node)
    {
        var stack = new StackPanel();
        if (node.Label != null) stack.Children.Add(MakeText(node.Label));
        stack.Children.Add(InputBox(new Border(), height: 72));
        return stack;
    }

    private UIElement RenderCheckbox(UiNode node)
    {
        bool chk = node.HasModifier("checked");
        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(new Border
        {
            Width           = 13,
            Height          = 13,
            Background      = WinXpTheme.InputBg,
            BorderBrush     = WinXpTheme.InputBorder,
            BorderThickness = new Thickness(1),
            Margin          = new Thickness(0, 0, 4, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Child = chk ? new TextBlock
            {
                Text = "✓", FontSize = 9, Foreground = WinXpTheme.DarkText,
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
            Width = 13, Height = 13,
            Stroke = WinXpTheme.InputBorder, StrokeThickness = 1,
            Fill   = chk ? WinXpTheme.DarkText : WinXpTheme.InputBg,
            Margin = new Thickness(0, 0, 4, 0),
            VerticalAlignment = VerticalAlignment.Center,
        });
        row.Children.Add(MakeText(node.Label ?? ""));
        return row;
    }

    private UIElement RenderSelect(UiNode node)
    {
        var stack = new StackPanel();
        if (node.Label != null) stack.Children.Add(MakeText(node.Label));
        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var val  = MakeText(node.Value ?? "", WinXpTheme.DarkText);
        var btn  = (UIElement)Raised(MakeText("▾"), padding: new Thickness(4, 2, 4, 2));
        Grid.SetColumn(val, 0); Grid.SetColumn(btn, 1);
        row.Children.Add(val); row.Children.Add(btn);
        stack.Children.Add(InputBox(row));
        return stack;
    }

    private UIElement RenderToggle(UiNode node)
    {
        bool on = node.HasModifier("on") || node.HasModifier("checked");
        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(new Border
        {
            Width = 13, Height = 13,
            Background = on ? WinXpTheme.Accent : WinXpTheme.InputBg,
            BorderBrush = WinXpTheme.InputBorder, BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 4, 0), VerticalAlignment = VerticalAlignment.Center,
            Child = on ? new TextBlock
            {
                Text = "✓", FontSize = 9, Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
            } : null,
        });
        row.Children.Add(MakeText(node.Label ?? ""));
        return row;
    }

    private UIElement RenderSlider(UiNode node)
    {
        var stack = new StackPanel();
        if (node.Label != null) stack.Children.Add(MakeText(node.Label));
        var track = new Grid { Height = 14, Margin = new Thickness(0, 6, 0, 0) };
        track.Children.Add(new Border
        {
            Height = 4, Background = WinXpTheme.InputBg,
            BorderBrush = WinXpTheme.InputBorder, BorderThickness = new Thickness(1),
            VerticalAlignment = VerticalAlignment.Center,
        });
        track.Children.Add(new Rectangle
        {
            Width = 10, Height = 14, Fill = WinXpTheme.ButtonFace,
            Stroke = WinXpTheme.Shadow, StrokeThickness = 1,
            HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(60, 0, 0, 0),
        });
        stack.Children.Add(track);
        return stack;
    }

    private UIElement RenderButton(UiNode node)
    {
        bool primary  = node.HasModifier("primary");
        bool danger   = node.HasModifier("danger");
        bool disabled = node.HasModifier("disabled");

        Brush bg = danger ? WinXpTheme.Destructive : primary ? WinXpTheme.Accent : WinXpTheme.ButtonFace;
        Brush fg = (primary || danger) ? WinXpTheme.AccentText : WinXpTheme.DarkText;
        if (disabled) { bg = WinXpTheme.ButtonFace; fg = WinXpTheme.MutedText; }

        return (UIElement)Raised(new TextBlock
        {
            Text = node.Label ?? "Button", FontFamily = WinXpTheme.Font,
            FontSize = WinXpTheme.FontSize, Foreground = fg,
            HorizontalAlignment = HorizontalAlignment.Center,
        }, bg: bg, padding: new Thickness(10, 4, 10, 4));
    }

    // ── Display ──────────────────────────────────────────────────────────────

    private UIElement RenderAvatar(UiNode node)
    {
        bool circle = node.HasModifier("circle");
        return circle
            ? (UIElement)new Ellipse { Width = 40, Height = 40, Fill = WinXpTheme.Accent }
            : new Rectangle { Width = 40, Height = 40, Fill = WinXpTheme.Accent };
    }

    private UIElement RenderImage(UiNode node)
    {
        double w = 200, h = 150;
        foreach (var m in node.Modifiers) { var (mw, mh) = ParseWxH(m); if (mw > 0) { w = mw; h = mh; } }
        var canvas = new Canvas { Width = w, Height = h };
        canvas.Children.Add(new Rectangle { Width = w, Height = h, Fill = WinXpTheme.ButtonFace });
        canvas.Children.Add(new Line { X1 = 0, Y1 = 0, X2 = w, Y2 = h, Stroke = WinXpTheme.Shadow, StrokeThickness = 1 });
        canvas.Children.Add(new Line { X1 = w, Y1 = 0, X2 = 0, Y2 = h, Stroke = WinXpTheme.Shadow, StrokeThickness = 1 });
        return canvas;
    }

    private UIElement RenderBadge(UiNode node) => new Border
    {
        Background   = WinXpTheme.Accent,
        Padding      = new Thickness(6, 2, 6, 2),
        Margin       = new Thickness(0, 0, 4, 0),
        Child        = MakeText(node.Label ?? "", Brushes.White, 10),
    };

    private UIElement RenderTag(UiNode node)
    {
        return (UIElement)Raised(MakeText(node.Label ?? "", size: 10), padding: new Thickness(4, 2, 4, 2));
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
                    Background = WinXpTheme.Accent, Padding = new Thickness(6, 4, 6, 4),
                    Child = MakeText(headers[c], Brushes.White, weight: FontWeights.Bold),
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
                    Background = alt ? WinXpTheme.ButtonFace : WinXpTheme.InputBg,
                    Padding    = new Thickness(6, 4, 6, 4),
                    BorderBrush = WinXpTheme.GroupBorder, BorderThickness = new Thickness(0, 0, 0, 1),
                    Child      = MakeText(cells[c]),
                };
                Grid.SetRow(cell, rowIdx); Grid.SetColumn(cell, c); grid.Children.Add(cell);
            }
            rowIdx++; alt = !alt;
        }

        return new Border
        {
            BorderBrush = WinXpTheme.GroupBorder, BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 4, 0, 4), Child = grid,
        };
    }

    private UIElement RenderIcon(UiNode node) => (UIElement)Raised(
        MakeText(node.Label?[..1].ToUpper() ?? "?", WinXpTheme.MutedText, 10),
        padding: new Thickness(4, 2, 4, 2));

    // ── Feedback ─────────────────────────────────────────────────────────────

    private static UIElement Divider() => new Border
    {
        Height = 2, Margin = new Thickness(0, 4, 0, 4),
        BorderBrush = WinXpTheme.Shadow, BorderThickness = new Thickness(0, 1, 0, 0),
    };

    private UIElement RenderAlert(UiNode node)
    {
        bool warning = node.HasModifier("warning");
        bool danger  = node.HasModifier("danger") || node.HasModifier("error");
        bool success = node.HasModifier("success");

        Brush bg = warning ? WinXpTheme.WarningBg
                 : danger  ? WinXpTheme.ErrorBg
                 : success ? WinXpTheme.SuccessBg
                 : new SolidColorBrush(Color.FromRgb(0xCC, 0xE5, 0xFF));

        return new Border
        {
            Background = bg, BorderBrush = WinXpTheme.GroupBorder,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(8, 6, 8, 6), Margin = new Thickness(0, 4, 0, 4),
            Child = MakeText(node.Label ?? ""),
        };
    }

    private UIElement RenderToast(UiNode node) => new Border
    {
        Background = WinXpTheme.Accent, Padding = new Thickness(12, 6, 12, 6),
        Margin = new Thickness(0, 4, 0, 4), HorizontalAlignment = HorizontalAlignment.Center,
        Child = MakeText(node.Label ?? "", Brushes.White),
    };

    private static UIElement RenderSpinner() => new Ellipse
    {
        Width = 20, Height = 20,
        Stroke = WinXpTheme.Accent, StrokeThickness = 3,
        StrokeDashArray = new DoubleCollection([4, 2]), Margin = new Thickness(4),
    };

    private UIElement RenderProgress(UiNode node)
    {
        int pct = 0;
        foreach (var m in node.Modifiers) if (int.TryParse(m, out var v)) pct = v;
        pct = Math.Clamp(pct, 0, 100);

        // XP-style chunked progress bar
        var bar = new StackPanel { Orientation = Orientation.Horizontal };
        int filled = pct / 7;
        for (int i = 0; i < 14; i++)
            bar.Children.Add(new Border
            {
                Width = 12, Height = 16, Margin = new Thickness(1, 0, 1, 0),
                Background = i < filled ? WinXpTheme.Accent : Brushes.Transparent,
            });

        return new Border
        {
            Background = WinXpTheme.InputBg, BorderBrush = WinXpTheme.InputBorder,
            BorderThickness = new Thickness(1), Padding = new Thickness(2),
            Margin = new Thickness(0, 4, 0, 4), Child = bar,
        };
    }

    // ── Date / Time ───────────────────────────────────────────────────────────

    private UIElement RenderDatePicker(UiNode node)
    {
        var stack = new StackPanel();
        if (node.Label != null) stack.Children.Add(MakeText(node.Label));
        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var val = MakeText(node.Value ?? "MM/DD/YYYY", WinXpTheme.MutedText);
        var btn = (UIElement)Raised(MakeText("▦"), padding: new Thickness(4, 2, 4, 2));
        Grid.SetColumn(val, 0); Grid.SetColumn(btn, 1);
        row.Children.Add(val); row.Children.Add(btn);
        stack.Children.Add(InputBox(row));
        return stack;
    }

    private UIElement RenderDateTimePicker(UiNode node)
    {
        var stack = new StackPanel();
        if (node.Label != null) stack.Children.Add(MakeText(node.Label));
        var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 0) };
        row.Children.Add(new Border { MinWidth = 130, Child = InputBox(MakeText("MM/DD/YYYY", WinXpTheme.MutedText)) });
        row.Children.Add(new Border { Width = 4 });
        row.Children.Add(new Border { MinWidth = 80, Child = InputBox(MakeText("HH:MM", WinXpTheme.MutedText)) });
        stack.Children.Add(row);
        return stack;
    }

    private UIElement RenderCalendar(UiNode node)
    {
        var stack = new StackPanel();

        // Month header
        var header = new Border
        {
            Background = WinXpTheme.Accent, Padding = new Thickness(4, 4, 4, 4),
        };
        var hGrid = new Grid();
        hGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        hGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        hGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var prev = (UIElement)Raised(MakeText("◄", Brushes.White, 9), padding: new Thickness(4, 1, 4, 1));
        var next = (UIElement)Raised(MakeText("►", Brushes.White, 9), padding: new Thickness(4, 1, 4, 1));
        var monthLbl = new TextBlock
        {
            Text = "April 2024", FontFamily = WinXpTheme.Font, FontSize = WinXpTheme.FontSize,
            FontWeight = FontWeights.Bold, Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetColumn(prev, 0); Grid.SetColumn(monthLbl, 1); Grid.SetColumn(next, 2);
        hGrid.Children.Add(prev); hGrid.Children.Add(monthLbl); hGrid.Children.Add(next);
        header.Child = hGrid;
        stack.Children.Add(header);

        var dowGrid = new UniformGrid { Columns = 7, Rows = 1, Background = WinXpTheme.Accent };
        foreach (var d in new[] { "Su", "Mo", "Tu", "We", "Th", "Fr", "Sa" })
            dowGrid.Children.Add(new TextBlock
            {
                Text = d, FontFamily = WinXpTheme.Font, FontSize = WinXpTheme.FontSize,
                Foreground = Brushes.White, TextAlignment = TextAlignment.Center,
                Padding = new Thickness(0, 2, 0, 2),
            });
        stack.Children.Add(dowGrid);

        var dayGrid = new UniformGrid { Columns = 7, Rows = 5, Background = WinXpTheme.InputBg };
        var cells = new[] { "", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "", "", "", "" };
        foreach (var cell in cells)
        {
            bool today = cell == "15";
            dayGrid.Children.Add(new Border
            {
                Background = today ? WinXpTheme.Accent : Brushes.Transparent,
                Child = new TextBlock
                {
                    Text = cell, FontFamily = WinXpTheme.Font, FontSize = WinXpTheme.FontSize,
                    Foreground = today ? Brushes.White : WinXpTheme.DarkText,
                    TextAlignment = TextAlignment.Center, Padding = new Thickness(2, 3, 2, 3),
                },
            });
        }
        stack.Children.Add(dayGrid);

        return new Border
        {
            BorderBrush = WinXpTheme.GroupBorder, BorderThickness = new Thickness(1),
            Margin = new Thickness(4), Child = stack,
        };
    }

    private UIElement Placeholder(UiNode node) => (UIElement)Raised(
        MakeText($"[{node.Type}]", WinXpTheme.MutedText, 10), padding: new Thickness(4, 2, 4, 2));
}
