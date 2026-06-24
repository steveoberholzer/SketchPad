using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using Sketchpad.Core.Ast;
using Sketchpad.Core.Rendering;

namespace Sketchpad.Renderers.MaterialDesign;

/// <summary>
/// Material Design 3 renderer (Baseline purple scheme).
/// Tonal surfaces (#FFFBFE with purple tint), stadium buttons,
/// outlined text fields, M3 navigation drawer pills, and linear progress.
/// </summary>
public class MaterialRenderer : IUiRenderer<UIElement>
{
    public string DisplayName => "Material Design";

    public UIElement Render(UiDocument document)
    {
        var root = new StackPanel
        {
            Orientation         = Orientation.Vertical,
            Background          = MaterialTheme.Surface,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        foreach (var node in document.Roots)
            root.Children.Add(RenderNode(node));

        if (document.HasErrors)
        {
            var err = new StackPanel { Margin = new Thickness(MaterialTheme.Pad) };
            foreach (var e in document.Errors)
                err.Children.Add(MakeText($"Line {e.Line}: {e.Message}", MaterialTheme.Error));
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
                ElementType.Spacer   => new Border { Height = MaterialTheme.Gap },

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

                ElementType.Label    => MakeText(node.Label ?? "", MaterialTheme.OnSurfaceVariant, MaterialTheme.LabelMedium),
                ElementType.Text     => MakeText(node.Label ?? "", size: MaterialTheme.BodyMedium),
                ElementType.Heading  => MakeText(node.Label ?? "", size: MaterialTheme.TitleLarge, weight: FontWeights.Medium),
                ElementType.Avatar   => RenderAvatar(node),
                ElementType.Image    => RenderImage(node),
                ElementType.Badge    => RenderBadge(node),
                ElementType.Tag      => RenderChip(node),
                ElementType.Table    => RenderTable(node),
                ElementType.Icon     => RenderIcon(node),

                ElementType.Alert    => RenderAlert(node),
                ElementType.Toast    => RenderSnackbar(node),
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

    private static TextBlock MakeText(string text, Brush? fg = null, double size = MaterialTheme.BodyMedium,
        FontWeight? weight = null) => new()
    {
        Text         = text,
        FontFamily   = MaterialTheme.Font,
        FontSize     = size,
        Foreground   = fg ?? MaterialTheme.OnSurface,
        FontWeight   = weight ?? FontWeights.Normal,
        TextWrapping = TextWrapping.Wrap,
    };

    // M3 Outlined text field box
    private static Border OutlinedBox(UIElement content, double? height = null) => new()
    {
        Background      = MaterialTheme.Surface,
        BorderBrush     = MaterialTheme.Outline,
        BorderThickness = new Thickness(1),
        CornerRadius    = new CornerRadius(MaterialTheme.InputRadius),
        Padding         = new Thickness(12, 8, 12, 8),
        Height          = height ?? double.NaN,
        Margin          = new Thickness(0, 4, 0, 0),
        Child           = content,
    };

    private static DropShadowEffect CardShadow() => new()
    {
        BlurRadius  = 8,
        ShadowDepth = 1,
        Opacity     = 0.12,
        Color       = Colors.Black,
        Direction   = 270,
    };

    private StackPanel RenderChildren(UiNode node, Orientation orientation, double gap = -1)
    {
        if (gap < 0) gap = MaterialTheme.Gap;
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

        // Top App Bar
        var appBarGrid = new Grid();
        appBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        appBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        appBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // Navigation icon (hamburger)
        var navIcon = new TextBlock
        {
            Text = "≡", FontFamily = MaterialTheme.Font, FontSize = 22,
            Foreground = MaterialTheme.OnSurface, VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(4, 0, 20, 0),
        };

        var title = new TextBlock
        {
            Text       = node.Label ?? "App",
            FontFamily = MaterialTheme.Font,
            FontSize   = MaterialTheme.TitleLarge,
            FontWeight = FontWeights.Medium,
            Foreground = MaterialTheme.OnSurface,
            VerticalAlignment = VerticalAlignment.Center,
        };

        Grid.SetColumn(navIcon, 0);
        Grid.SetColumn(title, 1);
        appBarGrid.Children.Add(navIcon);
        appBarGrid.Children.Add(title);

        var appBar = new Border
        {
            Background      = MaterialTheme.Surface,
            Height          = MaterialTheme.AppBarHeight,
            Padding         = new Thickness(MaterialTheme.Pad, 0, MaterialTheme.Pad, 0),
            BorderBrush     = MaterialTheme.OutlineVariant,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Child           = appBarGrid,
        };

        stack.Children.Add(appBar);
        stack.Children.Add(new Border
        {
            Background = MaterialTheme.SurfaceContainerLow,
            Padding    = new Thickness(MaterialTheme.Pad),
            Child      = RenderChildren(node, Orientation.Vertical),
        });

        return new Border
        {
            Child               = stack,
            Width               = width,
            Background          = MaterialTheme.Surface,
            CornerRadius        = new CornerRadius(MaterialTheme.CardRadius),
            ClipToBounds        = true,
            BorderBrush         = MaterialTheme.OutlineVariant,
            BorderThickness     = new Thickness(1),
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin              = new Thickness(12),
            Effect              = new DropShadowEffect { BlurRadius = 16, ShadowDepth = 2, Opacity = 0.12, Color = Colors.Black, Direction = 270 },
        };
    }

    private UIElement RenderPanel(UiNode node)
    {
        var content = RenderChildren(node, Orientation.Vertical);
        content.Margin = new Thickness(MaterialTheme.Pad);

        if (node.Label == null) return content;

        var stack = new StackPanel();
        stack.Children.Add(new Border
        {
            Padding = new Thickness(MaterialTheme.Pad, 12, MaterialTheme.Pad, 12),
            Child   = MakeText(node.Label, MaterialTheme.OnSurface, MaterialTheme.TitleMedium, FontWeights.Medium),
        });
        stack.Children.Add(content);

        return new Border
        {
            Child           = stack,
            Background      = MaterialTheme.SurfaceContainer,
            CornerRadius    = new CornerRadius(MaterialTheme.CardRadius),
            Margin          = new Thickness(4),
        };
    }

    private UIElement RenderCard(UiNode node)
    {
        var inner = new StackPanel();

        if (node.Label != null)
            inner.Children.Add(new Border
            {
                Padding         = new Thickness(0, 0, 0, MaterialTheme.Gap),
                BorderBrush     = MaterialTheme.OutlineVariant,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Margin          = new Thickness(0, 0, 0, MaterialTheme.Gap),
                Child           = MakeText(node.Label, MaterialTheme.OnSurface, MaterialTheme.TitleMedium, FontWeights.Medium),
            });

        foreach (var child in node.Children)
            inner.Children.Add(new Border { Margin = new Thickness(0, 3, 0, 3), Child = RenderNode(child) });

        return new Border
        {
            Background      = MaterialTheme.SurfaceContainerHigh,
            CornerRadius    = new CornerRadius(MaterialTheme.CardRadius),
            Padding         = new Thickness(MaterialTheme.Pad),
            Margin          = new Thickness(4),
            Child           = inner,
            Effect          = CardShadow(),
        };
    }

    private UIElement RenderRow(UiNode node)
    {
        if (node.HasModifier("vertical")) return RenderChildren(node, Orientation.Vertical);
        var grid = new UniformGrid { Rows = 1, Columns = node.Children.Count };
        foreach (var child in node.Children)
            grid.Children.Add(new Border
            {
                Padding = new Thickness(MaterialTheme.Gap / 2, 0, MaterialTheme.Gap / 2, 0),
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
                centreTitle = MakeText(child.Label ?? "", size: MaterialTheme.TitleLarge, weight: FontWeights.Medium);
                centreTitle.VerticalAlignment = VerticalAlignment.Center;
            }
            else if (child.Type == ElementType.Menu && child.HasModifier("right"))
            {
                foreach (var sub in child.Children)
                    right.Children.Add(new Border
                    {
                        Padding = new Thickness(8, 6, 8, 6),
                        Child   = MakeText(sub.Label ?? "", MaterialTheme.OnSurface, MaterialTheme.LabelLarge),
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
            Background      = MaterialTheme.Surface,
            Height          = MaterialTheme.AppBarHeight,
            BorderBrush     = MaterialTheme.OutlineVariant,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding         = new Thickness(MaterialTheme.Pad, 0, MaterialTheme.Pad, 0),
            Child           = grid,
        };
    }

    private UIElement RenderSidebar(UiNode node)
    {
        double width = 240;
        foreach (var m in node.Modifiers) { var px = ParsePx(m); if (px > 0) width = px; }

        var inner = RenderChildren(node, Orientation.Vertical, 4);
        inner.Margin = new Thickness(12);

        return new Border
        {
            Width           = width,
            Background      = MaterialTheme.SurfaceContainerLow,
            BorderBrush     = MaterialTheme.OutlineVariant,
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
                Padding = new Thickness(12, 6, 12, 6),
                Child   = MakeText(child.Label ?? "", size: MaterialTheme.LabelLarge),
            });
        return panel;
    }

    private UIElement RenderItem(UiNode node)
    {
        bool active = node.HasModifier("active");
        // M3 navigation drawer item: full-width pill when active
        return new Border
        {
            Background   = active ? MaterialTheme.SecondaryContainer : Brushes.Transparent,
            CornerRadius = new CornerRadius(MaterialTheme.NavPillRadius),
            Padding      = new Thickness(MaterialTheme.Pad, 10, MaterialTheme.Pad, 10),
            Margin       = new Thickness(0, 2, 0, 2),
            Child        = MakeText(node.Label ?? "",
                fg: active ? MaterialTheme.OnSecondaryContainer : MaterialTheme.OnSurfaceVariant,
                size: MaterialTheme.LabelLarge,
                weight: active ? FontWeights.SemiBold : FontWeights.Normal),
        };
    }

    private UIElement RenderTabs(UiNode node)
    {
        var strip = new StackPanel { Orientation = Orientation.Horizontal };
        foreach (var child in node.Children) strip.Children.Add(RenderNode(child));
        return new Border
        {
            Background      = MaterialTheme.Surface,
            BorderBrush     = MaterialTheme.OutlineVariant,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Child           = strip,
        };
    }

    private UIElement RenderTab(UiNode node)
    {
        bool active = node.HasModifier("active");
        return new Border
        {
            Padding         = new Thickness(MaterialTheme.Pad, 14, MaterialTheme.Pad, 14),
            BorderBrush     = MaterialTheme.Primary,
            BorderThickness = new Thickness(0, 0, 0, active ? 3 : 0),
            Margin          = new Thickness(0, 0, 4, 0),
            Child           = MakeText(node.Label ?? "",
                fg: active ? MaterialTheme.Primary : MaterialTheme.OnSurfaceVariant,
                size: MaterialTheme.LabelLarge,
                weight: active ? FontWeights.SemiBold : FontWeights.Medium),
        };
    }

    private UIElement RenderBrand(UiNode node) => new Border
    {
        Padding = new Thickness(0, 0, MaterialTheme.Pad, 0),
        Child   = MakeText(node.Label ?? "App", size: MaterialTheme.TitleLarge, weight: FontWeights.Medium),
    };

    // ── Form ─────────────────────────────────────────────────────────────────

    private UIElement RenderField(UiNode node)
    {
        var stack = new StackPanel();
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, MaterialTheme.OnSurfaceVariant, MaterialTheme.LabelMedium));
        stack.Children.Add(OutlinedBox(MakeText(node.Value ?? "", MaterialTheme.OnSurfaceVariant)));
        return stack;
    }

    private UIElement RenderTextarea(UiNode node)
    {
        var stack = new StackPanel();
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, MaterialTheme.OnSurfaceVariant, MaterialTheme.LabelMedium));
        stack.Children.Add(OutlinedBox(new Border(), height: 80));
        return stack;
    }

    private UIElement RenderCheckbox(UiNode node)
    {
        bool chk = node.HasModifier("checked");
        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(new Border
        {
            Width = 18, Height = 18,
            Background = chk ? MaterialTheme.Primary : Brushes.Transparent,
            BorderBrush = chk ? Brushes.Transparent : MaterialTheme.Outline,
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(2),
            Margin = new Thickness(0, 0, 8, 0), VerticalAlignment = VerticalAlignment.Center,
            Child = chk ? new TextBlock
            {
                Text = "✓", FontSize = 11, FontWeight = FontWeights.Bold,
                Foreground = MaterialTheme.OnPrimary,
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
            } : null,
        });
        row.Children.Add(MakeText(node.Label ?? "", size: MaterialTheme.BodyMedium));
        return row;
    }

    private UIElement RenderRadio(UiNode node)
    {
        bool chk = node.HasModifier("checked");
        var row = new StackPanel { Orientation = Orientation.Horizontal };
        var outer = new Grid { Width = 18, Height = 18, Margin = new Thickness(0, 0, 8, 0), VerticalAlignment = VerticalAlignment.Center };
        outer.Children.Add(new Ellipse { Stroke = chk ? MaterialTheme.Primary : MaterialTheme.Outline, StrokeThickness = 2, Fill = Brushes.Transparent });
        if (chk)
            outer.Children.Add(new Ellipse { Width = 10, Height = 10, Fill = MaterialTheme.Primary, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center });
        row.Children.Add(outer);
        row.Children.Add(MakeText(node.Label ?? "", size: MaterialTheme.BodyMedium));
        return row;
    }

    private UIElement RenderSelect(UiNode node)
    {
        var stack = new StackPanel();
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, MaterialTheme.OnSurfaceVariant, MaterialTheme.LabelMedium));
        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var val  = MakeText(node.Value ?? "", MaterialTheme.OnSurface, MaterialTheme.BodyMedium);
        var chev = MakeText("▾", MaterialTheme.Outline);
        Grid.SetColumn(val, 0); Grid.SetColumn(chev, 1);
        row.Children.Add(val); row.Children.Add(chev);
        stack.Children.Add(OutlinedBox(row));
        return stack;
    }

    private UIElement RenderToggle(UiNode node)
    {
        bool on = node.HasModifier("on") || node.HasModifier("checked");
        var row = new StackPanel { Orientation = Orientation.Horizontal };

        // M3 Switch: 52×32 track, small thumb when off, larger when on
        var knob = new Ellipse
        {
            Width  = on ? 20 : 16,
            Height = on ? 20 : 16,
            Fill   = on ? MaterialTheme.OnPrimary : MaterialTheme.Outline,
            VerticalAlignment   = VerticalAlignment.Center,
            HorizontalAlignment = on ? HorizontalAlignment.Right : HorizontalAlignment.Left,
            Margin              = new Thickness(on ? 0 : 4, 0, on ? 4 : 0, 0),
        };
        var track = new Border
        {
            Width        = 52,
            Height       = 32,
            CornerRadius = new CornerRadius(16),
            Background   = on ? MaterialTheme.Primary : MaterialTheme.SurfaceVariant,
            BorderBrush  = on ? Brushes.Transparent : MaterialTheme.Outline,
            BorderThickness = new Thickness(on ? 0 : 2),
            Margin       = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Child        = knob,
        };

        row.Children.Add(track);
        row.Children.Add(MakeText(node.Label ?? "", size: MaterialTheme.BodyMedium));
        return row;
    }

    private UIElement RenderSlider(UiNode node)
    {
        var stack = new StackPanel();
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, MaterialTheme.OnSurfaceVariant, MaterialTheme.LabelMedium));

        var track = new Grid { Height = 20, Margin = new Thickness(0, 8, 0, 0) };
        track.Children.Add(new Border
        {
            Height = 4, Background = MaterialTheme.SurfaceVariant,
            CornerRadius = new CornerRadius(2), VerticalAlignment = VerticalAlignment.Center,
        });
        track.Children.Add(new Border
        {
            Height = 4, Width = 110, Background = MaterialTheme.Primary,
            CornerRadius = new CornerRadius(2), VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
        });
        // M3 slider thumb: filled circle with value indicator ripple area
        track.Children.Add(new Ellipse
        {
            Width = 20, Height = 20, Fill = MaterialTheme.Primary,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(99, 0, 0, 0),
        });

        stack.Children.Add(track);
        return stack;
    }

    private UIElement RenderButton(UiNode node)
    {
        bool primary  = node.HasModifier("primary");
        bool danger   = node.HasModifier("danger");
        bool tonal    = node.HasModifier("tonal") || node.HasModifier("secondary");
        bool outlined = node.HasModifier("outlined") || node.HasModifier("outline");
        bool disabled = node.HasModifier("disabled");

        // Pick button style: filled > tonal > outlined > text
        if (danger)
        {
            return FilledButton(node.Label ?? "Button", MaterialTheme.Error, MaterialTheme.OnPrimary, disabled);
        }
        if (primary)
        {
            return FilledButton(node.Label ?? "Button", MaterialTheme.Primary, MaterialTheme.OnPrimary, disabled);
        }
        if (tonal)
        {
            return FilledButton(node.Label ?? "Button", MaterialTheme.SecondaryContainer, MaterialTheme.OnSecondaryContainer, disabled);
        }
        if (outlined)
        {
            return OutlinedButton(node.Label ?? "Button", disabled);
        }

        // Default: text / elevated button
        return FilledButton(node.Label ?? "Button", MaterialTheme.SurfaceContainerHigh, MaterialTheme.Primary, disabled,
            shadow: true);
    }

    private UIElement FilledButton(string label, Brush bg, Brush fg, bool disabled, bool shadow = false)
    {
        Brush actualBg = disabled ? MaterialTheme.SurfaceVariant : bg;
        Brush actualFg = disabled ? MaterialTheme.Outline : fg;
        var border = new Border
        {
            Background   = actualBg,
            CornerRadius = new CornerRadius(MaterialTheme.ButtonRadius),
            Padding      = new Thickness(24, 10, 24, 10),
            Margin       = new Thickness(0, 0, MaterialTheme.Gap, 0),
            Child        = new TextBlock
            {
                Text = label, FontFamily = MaterialTheme.Font,
                FontSize = MaterialTheme.LabelLarge, FontWeight = FontWeights.Medium,
                Foreground = actualFg, HorizontalAlignment = HorizontalAlignment.Center,
            },
        };
        if (shadow)
            border.Effect = new DropShadowEffect { BlurRadius = 6, ShadowDepth = 1, Opacity = 0.2, Color = Colors.Black, Direction = 270 };
        return border;
    }

    private UIElement OutlinedButton(string label, bool disabled)
    {
        Brush fg  = disabled ? MaterialTheme.Outline : MaterialTheme.Primary;
        Brush bdr = disabled ? MaterialTheme.OutlineVariant : MaterialTheme.Outline;
        return new Border
        {
            Background      = Brushes.Transparent,
            BorderBrush     = bdr,
            BorderThickness = new Thickness(1),
            CornerRadius    = new CornerRadius(MaterialTheme.ButtonRadius),
            Padding         = new Thickness(24, 10, 24, 10),
            Margin          = new Thickness(0, 0, MaterialTheme.Gap, 0),
            Child           = new TextBlock
            {
                Text = label, FontFamily = MaterialTheme.Font,
                FontSize = MaterialTheme.LabelLarge, FontWeight = FontWeights.Medium,
                Foreground = fg, HorizontalAlignment = HorizontalAlignment.Center,
            },
        };
    }

    // ── Display ──────────────────────────────────────────────────────────────

    private UIElement RenderAvatar(UiNode node)
    {
        bool circle = node.HasModifier("circle");
        int size = 40;
        return circle
            ? (UIElement)new Ellipse { Width = size, Height = size, Fill = MaterialTheme.PrimaryContainer }
            : new Rectangle
            {
                Width = size, Height = size, Fill = MaterialTheme.PrimaryContainer,
                RadiusX = MaterialTheme.CardRadius / 2, RadiusY = MaterialTheme.CardRadius / 2,
            };
    }

    private UIElement RenderImage(UiNode node)
    {
        double w = 200, h = 150;
        foreach (var m in node.Modifiers) { var (mw, mh) = ParseWxH(m); if (mw > 0) { w = mw; h = mh; } }
        var canvas = new Canvas { Width = w, Height = h };
        canvas.Children.Add(new Rectangle
        {
            Width = w, Height = h, Fill = MaterialTheme.SurfaceContainerHigh,
            RadiusX = MaterialTheme.CardRadius, RadiusY = MaterialTheme.CardRadius,
        });
        canvas.Children.Add(new Line { X1 = 0, Y1 = 0, X2 = w, Y2 = h, Stroke = MaterialTheme.OutlineVariant, StrokeThickness = 1 });
        canvas.Children.Add(new Line { X1 = w, Y1 = 0, X2 = 0, Y2 = h, Stroke = MaterialTheme.OutlineVariant, StrokeThickness = 1 });
        return canvas;
    }

    private UIElement RenderBadge(UiNode node) => new Border
    {
        Background   = MaterialTheme.Error,
        CornerRadius = new CornerRadius(10),
        Padding      = new Thickness(6, 2, 6, 2),
        Margin       = new Thickness(0, 0, 4, 0),
        HorizontalAlignment = HorizontalAlignment.Left,
        Child        = MakeText(node.Label ?? "", MaterialTheme.OnPrimary, MaterialTheme.LabelSmall),
    };

    // M3 tags render as Chips
    private UIElement RenderChip(UiNode node) => new Border
    {
        Background      = MaterialTheme.SurfaceVariant,
        BorderBrush     = MaterialTheme.OutlineVariant,
        BorderThickness = new Thickness(1),
        CornerRadius    = new CornerRadius(MaterialTheme.ChipRadius),
        Padding         = new Thickness(12, 6, 12, 6),
        Margin          = new Thickness(0, 0, 4, 0),
        HorizontalAlignment = HorizontalAlignment.Left,
        Child           = MakeText(node.Label ?? "", MaterialTheme.OnSurfaceVariant, MaterialTheme.LabelLarge),
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
                    Background = MaterialTheme.SurfaceContainerHigh,
                    Padding = new Thickness(MaterialTheme.Pad, 10, MaterialTheme.Pad, 10),
                    BorderBrush = MaterialTheme.OutlineVariant, BorderThickness = new Thickness(0, 0, 0, 1),
                    Child = MakeText(headers[c], MaterialTheme.OnSurfaceVariant, MaterialTheme.LabelLarge, FontWeights.SemiBold),
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
                    Background = alt ? MaterialTheme.SurfaceContainerLow : MaterialTheme.Surface,
                    Padding = new Thickness(MaterialTheme.Pad, 10, MaterialTheme.Pad, 10),
                    BorderBrush = MaterialTheme.OutlineVariant, BorderThickness = new Thickness(0, 0, 0, 1),
                    Child = MakeText(cells[c], size: MaterialTheme.BodyMedium),
                };
                Grid.SetRow(cell, rowIdx); Grid.SetColumn(cell, c); grid.Children.Add(cell);
            }
            rowIdx++; alt = !alt;
        }

        return new Border
        {
            BorderBrush = MaterialTheme.OutlineVariant, BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(MaterialTheme.CardRadius), ClipToBounds = true,
            Margin = new Thickness(0, 4, 0, 4), Child = grid,
        };
    }

    private UIElement RenderIcon(UiNode node) => new Border
    {
        Width = 40, Height = 40,
        Background = MaterialTheme.PrimaryContainer,
        CornerRadius = new CornerRadius(MaterialTheme.CardRadius),
        Margin = new Thickness(0, 0, 8, 0), HorizontalAlignment = HorizontalAlignment.Left,
        Child = MakeText(node.Label?[..1].ToUpper() ?? "?", MaterialTheme.OnPrimaryContainer, MaterialTheme.TitleMedium),
    };

    // ── Feedback ─────────────────────────────────────────────────────────────

    private static UIElement RenderDivider() => new Border
    {
        Height              = 1,
        Background          = MaterialTheme.OutlineVariant,
        Margin              = new Thickness(0, 4, 0, 4),
        HorizontalAlignment = HorizontalAlignment.Stretch,
    };

    private UIElement RenderAlert(UiNode node)
    {
        bool warning = node.HasModifier("warning");
        bool danger  = node.HasModifier("danger") || node.HasModifier("error");
        bool success = node.HasModifier("success");

        Brush bg = warning ? MaterialTheme.WarningContainer
                 : danger  ? MaterialTheme.ErrorContainer
                 : success ? MaterialTheme.SuccessContainer
                 : MaterialTheme.PrimaryContainer;

        Brush fg = danger ? MaterialTheme.OnErrorContainer : MaterialTheme.OnPrimaryContainer;

        return new Border
        {
            Background = bg, CornerRadius = new CornerRadius(MaterialTheme.CardRadius),
            Padding = new Thickness(MaterialTheme.Pad, 12, MaterialTheme.Pad, 12),
            Margin = new Thickness(0, 4, 0, 4),
            Child = MakeText(node.Label ?? "", fg, MaterialTheme.BodyMedium),
        };
    }

    // M3 Snackbar — dark pill, centered
    private UIElement RenderSnackbar(UiNode node) => new Border
    {
        Background          = MaterialTheme.InverseSurface,
        CornerRadius        = new CornerRadius(4),
        Padding             = new Thickness(MaterialTheme.Pad, 14, MaterialTheme.Pad, 14),
        Margin              = new Thickness(MaterialTheme.Pad * 2, 4, MaterialTheme.Pad * 2, 4),
        HorizontalAlignment = HorizontalAlignment.Center,
        Child               = MakeText(node.Label ?? "", MaterialTheme.InverseOnSurface, MaterialTheme.BodyMedium),
    };

    private static UIElement RenderSpinner() => new Ellipse
    {
        Width = 24, Height = 24, Stroke = MaterialTheme.Primary, StrokeThickness = 3,
        StrokeDashArray = new DoubleCollection([5, 3]), Margin = new Thickness(4),
    };

    private UIElement RenderProgress(UiNode node)
    {
        int pct = 0;
        foreach (var m in node.Modifiers) if (int.TryParse(m, out var v)) pct = v;
        pct = Math.Clamp(pct, 0, 100);

        // M3 Linear Progress Indicator: 4dp track
        var track = new Grid { Height = 4 };
        track.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(pct, GridUnitType.Star) });
        track.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100 - pct, GridUnitType.Star) });
        var fill = new Rectangle { Fill = MaterialTheme.Primary };
        Grid.SetColumn(fill, 0);
        track.Children.Add(fill);

        return new Border
        {
            Background = MaterialTheme.SurfaceVariant,
            Child      = track,
            Margin     = new Thickness(0, 4, 0, 4),
        };
    }

    // ── Date / Time ───────────────────────────────────────────────────────────

    private UIElement RenderDatePicker(UiNode node)
    {
        var stack = new StackPanel();
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, MaterialTheme.OnSurfaceVariant, MaterialTheme.LabelMedium));
        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var val = MakeText(node.Value ?? "MM / DD / YYYY", MaterialTheme.OnSurfaceVariant, MaterialTheme.BodyMedium);
        var ico = MakeText("▦", MaterialTheme.Outline);
        Grid.SetColumn(val, 0); Grid.SetColumn(ico, 1);
        row.Children.Add(val); row.Children.Add(ico);
        stack.Children.Add(OutlinedBox(row));
        return stack;
    }

    private UIElement RenderDateTimePicker(UiNode node)
    {
        var stack = new StackPanel();
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, MaterialTheme.OnSurfaceVariant, MaterialTheme.LabelMedium));
        var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 0) };
        row.Children.Add(new Border { MinWidth = 140, Child = OutlinedBox(MakeText("MM / DD / YYYY", MaterialTheme.OnSurfaceVariant, MaterialTheme.BodyMedium)) });
        row.Children.Add(new Border { Width = MaterialTheme.Gap });
        row.Children.Add(new Border { MinWidth = 90, Child = OutlinedBox(MakeText("HH : MM", MaterialTheme.OnSurfaceVariant, MaterialTheme.BodyMedium)) });
        stack.Children.Add(row);
        return stack;
    }

    private UIElement RenderCalendar(UiNode node)
    {
        var stack = new StackPanel();

        var header = new StackPanel
        {
            Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, MaterialTheme.Gap),
        };
        header.Children.Add(new Border
        {
            Background = MaterialTheme.SurfaceContainerHigh, CornerRadius = new CornerRadius(MaterialTheme.CardRadius),
            Padding = new Thickness(8, 4, 8, 4), Margin = new Thickness(0, 0, 12, 0),
            Child = MakeText("◀", MaterialTheme.OnSurfaceVariant),
        });
        header.Children.Add(MakeText("April 2024", size: MaterialTheme.TitleMedium, weight: FontWeights.Medium));
        header.Children.Add(new Border
        {
            Background = MaterialTheme.SurfaceContainerHigh, CornerRadius = new CornerRadius(MaterialTheme.CardRadius),
            Padding = new Thickness(8, 4, 8, 4), Margin = new Thickness(12, 0, 0, 0),
            Child = MakeText("▶", MaterialTheme.OnSurfaceVariant),
        });
        stack.Children.Add(header);

        var dowGrid = new UniformGrid { Columns = 7, Rows = 1 };
        foreach (var d in new[] { "Su", "Mo", "Tu", "We", "Th", "Fr", "Sa" })
            dowGrid.Children.Add(new TextBlock
            {
                Text = d, FontFamily = MaterialTheme.Font, FontSize = MaterialTheme.LabelLarge,
                Foreground = MaterialTheme.OnSurfaceVariant, TextAlignment = TextAlignment.Center,
                Padding = new Thickness(0, 2, 0, 6),
            });
        stack.Children.Add(dowGrid);

        var dayGrid = new UniformGrid { Columns = 7, Rows = 5 };
        var cells = new[] { "", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "", "", "", "" };
        foreach (var cell in cells)
        {
            bool today = cell == "15";
            var tb = new TextBlock
            {
                Text = cell, FontFamily = MaterialTheme.Font, FontSize = MaterialTheme.BodyMedium,
                Foreground = today ? MaterialTheme.OnPrimary : MaterialTheme.OnSurface,
                TextAlignment = TextAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center, Padding = new Thickness(2, 5, 2, 5), MinWidth = 32,
            };
            dayGrid.Children.Add(today
                ? new Border
                  {
                      Background = MaterialTheme.Primary, CornerRadius = new CornerRadius(16),
                      Margin = new Thickness(2), Child = tb,
                  }
                : tb);
        }
        stack.Children.Add(dayGrid);

        return new Border
        {
            Background = MaterialTheme.Surface, CornerRadius = new CornerRadius(MaterialTheme.CardRadius),
            BorderBrush = MaterialTheme.OutlineVariant, BorderThickness = new Thickness(1),
            Padding = new Thickness(MaterialTheme.Pad), Margin = new Thickness(4),
            Child = stack, Effect = CardShadow(),
        };
    }

    private UIElement RenderPlaceholder(UiNode node) => new Border
    {
        Background = MaterialTheme.SurfaceContainerHigh,
        BorderBrush = MaterialTheme.OutlineVariant, BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(MaterialTheme.ChipRadius),
        Padding = new Thickness(MaterialTheme.Gap, 4, MaterialTheme.Gap, 4), Margin = new Thickness(2),
        Child = MakeText($"[{node.Type}]", MaterialTheme.OnSurfaceVariant, MaterialTheme.LabelMedium),
    };

}
