using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using Sketchpad.Core.Ast;
using Sketchpad.Core.Rendering;

namespace Sketchpad.Renderers.LoFi;

/// <summary>
/// Lo-fidelity "Sketch" renderer.
/// Greyscale palette, Segoe Print handwriting font, slightly uneven corners,
/// and body text replaced with grey placeholder lines.
/// </summary>
public class LoFiRenderer : IUiRenderer<UIElement>
{
    public string DisplayName => "Sketch";

    // Each node gets a tiny deterministic corner-radius nudge so nothing
    // looks perfectly machine-drawn. Seeded from line number.
    private static CornerRadius Wobbly(int seed, double baseRadius = 3)
    {
        var r = new Random(seed * 1000003); // cheap hash
        double D() => baseRadius + (r.NextDouble() - 0.5) * 2.5;
        return new CornerRadius(D(), D(), D(), D());
    }

    public UIElement Render(UiDocument document)
    {
        var root = new StackPanel
        {
            Orientation         = Orientation.Vertical,
            Background          = LoFiTheme.Page,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        foreach (var node in document.Roots)
            root.Children.Add(RenderNode(node));

        if (document.HasErrors)
        {
            var errPanel = new StackPanel { Margin = new Thickness(8) };
            foreach (var err in document.Errors)
                errPanel.Children.Add(MakeText($"Line {err.Line}: {err.Message}", 11, LoFiTheme.MutedText));
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
                ElementType.Col      => RenderCol(node),
                ElementType.Divider  => RenderDivider(),
                ElementType.Spacer   => new Border { Height = 16 },

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

                ElementType.Label    => RenderLabel(node),
                ElementType.Text     => RenderTextBlock(node),
                ElementType.Heading  => RenderHeading(node),
                ElementType.Avatar   => RenderAvatar(node),
                ElementType.Image    => RenderImage(node),
                ElementType.Badge    => RenderBadge(node),
                ElementType.Tag      => RenderTag(node),
                ElementType.Table    => RenderTable(node),
                ElementType.Icon     => RenderIcon(node),

                ElementType.Alert          => RenderAlert(node),
                ElementType.Toast          => RenderToast(node),
                ElementType.Spinner        => RenderSpinner(),
                ElementType.Progress       => RenderProgress(node),

                ElementType.DatePicker     => RenderDatePicker(node),
                ElementType.DateTimePicker => RenderDateTimePicker(node),
                ElementType.Calendar       => RenderCalendar(node),

                _                          => RenderPlaceholder(node),
            };
        }
        catch
        {
            return RenderPlaceholder(node);
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static TextBlock MakeText(string text, double size = 13,
        Brush? fg = null, FontWeight? weight = null)
        => new()
        {
            Text         = text,
            FontFamily   = LoFiTheme.Font,
            FontSize     = size,
            Foreground   = fg     ?? LoFiTheme.DarkText,
            FontWeight   = weight ?? FontWeights.Normal,
            TextWrapping = TextWrapping.Wrap,
        };

    private Border LoFiBorder(UIElement content, int seed = 0,
        Brush? fill = null, Thickness? padding = null, Thickness? margin = null,
        double strokeWeight = LoFiTheme.StrokeWeight)
        => new()
        {
            Background      = fill   ?? LoFiTheme.ElementFill,
            BorderBrush     = LoFiTheme.Stroke,
            BorderThickness = new Thickness(strokeWeight),
            CornerRadius    = Wobbly(seed),
            Padding         = padding ?? new Thickness(LoFiTheme.Pad),
            Margin          = margin  ?? new Thickness(0),
            Child           = content,
        };

    private StackPanel RenderChildren(UiNode node, Orientation orientation, double gap = LoFiTheme.Gap)
    {
        var panel = new StackPanel { Orientation = orientation };
        bool first = true;
        foreach (var child in node.Children)
        {
            if (!first)
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

    private static int ParsePx(string m) =>
        int.TryParse(m.Replace("px", "").Trim(), out var v) ? v : 0;

    private static (int w, int h) ParseWxH(string m)
    {
        var p = m.Split('x');
        return p.Length == 2 &&
               int.TryParse(p[0], out var w) &&
               int.TryParse(p[1], out var h)
            ? (w, h) : (0, 0);
    }

    // ── Layout ───────────────────────────────────────────────────────────────

    private UIElement RenderWindow(UiNode node)
    {
        double width = 800;
        foreach (var m in node.Modifiers)
        {
            var (w, _) = ParseWxH(m);
            if (w > 0) width = w;
        }

        var stack = new StackPanel { Orientation = Orientation.Vertical };

        // Hand-drawn title bar
        var titleRow = new StackPanel { Orientation = Orientation.Horizontal };
        titleRow.Children.Add(new Ellipse { Width = 9, Height = 9, Stroke = LoFiTheme.Stroke, StrokeThickness = 1.5, Margin = new Thickness(0,0,4,0) });
        titleRow.Children.Add(new Ellipse { Width = 9, Height = 9, Stroke = LoFiTheme.Stroke, StrokeThickness = 1.5, Margin = new Thickness(0,0,4,0) });
        titleRow.Children.Add(new Ellipse { Width = 9, Height = 9, Stroke = LoFiTheme.Stroke, StrokeThickness = 1.5, Margin = new Thickness(0,0,12,0) });
        titleRow.Children.Add(MakeText(node.Label ?? "Window", 11, LoFiTheme.MutedText));

        stack.Children.Add(new Border
        {
            Background      = LoFiTheme.LightFill,
            BorderBrush     = LoFiTheme.Stroke,
            BorderThickness = new Thickness(1.5, 1.5, 1.5, 1),
            Padding         = new Thickness(10, 7, 10, 7),
            Child           = titleRow,
        });
        stack.Children.Add(RenderChildren(node, Orientation.Vertical));

        return new Border
        {
            BorderBrush         = LoFiTheme.Stroke,
            BorderThickness     = new Thickness(1.5),
            Width               = width,
            Child               = stack,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin              = new Thickness(8),
            Background          = LoFiTheme.Page,
        };
    }

    private UIElement RenderPanel(UiNode node)
    {
        var inner = RenderChildren(node, Orientation.Vertical);
        inner.Margin = new Thickness(8);

        if (node.Label == null) return inner;

        var stack = new StackPanel { Orientation = Orientation.Vertical };
        stack.Children.Add(new Border
        {
            BorderBrush     = LoFiTheme.Stroke,
            BorderThickness = new Thickness(0, 0, 0, 1.5),
            Padding         = new Thickness(10, 6, 10, 6),
            Background      = LoFiTheme.LightFill,
            Child           = MakeText(node.Label, 12, weight: FontWeights.Bold),
        });
        stack.Children.Add(inner);

        return new Border
        {
            BorderBrush     = LoFiTheme.Stroke,
            BorderThickness = new Thickness(1.5),
            CornerRadius    = Wobbly(node.Line, 2),
            Margin          = new Thickness(4),
            Child           = stack,
        };
    }

    private UIElement RenderCard(UiNode node)
    {
        var inner = new StackPanel { Orientation = Orientation.Vertical };

        if (node.Label != null)
            inner.Children.Add(new Border
            {
                Padding         = new Thickness(0, 0, 0, 8),
                BorderBrush     = LoFiTheme.Stroke,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Margin          = new Thickness(0, 0, 0, 8),
                Child           = MakeText(node.Label, 13, weight: FontWeights.Bold),
            });

        foreach (var child in node.Children)
            inner.Children.Add(new Border
            {
                Margin = new Thickness(0, 0, 0, LoFiTheme.Gap),
                Child  = RenderNode(child),
            });

        return LoFiBorder(inner, node.Line, margin: new Thickness(4));
    }

    private UIElement RenderRow(UiNode node)
    {
        if (node.HasModifier("vertical"))
            return RenderChildren(node, Orientation.Vertical);

        var grid = new UniformGrid { Rows = 1, Columns = node.Children.Count };
        foreach (var child in node.Children)
            grid.Children.Add(new Border
            {
                Padding = new Thickness(LoFiTheme.PadSmall, 0, LoFiTheme.PadSmall, 0),
                Child   = RenderNode(child),
            });
        return grid;
    }

    private UIElement RenderCol(UiNode node) =>
        RenderChildren(node, Orientation.Vertical);

    private static UIElement RenderDivider() => new Rectangle
    {
        Height              = 1.5,
        Fill                = LoFiTheme.MidFill,
        Margin              = new Thickness(0, 4, 0, 4),
        HorizontalAlignment = HorizontalAlignment.Stretch,
    };

    // ── Navigation ───────────────────────────────────────────────────────────

    private UIElement RenderNavbar(UiNode node)
    {
        var left  = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        var right = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };

        foreach (var child in node.Children)
        {
            if (child.Type == ElementType.Menu && child.HasModifier("right"))
                foreach (var sub in child.Children)
                    right.Children.Add(RenderNode(sub));
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
            Background      = LoFiTheme.LightFill,
            BorderBrush     = LoFiTheme.Stroke,
            BorderThickness = new Thickness(0, 0, 0, 1.5),
            Height          = 48,
            Padding         = new Thickness(12, 0, 12, 0),
            Child           = grid,
        };
    }

    private UIElement RenderSidebar(UiNode node)
    {
        double width = 200;
        foreach (var m in node.Modifiers) { var px = ParsePx(m); if (px > 0) width = px; }

        var inner = RenderChildren(node, Orientation.Vertical, 4);
        inner.Margin = new Thickness(8);

        return new Border
        {
            Width           = width,
            BorderBrush     = LoFiTheme.Stroke,
            BorderThickness = new Thickness(0, 0, 1.5, 0),
            Background      = LoFiTheme.LightFill,
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
            Padding      = new Thickness(8, 5, 8, 5),
            Background   = active ? LoFiTheme.MidFill : Brushes.Transparent,
            CornerRadius = Wobbly(node.Line, 3),
            Margin       = new Thickness(2, 1, 2, 1),
            Child        = MakeText(node.Label ?? "",  13,
                               active ? LoFiTheme.DarkText : LoFiTheme.MutedText,
                               active ? FontWeights.Bold : FontWeights.Normal),
        };
    }

    private UIElement RenderTabs(UiNode node)
    {
        var strip = new StackPanel { Orientation = Orientation.Horizontal };
        foreach (var child in node.Children) strip.Children.Add(RenderNode(child));
        return new Border
        {
            BorderBrush     = LoFiTheme.Stroke,
            BorderThickness = new Thickness(0, 0, 0, 1.5),
            Child           = strip,
        };
    }

    private UIElement RenderTab(UiNode node)
    {
        bool active = node.HasModifier("active");
        return new Border
        {
            Padding         = new Thickness(14, 7, 14, 7),
            BorderBrush     = LoFiTheme.Stroke,
            BorderThickness = new Thickness(0, 0, 0, active ? 2.5 : 0),
            Child           = MakeText(node.Label ?? "", 13,
                                  active ? LoFiTheme.DarkText : LoFiTheme.MutedText,
                                  active ? FontWeights.Bold : FontWeights.Normal),
            Margin = new Thickness(0, 0, 4, 0),
        };
    }

    private UIElement RenderBrand(UiNode node) => new Border
    {
        Padding = new Thickness(0, 0, 16, 0),
        Child   = MakeText(node.Label ?? "Brand", 15, weight: FontWeights.Bold),
    };

    // ── Form ─────────────────────────────────────────────────────────────────

    private UIElement RenderField(UiNode node)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical };
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, 11, LoFiTheme.MutedText));

        stack.Children.Add(new Border
        {
            BorderBrush     = LoFiTheme.Stroke,
            BorderThickness = new Thickness(LoFiTheme.StrokeWeight),
            CornerRadius    = Wobbly(node.Line, 2),
            Background      = LoFiTheme.ElementFill,
            Padding         = new Thickness(8, 5, 8, 5),
            Margin          = new Thickness(0, 2, 0, 0),
            Child           = MakeText(node.Value ?? "", 12, LoFiTheme.MutedText),
        });
        return stack;
    }

    private UIElement RenderTextarea(UiNode node)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical };
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, 11, LoFiTheme.MutedText));

        stack.Children.Add(new Border
        {
            BorderBrush     = LoFiTheme.Stroke,
            BorderThickness = new Thickness(LoFiTheme.StrokeWeight),
            CornerRadius    = Wobbly(node.Line, 2),
            Background      = LoFiTheme.ElementFill,
            Height          = 72,
            Margin          = new Thickness(0, 2, 0, 0),
            Child           = TextPlaceholderLines(3),
        });
        return stack;
    }

    private UIElement RenderCheckbox(UiNode node)
    {
        bool chk = node.HasModifier("checked");
        var row = new StackPanel { Orientation = Orientation.Horizontal };

        var box = new Border
        {
            Width           = 14, Height = 14,
            BorderBrush     = LoFiTheme.Stroke,
            BorderThickness = new Thickness(LoFiTheme.StrokeWeight),
            CornerRadius    = new CornerRadius(2),
            Background      = LoFiTheme.ElementFill,
            VerticalAlignment = VerticalAlignment.Center,
            Margin          = new Thickness(0, 0, 6, 0),
        };
        if (chk) box.Child = MakeText("✓", 10, LoFiTheme.DarkText);

        row.Children.Add(box);
        row.Children.Add(MakeText(node.Label ?? "", 13));
        return row;
    }

    private UIElement RenderRadio(UiNode node)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(new Ellipse
        {
            Width = 14, Height = 14,
            Stroke = LoFiTheme.Stroke, StrokeThickness = LoFiTheme.StrokeWeight,
            Fill = LoFiTheme.ElementFill,
            Margin = new Thickness(0, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center,
        });
        row.Children.Add(MakeText(node.Label ?? "", 13));
        return row;
    }

    private UIElement RenderSelect(UiNode node)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical };
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, 11, LoFiTheme.MutedText));

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var val = MakeText(node.Value ?? "", 12, LoFiTheme.MutedText);
        var chev = MakeText("▾", 12, LoFiTheme.MutedText);
        Grid.SetColumn(val, 0); Grid.SetColumn(chev, 1);
        grid.Children.Add(val); grid.Children.Add(chev);

        stack.Children.Add(new Border
        {
            BorderBrush     = LoFiTheme.Stroke,
            BorderThickness = new Thickness(LoFiTheme.StrokeWeight),
            CornerRadius    = Wobbly(node.Line, 2),
            Background      = LoFiTheme.ElementFill,
            Padding         = new Thickness(8, 5, 8, 5),
            Margin          = new Thickness(0, 2, 0, 0),
            Child           = grid,
        });
        return stack;
    }

    private UIElement RenderToggle(UiNode node)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(new Border
        {
            Width = 32, Height = 18,
            BorderBrush = LoFiTheme.Stroke, BorderThickness = new Thickness(LoFiTheme.StrokeWeight),
            CornerRadius = new CornerRadius(9),
            Background = LoFiTheme.MidFill,
            Margin = new Thickness(0, 0, 8, 0), VerticalAlignment = VerticalAlignment.Center,
        });
        row.Children.Add(MakeText(node.Label ?? "", 13));
        return row;
    }

    private UIElement RenderSlider(UiNode node)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical };
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, 11, LoFiTheme.MutedText));
        stack.Children.Add(new Border
        {
            Height = 3, Background = LoFiTheme.MidFill,
            CornerRadius = new CornerRadius(2), Margin = new Thickness(0, 6, 0, 0),
        });
        return stack;
    }

    private UIElement RenderButton(UiNode node)
    {
        bool primary  = node.HasModifier("primary");
        bool danger   = node.HasModifier("danger");
        bool disabled = node.HasModifier("disabled");

        // Lo-fi: no colour — primary is dark, danger is medium, normal is outline
        Brush bg = primary ? LoFiTheme.DarkFill
                 : danger  ? new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66))
                 : LoFiTheme.ElementFill;
        Brush fg = (primary || danger) && !disabled ? LoFiTheme.White : LoFiTheme.DarkText;

        if (disabled) { bg = LoFiTheme.LightFill; fg = LoFiTheme.MutedText; }

        return new Border
        {
            Background      = bg,
            BorderBrush     = LoFiTheme.Stroke,
            BorderThickness = new Thickness(LoFiTheme.StrokeWeight),
            CornerRadius    = Wobbly(node.Line, 4),
            Padding         = new Thickness(14, 6, 14, 6),
            Margin          = new Thickness(0, 0, 4, 0),
            Child           = new TextBlock
            {
                Text                = node.Label ?? "Button",
                FontFamily          = LoFiTheme.Font,
                FontSize            = 13,
                Foreground          = fg,
                HorizontalAlignment = HorizontalAlignment.Center,
            },
        };
    }

    // ── Display ──────────────────────────────────────────────────────────────

    private UIElement RenderLabel(UiNode node) =>
        MakeText(node.Label ?? "", 12,
            node.HasModifier("muted") ? LoFiTheme.MutedText : LoFiTheme.DarkText);

    /// <summary>
    /// Body text is rendered as grey placeholder lines — the classic lo-fi convention.
    /// Actual words are not shown; the lines convey "there is text here" at a glance.
    /// </summary>
    private UIElement RenderTextBlock(UiNode node)
    {
        // Estimate line count from word count of the label
        int words = (node.Label ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        int lines = Math.Max(1, (int)Math.Ceiling(words / 7.0));
        return TextPlaceholderLines(lines);
    }

    private UIElement RenderHeading(UiNode node) =>
        MakeText(node.Label ?? "", 18, weight: FontWeights.Bold);

    private UIElement RenderAvatar(UiNode node)
    {
        bool circle = node.HasModifier("circle");
        // Draw as an outlined shape with a stick figure head suggestion
        double size = 40;
        if (circle)
        {
            var canvas = new Canvas { Width = size, Height = size, Margin = new Thickness(0,0,0,4) };
            canvas.Children.Add(new Ellipse
            {
                Width = size, Height = size,
                Stroke = LoFiTheme.Stroke, StrokeThickness = LoFiTheme.StrokeWeight,
                Fill = LoFiTheme.LightFill,
            });
            // Simple person icon suggestion: small circle head + lines for body
            canvas.Children.Add(new Ellipse
            {
                Width = 12, Height = 12,
                Stroke = LoFiTheme.Stroke, StrokeThickness = 1,
                Fill = LoFiTheme.LightFill,
                Margin = new Thickness(14, 6, 0, 0),
            });
            return canvas;
        }
        return new Rectangle
        {
            Width = size, Height = size,
            Stroke = LoFiTheme.Stroke, StrokeThickness = LoFiTheme.StrokeWeight,
            Fill = LoFiTheme.LightFill,
            Margin = new Thickness(0, 0, 0, 4),
        };
    }

    private UIElement RenderImage(UiNode node)
    {
        double w = 200, h = 150;
        foreach (var m in node.Modifiers) { var (mw, mh) = ParseWxH(m); if (mw > 0) { w = mw; h = mh; } }

        var canvas = new Canvas { Width = w, Height = h, Background = LoFiTheme.LightFill };
        canvas.Children.Add(new Rectangle { Width = w, Height = h, Stroke = LoFiTheme.Stroke, StrokeThickness = LoFiTheme.StrokeWeight });
        canvas.Children.Add(new Line { X1 = 0, Y1 = 0, X2 = w, Y2 = h, Stroke = LoFiTheme.MidFill, StrokeThickness = 1.5 });
        canvas.Children.Add(new Line { X1 = w, Y1 = 0, X2 = 0, Y2 = h, Stroke = LoFiTheme.MidFill, StrokeThickness = 1.5 });
        if (node.Label != null)
        {
            var lbl = MakeText(node.Label, 10, LoFiTheme.MutedText);
            Canvas.SetLeft(lbl, 4); Canvas.SetTop(lbl, 4);
            canvas.Children.Add(lbl);
        }
        return canvas;
    }

    private UIElement RenderBadge(UiNode node) => new Border
    {
        BorderBrush     = LoFiTheme.Stroke,
        BorderThickness = new Thickness(LoFiTheme.StrokeWeight),
        CornerRadius    = new CornerRadius(10),
        Padding         = new Thickness(7, 2, 7, 2),
        Margin          = new Thickness(0, 0, 4, 0),
        Background      = LoFiTheme.LightFill,
        Child           = MakeText(node.Label ?? "", 10),
    };

    private UIElement RenderTag(UiNode node)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(MakeText(node.Label ?? "", 10));
        row.Children.Add(MakeText(" ×", 10, LoFiTheme.MutedText));
        return new Border
        {
            BorderBrush = LoFiTheme.Stroke, BorderThickness = new Thickness(LoFiTheme.StrokeWeight),
            CornerRadius = new CornerRadius(3), Padding = new Thickness(6, 2, 6, 2),
            Margin = new Thickness(0, 0, 4, 0), Background = LoFiTheme.LightFill, Child = row,
        };
    }

    private UIElement RenderTable(UiNode node)
    {
        var grid = new Grid();
        var colsNode = node.Children.FirstOrDefault(c => c.Type == ElementType.Columns);
        var rows     = node.Children.Where(c => c.Type == ElementType.Row).ToList();
        var headers  = colsNode?.Label?.Split(", ").Select(s => s.Trim()).ToArray() ?? [];
        int colCount = Math.Max(headers.Length, rows.Count > 0 ? rows.Max(r => r.Label?.Split(", ").Length ?? 0) : 0);

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
                    Background = LoFiTheme.LightFill, Padding = new Thickness(8, 5, 8, 5),
                    BorderBrush = LoFiTheme.Stroke, BorderThickness = new Thickness(0, 0, 0, 1.5),
                    Child = MakeText(headers[c], 11, weight: FontWeights.Bold),
                };
                Grid.SetRow(cell, 0); Grid.SetColumn(cell, c);
                grid.Children.Add(cell);
            }
            rowIdx++;
        }

        foreach (var row in rows)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var cells = row.Label?.Split(", ").Select(s => s.Trim()).ToArray() ?? [];
            for (int c = 0; c < Math.Min(cells.Length, colCount); c++)
            {
                var cell = new Border
                {
                    Padding = new Thickness(8, 5, 8, 5),
                    BorderBrush = LoFiTheme.Stroke, BorderThickness = new Thickness(0, 0, 0, 1),
                    Child = MakeText(cells[c], 12),
                };
                Grid.SetRow(cell, rowIdx); Grid.SetColumn(cell, c);
                grid.Children.Add(cell);
            }
            rowIdx++;
        }

        return new Border
        {
            BorderBrush = LoFiTheme.Stroke, BorderThickness = new Thickness(LoFiTheme.StrokeWeight),
            CornerRadius = Wobbly(node.Line, 2), Child = grid, Margin = new Thickness(0, 4, 0, 4),
        };
    }

    private UIElement RenderIcon(UiNode node) => new Border
    {
        Width = 22, Height = 22, Background = LoFiTheme.LightFill,
        BorderBrush = LoFiTheme.Stroke, BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(3), Margin = new Thickness(0, 0, 4, 0),
        Child = MakeText(node.Label?[..1].ToUpper() ?? "?", 10, LoFiTheme.MutedText),
        HorizontalAlignment = HorizontalAlignment.Left,
    };

    // ── Feedback ─────────────────────────────────────────────────────────────

    private UIElement RenderAlert(UiNode node)
    {
        // Lo-fi: all variants look the same — outlined box with left accent bar
        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var accent = new Rectangle { Fill = LoFiTheme.DarkFill };
        var text   = new Border { Padding = new Thickness(10, 7, 10, 7), Child = MakeText(node.Label ?? "", 13) };
        Grid.SetColumn(accent, 0); Grid.SetColumn(text, 1);
        row.Children.Add(accent); row.Children.Add(text);

        return new Border
        {
            Background = LoFiTheme.LightFill, BorderBrush = LoFiTheme.Stroke,
            BorderThickness = new Thickness(LoFiTheme.StrokeWeight),
            CornerRadius = Wobbly(node.Line, 2), ClipToBounds = true,
            Child = row, Margin = new Thickness(0, 4, 0, 4),
        };
    }

    private UIElement RenderToast(UiNode node) => new Border
    {
        Background = LoFiTheme.LightFill, BorderBrush = LoFiTheme.Stroke,
        BorderThickness = new Thickness(LoFiTheme.StrokeWeight),
        CornerRadius = Wobbly(node.Line, 5), Padding = new Thickness(12, 7, 12, 7),
        HorizontalAlignment = HorizontalAlignment.Right,
        Child = MakeText(node.Label ?? "", 13), Margin = new Thickness(0, 4, 0, 4),
    };

    private static UIElement RenderSpinner() => new Ellipse
    {
        Width = 24, Height = 24,
        Stroke = LoFiTheme.Stroke, StrokeThickness = LoFiTheme.StrokeWeight,
        StrokeDashArray = new DoubleCollection([5, 3]), Margin = new Thickness(4),
    };

    private UIElement RenderProgress(UiNode node)
    {
        int pct = 0;
        foreach (var m in node.Modifiers) if (int.TryParse(m, out var v)) pct = v;
        pct = Math.Clamp(pct, 0, 100);

        var track = new Grid { Height = 8 };
        track.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(pct, GridUnitType.Star) });
        track.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100 - pct, GridUnitType.Star) });
        var fill = new Rectangle { Fill = LoFiTheme.DarkFill, RadiusX = 3, RadiusY = 3 };
        Grid.SetColumn(fill, 0);
        track.Children.Add(fill);

        return new Border
        {
            Background = LoFiTheme.MidFill, CornerRadius = Wobbly(node.Line, 3),
            Child = track, Margin = new Thickness(0, 4, 0, 4),
        };
    }

    // ── Date / Time ───────────────────────────────────────────────────────────

    private UIElement RenderDatePicker(UiNode node)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical };
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, 11, LoFiTheme.MutedText));

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin      = new Thickness(0, 2, 0, 0),
        };

        row.Children.Add(new Border
        {
            BorderBrush     = LoFiTheme.Stroke,
            BorderThickness = new Thickness(LoFiTheme.StrokeWeight),
            CornerRadius    = Wobbly(node.Line, 2),
            Background      = LoFiTheme.ElementFill,
            Padding         = new Thickness(8, 5, 8, 5),
            MinWidth        = 140,
            Child           = MakeText(node.Value ?? "MM / DD / YYYY", 12, LoFiTheme.MutedText),
        });

        // Calendar icon button
        row.Children.Add(new Border
        {
            BorderBrush     = LoFiTheme.Stroke,
            BorderThickness = new Thickness(LoFiTheme.StrokeWeight),
            CornerRadius    = Wobbly(node.Line + 1, 2),
            Background      = LoFiTheme.LightFill,
            Padding         = new Thickness(7, 5, 7, 5),
            Margin          = new Thickness(2, 0, 0, 0),
            Child           = MakeText("▦", 12, LoFiTheme.MutedText),
        });

        stack.Children.Add(row);
        return stack;
    }

    private UIElement RenderDateTimePicker(UiNode node)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical };
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, 11, LoFiTheme.MutedText));

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin      = new Thickness(0, 2, 0, 0),
        };

        // Date part
        row.Children.Add(new Border
        {
            BorderBrush     = LoFiTheme.Stroke,
            BorderThickness = new Thickness(LoFiTheme.StrokeWeight),
            CornerRadius    = Wobbly(node.Line, 2),
            Background      = LoFiTheme.ElementFill,
            Padding         = new Thickness(8, 5, 8, 5),
            MinWidth        = 130,
            Child           = MakeText("MM / DD / YYYY", 12, LoFiTheme.MutedText),
        });
        row.Children.Add(new Border
        {
            BorderBrush     = LoFiTheme.Stroke,
            BorderThickness = new Thickness(LoFiTheme.StrokeWeight),
            CornerRadius    = Wobbly(node.Line + 1, 2),
            Background      = LoFiTheme.LightFill,
            Padding         = new Thickness(7, 5, 7, 5),
            Margin          = new Thickness(2, 0, 0, 0),
            Child           = MakeText("▦", 12, LoFiTheme.MutedText),
        });

        // Time part
        row.Children.Add(new Border
        {
            BorderBrush     = LoFiTheme.Stroke,
            BorderThickness = new Thickness(LoFiTheme.StrokeWeight),
            CornerRadius    = Wobbly(node.Line + 2, 2),
            Background      = LoFiTheme.ElementFill,
            Padding         = new Thickness(8, 5, 8, 5),
            Margin          = new Thickness(8, 0, 0, 0),
            MinWidth        = 80,
            Child           = MakeText("HH : MM", 12, LoFiTheme.MutedText),
        });
        row.Children.Add(new Border
        {
            BorderBrush     = LoFiTheme.Stroke,
            BorderThickness = new Thickness(LoFiTheme.StrokeWeight),
            CornerRadius    = Wobbly(node.Line + 3, 2),
            Background      = LoFiTheme.LightFill,
            Padding         = new Thickness(7, 5, 7, 5),
            Margin          = new Thickness(2, 0, 0, 0),
            Child           = MakeText("◷", 12, LoFiTheme.MutedText),
        });

        stack.Children.Add(row);
        return stack;
    }

    private UIElement RenderCalendar(UiNode node)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical };

        // Month navigation header
        var headerRow = new StackPanel
        {
            Orientation         = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin              = new Thickness(0, 0, 0, 6),
        };
        headerRow.Children.Add(MakeText("◀", 12, LoFiTheme.MutedText));
        headerRow.Children.Add(MakeText("  April 2024  ", 13, weight: FontWeights.Bold));
        headerRow.Children.Add(MakeText("▶", 12, LoFiTheme.MutedText));
        stack.Children.Add(headerRow);

        // Day-of-week header row
        var dowGrid = new UniformGrid { Columns = 7, Rows = 1 };
        foreach (var d in new[] { "Su", "Mo", "Tu", "We", "Th", "Fr", "Sa" })
            dowGrid.Children.Add(CalCell(d, LoFiTheme.MutedText, FontWeights.Bold));
        stack.Children.Add(dowGrid);

        // April 2024: starts on Monday (index 1 of Su-Sa), 30 days
        var dayGrid = new UniformGrid { Columns = 7, Rows = 5 };
        var cells = new[]
        {
            "",  "1",  "2",  "3",  "4",  "5",  "6",
            "7", "8",  "9",  "10", "11", "12", "13",
            "14","15", "16", "17", "18", "19", "20",
            "21","22", "23", "24", "25", "26", "27",
            "28","29", "30", "",   "",   "",   "",
        };

        foreach (var cell in cells)
        {
            bool isToday = cell == "15";
            var tb = CalCell(cell,
                isToday ? LoFiTheme.White : LoFiTheme.DarkText,
                FontWeights.Normal);

            dayGrid.Children.Add(isToday
                ? new Border
                  {
                      Background   = LoFiTheme.DarkFill,
                      CornerRadius = new CornerRadius(12),
                      Child        = tb,
                      Margin       = new Thickness(2),
                  }
                : tb);
        }
        stack.Children.Add(dayGrid);

        return LoFiBorder(stack, node.Line, padding: new Thickness(LoFiTheme.Pad));
    }

    private static TextBlock CalCell(string text, Brush? fg = null, FontWeight? weight = null)
        => new()
        {
            Text                = text,
            FontFamily          = LoFiTheme.Font,
            FontSize            = 11,
            Foreground          = fg ?? LoFiTheme.DarkText,
            FontWeight          = weight ?? FontWeights.Normal,
            TextAlignment       = TextAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment   = VerticalAlignment.Center,
            Padding             = new Thickness(2, 4, 2, 4),
            MinWidth            = 28,
        };

    // ── Placeholder ───────────────────────────────────────────────────────────

    private UIElement RenderPlaceholder(UiNode node) => new Border
    {
        BorderBrush = LoFiTheme.Stroke, BorderThickness = new Thickness(1),
        CornerRadius = Wobbly(node.Line), Padding = new Thickness(6, 4, 6, 4),
        Background = LoFiTheme.LightFill, Margin = new Thickness(2),
        Child = MakeText($"[{node.Type}]", 11, LoFiTheme.MutedText),
    };

    // ── Lo-fi specific helpers ────────────────────────────────────────────────

    /// <summary>
    /// Creates a stack of grey horizontal bars representing body text.
    /// Lines alternate between full-width and ~70% width for a natural paragraph look.
    /// </summary>
    private static StackPanel TextPlaceholderLines(int lineCount)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(0, 2, 0, 2) };
        for (int i = 0; i < lineCount; i++)
        {
            bool isShort = (i == lineCount - 1) && lineCount > 1; // last line is shorter
            stack.Children.Add(new Border
            {
                Height              = 7,
                Background          = isShort ? LoFiTheme.LineShort : LoFiTheme.LineFull,
                CornerRadius        = new CornerRadius(2),
                Margin              = new Thickness(0, 0, isShort ? 60 : 0, 4),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            });
        }
        return stack;
    }
}
