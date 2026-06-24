using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using Sketchpad.Core.Ast;
using Sketchpad.Core.Rendering;

namespace Sketchpad.Renderers.Win95;

/// <summary>
/// Windows 95 / 98 Classic renderer.
/// Silver backgrounds, navy title bars, and 3-D beveled borders.
/// </summary>
public class Win95Renderer : IUiRenderer<UIElement>
{
    public string DisplayName => "Windows Classic";

    public UIElement Render(UiDocument document)
    {
        var root = new StackPanel
        {
            Orientation         = Orientation.Vertical,
            Background          = Win95Theme.ButtonFace,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        foreach (var node in document.Roots)
            root.Children.Add(RenderNode(node));

        if (document.HasErrors)
        {
            var errPanel = new StackPanel { Margin = new Thickness(4) };
            foreach (var err in document.Errors)
                errPanel.Children.Add(MakeText($"Line {err.Line}: {err.Message}", Win95Theme.DarkText));
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
                ElementType.Spacer   => new Border { Height = 8 },

                ElementType.Navbar   => RenderNavbar(node),
                ElementType.Sidebar  => RenderSidebar(node),
                ElementType.Menu     => RenderMenu(node),
                ElementType.Nav      => RenderChildren(node, Orientation.Vertical, 0),
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

                ElementType.Label    => MakeText(node.Label ?? "", Win95Theme.DarkText),
                ElementType.Text     => MakeText(node.Label ?? "", Win95Theme.DarkText),
                ElementType.Heading  => RenderHeading(node),
                ElementType.Avatar   => RenderAvatar(),
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

    // ── Bevel helpers ────────────────────────────────────────────────────────

    /// Raised 3-D bevel: white top/left, dark gray bottom/right.
    private static UIElement Raised(UIElement content, Brush? bg = null, Thickness? padding = null)
    {
        var core = new Border
        {
            Background = bg ?? Win95Theme.ButtonFace,
            Padding    = padding ?? new Thickness(Win95Theme.Pad, 4, Win95Theme.Pad, 4),
            Child      = content,
        };
        return new Border
        {
            BorderBrush     = Win95Theme.Highlight,
            BorderThickness = new Thickness(2, 2, 0, 0),
            Child           = new Border
            {
                BorderBrush     = Win95Theme.Shadow,
                BorderThickness = new Thickness(0, 0, 2, 2),
                Child           = core,
            },
        };
    }

    /// Sunken 3-D bevel: dark gray top/left, white bottom/right.
    private static UIElement Sunken(UIElement content, Brush? bg = null, Thickness? padding = null)
    {
        var core = new Border
        {
            Background = bg ?? Win95Theme.InputFill,
            Padding    = padding ?? new Thickness(4, 2, 4, 2),
            Child      = content,
        };
        return new Border
        {
            BorderBrush     = Win95Theme.Shadow,
            BorderThickness = new Thickness(2, 2, 0, 0),
            Child           = new Border
            {
                BorderBrush     = Win95Theme.Highlight,
                BorderThickness = new Thickness(0, 0, 2, 2),
                Child           = core,
            },
        };
    }

    // ── Text helper ─────────────────────────────────────────────────────────

    private static TextBlock MakeText(string text, Brush? fg = null, double size = Win95Theme.FontSize,
        FontWeight? weight = null)
        => new()
        {
            Text         = text,
            FontFamily   = Win95Theme.Font,
            FontSize     = size,
            Foreground   = fg ?? Win95Theme.DarkText,
            FontWeight   = weight ?? FontWeights.Normal,
            TextWrapping = TextWrapping.Wrap,
        };

    private StackPanel RenderChildren(UiNode node, Orientation orientation, double gap = Win95Theme.Pad)
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

    // ── Layout ──────────────────────────────────────────────────────────────

    private UIElement RenderWindow(UiNode node)
    {
        double width = 800;
        foreach (var m in node.Modifiers) { var (w, _) = ParseWxH(m); if (w > 0) width = w; }

        var stack = new StackPanel { Orientation = Orientation.Vertical };

        // Title bar: navy gradient with white title text and classic control buttons
        var titleBar = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromRgb(0x00, 0x00, 0x80),
                Color.FromRgb(0x10, 0x84, 0xD0),
                new Point(0, 0.5), new Point(1, 0.5)),
            Height  = Win95Theme.TitleHeight,
            Padding = new Thickness(4, 0, 4, 0),
        };

        var titleRow = new Grid();
        titleRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        titleRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        titleRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var titleText = new TextBlock
        {
            Text              = node.Label ?? "Window",
            FontFamily        = Win95Theme.Font,
            FontSize          = Win95Theme.FontSize,
            FontWeight        = FontWeights.Bold,
            Foreground        = Win95Theme.TitleBarText,
            VerticalAlignment = VerticalAlignment.Center,
            Padding           = new Thickness(4, 0, 8, 0),
        };

        // Classic Win95 window control buttons
        var btnPanel = new StackPanel
        {
            Orientation       = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
        };
        btnPanel.Children.Add(MakeWinBtn("_"));
        btnPanel.Children.Add(MakeWinBtn("□"));
        btnPanel.Children.Add(MakeWinBtn("×"));

        Grid.SetColumn(titleText, 1);
        Grid.SetColumn(btnPanel, 2);
        titleRow.Children.Add(titleText);
        titleRow.Children.Add(btnPanel);

        titleBar.Child = titleRow;
        stack.Children.Add(titleBar);

        var content = new Border
        {
            Background = Win95Theme.ButtonFace,
            Padding    = new Thickness(Win95Theme.Pad),
            Child      = RenderChildren(node, Orientation.Vertical),
        };
        stack.Children.Add(content);

        // Outer raised border for the whole window
        var outer = Raised(stack, bg: Win95Theme.ButtonFace, padding: new Thickness(0));
        return new Border
        {
            Child               = outer,
            Width               = width,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin              = new Thickness(8),
        };
    }

    private static UIElement MakeWinBtn(string label) =>
        (UIElement)Raised(new TextBlock
        {
            Text              = label,
            FontFamily        = Win95Theme.Font,
            FontSize          = Win95Theme.FontSize,
            Foreground        = Win95Theme.DarkText,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
        }, padding: new Thickness(3, 1, 3, 1));

    private UIElement RenderPanel(UiNode node)
    {
        var content = RenderChildren(node, Orientation.Vertical);
        content.Margin = new Thickness(Win95Theme.Pad);

        if (node.Label == null)
            return (UIElement)Raised(content, padding: new Thickness(0));

        var stack = new StackPanel { Orientation = Orientation.Vertical };
        stack.Children.Add(new Border
        {
            Background      = Win95Theme.ButtonFace,
            BorderBrush     = Win95Theme.Shadow,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding         = new Thickness(Win95Theme.Pad, 4, Win95Theme.Pad, 4),
            Child           = MakeText(node.Label, weight: FontWeights.Bold),
        });
        stack.Children.Add(content);

        return (UIElement)Raised(stack, padding: new Thickness(0));
    }

    private UIElement RenderCard(UiNode node)
    {
        // Classic Win95 group box: sunken border with label
        var inner = new StackPanel { Orientation = Orientation.Vertical };
        foreach (var child in node.Children)
            inner.Children.Add(new Border
            {
                Margin = new Thickness(0, 0, 0, Win95Theme.Pad / 2),
                Child  = RenderNode(child),
            });

        var content = (UIElement)Sunken(inner, bg: Win95Theme.ButtonFace,
            padding: new Thickness(Win95Theme.Pad));

        if (node.Label == null) return content;

        // Wrap in a labeled group box
        var outerStack = new StackPanel { Orientation = Orientation.Vertical };
        outerStack.Children.Add(MakeText(node.Label, weight: FontWeights.Bold));
        outerStack.Children.Add(new Border { Height = 2 });
        outerStack.Children.Add(content);

        return new Border
        {
            Margin = new Thickness(4),
            Child  = outerStack,
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
                Padding = new Thickness(2, 0, 2, 0),
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
            Background      = Win95Theme.ButtonFace,
            BorderBrush     = Win95Theme.Shadow,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Height          = 36,
            Padding         = new Thickness(4, 0, 4, 0),
            Child           = grid,
        };
    }

    private UIElement RenderSidebar(UiNode node)
    {
        double width = 180;
        foreach (var m in node.Modifiers) { var px = ParsePx(m); if (px > 0) width = px; }

        var inner = RenderChildren(node, Orientation.Vertical, 0);
        inner.Margin = new Thickness(4);

        return (UIElement)Raised(inner, padding: new Thickness(0));
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
        var row = new Border
        {
            Background = active ? Win95Theme.SelectFill : Brushes.Transparent,
            Padding    = new Thickness(6, 3, 6, 3),
            Child      = MakeText(node.Label ?? "",
                fg: active ? Win95Theme.SelectText : Win95Theme.DarkText),
        };
        return row;
    }

    private UIElement RenderTabs(UiNode node)
    {
        var strip = new StackPanel { Orientation = Orientation.Horizontal };
        foreach (var child in node.Children) strip.Children.Add(RenderNode(child));
        return new Border
        {
            BorderBrush     = Win95Theme.Shadow,
            BorderThickness = new Thickness(0, 0, 0, 2),
            Child           = strip,
        };
    }

    private UIElement RenderTab(UiNode node)
    {
        bool active = node.HasModifier("active");
        var label = MakeText(node.Label ?? "",
            weight: active ? FontWeights.Bold : FontWeights.Normal);

        return active
            ? (UIElement)Raised(label, padding: new Thickness(12, 5, 12, 5))
            : new Border
            {
                BorderBrush     = Win95Theme.Shadow,
                BorderThickness = new Thickness(1, 1, 1, 0),
                Background      = Win95Theme.ButtonLight,
                Padding         = new Thickness(12, 4, 12, 4),
                Margin          = new Thickness(0, 2, 2, 0),
                Child           = label,
            };
    }

    private UIElement RenderBrand(UiNode node) => new Border
    {
        Padding = new Thickness(0, 0, 12, 0),
        Child   = MakeText(node.Label ?? "Brand", weight: FontWeights.Bold),
    };

    // ── Form controls ────────────────────────────────────────────────────────

    private UIElement RenderField(UiNode node)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical };
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label));
        stack.Children.Add(new Border
        {
            Margin = new Thickness(0, 2, 0, 0),
            Child  = Sunken(MakeText(node.Value ?? "", Win95Theme.DarkText),
                padding: new Thickness(4, 2, 4, 2)),
        });
        return stack;
    }

    private UIElement RenderTextarea(UiNode node)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical };
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label));
        stack.Children.Add(new Border
        {
            Height = 68,
            Margin = new Thickness(0, 2, 0, 0),
            Child  = Sunken(new Border(), bg: Win95Theme.InputFill, padding: new Thickness(4)),
        });
        return stack;
    }

    private UIElement RenderCheckbox(UiNode node)
    {
        bool chk = node.HasModifier("checked");
        var row = new StackPanel { Orientation = Orientation.Horizontal };

        var box = (UIElement)Sunken(
            chk ? (UIElement)new TextBlock
            {
                Text              = "✓",
                FontFamily        = Win95Theme.Font,
                FontSize          = 10,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground        = Win95Theme.DarkText,
            }
            : (UIElement)new Border(),
            bg: Win95Theme.InputFill,
            padding: new Thickness(2));

        row.Children.Add(new Border { Width = 14, Height = 14, Child = box, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0) });
        row.Children.Add(MakeText(node.Label ?? ""));
        return row;
    }

    private UIElement RenderRadio(UiNode node)
    {
        bool chk = node.HasModifier("checked");
        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(new Ellipse
        {
            Width             = 14,
            Height            = 14,
            Stroke            = Win95Theme.Shadow,
            StrokeThickness   = 1,
            Fill              = Win95Theme.InputFill,
            Margin            = new Thickness(0, 0, 5, 0),
            VerticalAlignment = VerticalAlignment.Center,
        });
        row.Children.Add(MakeText(node.Label ?? ""));
        return row;
    }

    private UIElement RenderSelect(UiNode node)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical };
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label));

        var grid = new Grid { Margin = new Thickness(0, 2, 0, 0) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var valBox = (UIElement)Sunken(MakeText(node.Value ?? "", Win95Theme.DarkText),
            padding: new Thickness(4, 2, 4, 2));
        var btnBox = (UIElement)Raised(
            new TextBlock { Text = "▾", FontFamily = Win95Theme.Font, FontSize = Win95Theme.FontSize, Foreground = Win95Theme.DarkText },
            padding: new Thickness(4, 2, 4, 2));

        Grid.SetColumn(valBox, 0);
        Grid.SetColumn(btnBox, 1);
        grid.Children.Add(valBox);
        grid.Children.Add(btnBox);

        stack.Children.Add(grid);
        return stack;
    }

    private UIElement RenderToggle(UiNode node)
    {
        // Win95 has no toggles; render as checkbox
        return RenderCheckbox(node);
    }

    private UIElement RenderSlider(UiNode node)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical };
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label));

        // Classic trackbar: sunken track + raised thumb
        var track = new Grid { Height = 20, Margin = new Thickness(0, 4, 0, 0) };
        track.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        var trackLine = (UIElement)Sunken(new Border(), bg: Win95Theme.ButtonFace, padding: new Thickness(0));
        var thumb = (UIElement)Raised(new Border(), padding: new Thickness(6, 8, 6, 8));

        track.Children.Add(trackLine);
        track.Children.Add(new Border { Width = 12, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(30, 0, 0, 0), Child = thumb });
        stack.Children.Add(track);
        return stack;
    }

    private UIElement RenderButton(UiNode node)
    {
        bool primary  = node.HasModifier("primary");
        bool danger   = node.HasModifier("danger");
        bool disabled = node.HasModifier("disabled");

        var label = new TextBlock
        {
            Text                = node.Label ?? "Button",
            FontFamily          = Win95Theme.Font,
            FontSize            = Win95Theme.FontSize,
            Foreground          = disabled ? Win95Theme.GrayText : Win95Theme.DarkText,
            HorizontalAlignment = HorizontalAlignment.Center,
        };

        // Classic Win95: all buttons look the same (no primary colour)
        return new Border
        {
            Margin = new Thickness(0, 0, 4, 0),
            Child  = Raised(label, bg: Win95Theme.ButtonFace,
                padding: new Thickness(Win95Theme.Pad, 4, Win95Theme.Pad, 4)),
        };
    }

    // ── Display ──────────────────────────────────────────────────────────────

    private UIElement RenderHeading(UiNode node) =>
        MakeText(node.Label ?? "", size: 14, weight: FontWeights.Bold);

    private static UIElement RenderAvatar() => new Rectangle
    {
        Width  = 36,
        Height = 36,
        Fill   = Win95Theme.ButtonLight,
        Stroke = Win95Theme.Shadow, StrokeThickness = 1,
    };

    private UIElement RenderImage(UiNode node)
    {
        double w = 200, h = 150;
        foreach (var m in node.Modifiers) { var (mw, mh) = ParseWxH(m); if (mw > 0) { w = mw; h = mh; } }

        var canvas = new Canvas { Width = w, Height = h };
        canvas.Children.Add(new Rectangle
        {
            Width = w, Height = h,
            Fill = Win95Theme.ButtonLight, Stroke = Win95Theme.Shadow, StrokeThickness = 1,
        });
        canvas.Children.Add(new Line { X1 = 0, Y1 = 0, X2 = w, Y2 = h, Stroke = Win95Theme.Shadow, StrokeThickness = 1 });
        canvas.Children.Add(new Line { X1 = w, Y1 = 0, X2 = 0, Y2 = h, Stroke = Win95Theme.Shadow, StrokeThickness = 1 });
        return canvas;
    }

    private UIElement RenderBadge(UiNode node) => new Border
    {
        Background      = Win95Theme.SelectFill,
        Padding         = new Thickness(6, 1, 6, 1),
        Margin          = new Thickness(0, 0, 4, 0),
        Child           = MakeText(node.Label ?? "", Win95Theme.SelectText, 10),
    };

    private UIElement RenderTag(UiNode node)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(MakeText(node.Label ?? "", size: 10));
        row.Children.Add(MakeText(" ×", Win95Theme.GrayText, 10));
        return (UIElement)Raised(row, padding: new Thickness(4, 1, 4, 1));
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
                var cell = (UIElement)Raised(
                    MakeText(headers[c], weight: FontWeights.Bold),
                    padding: new Thickness(6, 3, 6, 3));
                Grid.SetRow(cell, 0); Grid.SetColumn(cell, c);
                grid.Children.Add(cell);
            }
            rowIdx++;
        }

        foreach (var row in rows)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var cells = row.Label?.Split('|').Select(s => s.Trim()).ToArray() ?? [];
            for (int c = 0; c < Math.Min(cells.Length, colCount); c++)
            {
                var cell = new Border
                {
                    Padding         = new Thickness(6, 3, 6, 3),
                    BorderBrush     = Win95Theme.Shadow,
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    Child           = MakeText(cells[c]),
                };
                Grid.SetRow(cell, rowIdx); Grid.SetColumn(cell, c);
                grid.Children.Add(cell);
            }
            rowIdx++;
        }

        return (UIElement)Sunken(grid, bg: Win95Theme.InputFill, padding: new Thickness(0));
    }

    private UIElement RenderIcon(UiNode node) => (UIElement)Raised(
        new TextBlock
        {
            Text              = node.Label?[..1].ToUpper() ?? "?",
            FontFamily        = Win95Theme.Font,
            FontSize          = 9,
            Foreground        = Win95Theme.DarkText,
            HorizontalAlignment = HorizontalAlignment.Center,
        },
        padding: new Thickness(3, 2, 3, 2));

    // ── Feedback ─────────────────────────────────────────────────────────────

    private UIElement RenderDivider() => (UIElement)Sunken(new Border(), bg: Win95Theme.ButtonFace,
        padding: new Thickness(0, 1, 0, 1));

    private UIElement RenderAlert(UiNode node)
    {
        // Classic Win95 message box inner panel
        var icon = new TextBlock
        {
            Text                = "ℹ",
            FontSize            = 22,
            Foreground          = Win95Theme.TitleBar,
            VerticalAlignment   = VerticalAlignment.Center,
            Margin              = new Thickness(0, 0, 10, 0),
        };
        var text = MakeText(node.Label ?? "");

        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(icon);
        row.Children.Add(text);

        return new Border
        {
            Margin  = new Thickness(0, 4, 0, 4),
            Child   = Raised(row, padding: new Thickness(Win95Theme.Pad)),
        };
    }

    private UIElement RenderToast(UiNode node) => new Border
    {
        HorizontalAlignment = HorizontalAlignment.Right,
        Margin              = new Thickness(0, 4, 0, 4),
        Child               = Raised(MakeText(node.Label ?? ""),
            padding: new Thickness(Win95Theme.Pad, 6, Win95Theme.Pad, 6)),
    };

    private static UIElement RenderSpinner() => new Border
    {
        Width = 22, Height = 22, Margin = new Thickness(4),
        Child = new TextBlock
        {
            Text = "⌛", FontSize = 16, HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center, Foreground = Win95Theme.DarkText,
        },
    };

    private UIElement RenderProgress(UiNode node)
    {
        int pct = 0;
        foreach (var m in node.Modifiers) if (int.TryParse(m, out var v)) pct = v;
        pct = Math.Clamp(pct, 0, 100);

        // Classic Win95 progress: chunked navy blocks
        int blocks = pct / 10;
        var blocksPanel = new StackPanel { Orientation = Orientation.Horizontal };
        for (int i = 0; i < 10; i++)
            blocksPanel.Children.Add(new Border
            {
                Width      = 14,
                Height     = 14,
                Background = i < blocks ? Win95Theme.ProgressFill : Win95Theme.ButtonFace,
                Margin     = new Thickness(1, 1, 1, 1),
            });

        return new Border
        {
            Margin = new Thickness(0, 4, 0, 4),
            Child  = Sunken(blocksPanel, bg: Win95Theme.ButtonFace,
                padding: new Thickness(2)),
        };
    }

    // ── Date / Time ───────────────────────────────────────────────────────────

    private UIElement RenderDatePicker(UiNode node)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical };
        if (node.Label != null) stack.Children.Add(MakeText(node.Label));

        var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 0) };
        row.Children.Add(new Border
        {
            MinWidth = 130,
            Child    = Sunken(MakeText(node.Value ?? "MM/DD/YYYY", Win95Theme.GrayText),
                padding: new Thickness(4, 2, 4, 2)),
        });
        row.Children.Add(new Border
        {
            Margin = new Thickness(2, 0, 0, 0),
            Child  = Raised(MakeText("▾"), padding: new Thickness(6, 2, 6, 2)),
        });

        stack.Children.Add(row);
        return stack;
    }

    private UIElement RenderDateTimePicker(UiNode node)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical };
        if (node.Label != null) stack.Children.Add(MakeText(node.Label));

        var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 0) };
        row.Children.Add(new Border
        {
            MinWidth = 110,
            Child    = Sunken(MakeText("MM/DD/YYYY", Win95Theme.GrayText), padding: new Thickness(4, 2, 4, 2)),
        });
        row.Children.Add(new Border { Margin = new Thickness(2, 0, 0, 0), Child = Raised(MakeText("▾"), padding: new Thickness(4, 2, 4, 2)) });
        row.Children.Add(new Border
        {
            MinWidth = 70,
            Margin   = new Thickness(8, 0, 0, 0),
            Child    = Sunken(MakeText("HH:MM", Win95Theme.GrayText), padding: new Thickness(4, 2, 4, 2)),
        });

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
            Margin              = new Thickness(0, 0, 0, 4),
        };
        header.Children.Add(new Border { Child = Raised(MakeText("◀"), padding: new Thickness(4, 1, 4, 1)) });
        header.Children.Add(MakeText("  April 2024  ", weight: FontWeights.Bold));
        header.Children.Add(new Border { Child = Raised(MakeText("▶"), padding: new Thickness(4, 1, 4, 1)) });
        stack.Children.Add(header);

        var dowGrid = new UniformGrid { Columns = 7, Rows = 1 };
        foreach (var d in new[] { "Su", "Mo", "Tu", "We", "Th", "Fr", "Sa" })
            dowGrid.Children.Add(CalCell(d, Win95Theme.DarkText, FontWeights.Bold));
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
            var tb = CalCell(cell, today ? Win95Theme.SelectText : Win95Theme.DarkText);
            dayGrid.Children.Add(today
                ? new Border { Background = Win95Theme.SelectFill, Child = tb, Margin = new Thickness(1) }
                : tb);
        }

        stack.Children.Add(dayGrid);
        return new Border { Child = Sunken(stack, bg: Win95Theme.InputFill, padding: new Thickness(4)), Margin = new Thickness(4) };
    }

    private static TextBlock CalCell(string text, Brush? fg = null, FontWeight? weight = null) => new()
    {
        Text                = text,
        FontFamily          = Win95Theme.Font,
        FontSize            = Win95Theme.FontSize,
        Foreground          = fg ?? Win95Theme.DarkText,
        FontWeight          = weight ?? FontWeights.Normal,
        TextAlignment       = TextAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        Padding             = new Thickness(2, 3, 2, 3),
        MinWidth            = 24,
    };

    private UIElement RenderPlaceholder(UiNode node) => new Border
    {
        Margin  = new Thickness(2),
        Child   = Raised(MakeText($"[{node.Type}]", Win95Theme.GrayText),
            padding: new Thickness(6, 2, 6, 2)),
    };
}
