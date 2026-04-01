using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using Sketchpad.Core.Ast;
using Sketchpad.Core.Rendering;

namespace Sketchpad.Renderers.Sketch;

public class SketchRenderer : IUiRenderer<UIElement>
{
    public string DisplayName => "Realistic";

    public UIElement Render(UiDocument document)
    {
        var root = new StackPanel
        {
            Orientation       = Orientation.Vertical,
            Background        = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        foreach (var node in document.Roots)
            root.Children.Add(RenderNode(node, isHorizontal: false));

        if (document.HasErrors)
        {
            var errPanel = new StackPanel { Margin = new Thickness(8) };
            foreach (var err in document.Errors)
            {
                errPanel.Children.Add(new TextBlock
                {
                    Text       = $"Line {err.Line}: {err.Message}",
                    Foreground = SketchTheme.DangerFill,
                    FontFamily = SketchTheme.BodyFont,
                    FontSize   = 11,
                });
            }
            root.Children.Insert(0, errPanel);
        }

        return root;
    }

    private UIElement RenderNode(UiNode node, bool isHorizontal = false)
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
                ElementType.Spacer   => RenderSpacer(),

                ElementType.Navbar   => RenderNavbar(node),
                ElementType.Sidebar  => RenderSidebar(node),
                ElementType.Menu     => RenderMenu(node),
                ElementType.Nav      => RenderNav(node),
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
                ElementType.Text     => RenderText(node),
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

    // ── Helpers ─────────────────────────────────────────────────────────────

    private StackPanel ChildrenPanel(UiNode node, Orientation orientation, double gap = SketchTheme.GapMedium)
    {
        var panel = new StackPanel
        {
            Orientation = orientation,
        };
        bool first = true;
        foreach (var child in node.Children)
        {
            if (!first)
            {
                panel.Children.Add(new Border
                {
                    Width  = orientation == Orientation.Horizontal ? gap : double.NaN,
                    Height = orientation == Orientation.Vertical   ? gap : double.NaN,
                });
            }
            first = false;
            panel.Children.Add(RenderNode(child, isHorizontal: orientation == Orientation.Horizontal));
        }
        return panel;
    }

    private Border SketchBorder(UIElement content, double cornerRadius = 3, double borderThickness = 1,
        Brush? background = null, Brush? borderBrush = null, Thickness? padding = null, Thickness? margin = null)
    {
        var border = new Border
        {
            CornerRadius    = new CornerRadius(cornerRadius),
            BorderThickness = new Thickness(borderThickness),
            BorderBrush     = borderBrush ?? SketchTheme.Border,
            Background      = background ?? SketchTheme.Background,
            Padding         = padding ?? new Thickness(SketchTheme.PadCard),
            Child           = content,
        };
        if (margin.HasValue) border.Margin = margin.Value;
        return border;
    }

    private TextBlock MakeText(string text, double fontSize = 13, Brush? foreground = null,
        FontWeight? weight = null, FontFamily? family = null)
    {
        return new TextBlock
        {
            Text            = text,
            FontFamily      = family ?? SketchTheme.BodyFont,
            FontSize        = fontSize,
            Foreground      = foreground ?? SketchTheme.DarkText,
            FontWeight      = weight ?? FontWeights.Normal,
            TextWrapping    = TextWrapping.Wrap,
        };
    }

    private int ParsePx(string modifier)
    {
        var clean = modifier.Replace("px", "").Trim();
        return int.TryParse(clean, out var val) ? val : 0;
    }

    private (int w, int h) ParseWxH(string modifier)
    {
        var parts = modifier.Split('x');
        if (parts.Length == 2 &&
            int.TryParse(parts[0], out var w) &&
            int.TryParse(parts[1], out var h))
            return (w, h);
        return (0, 0);
    }

    // ── Layout ───────────────────────────────────────────────────────────────

    private UIElement RenderWindow(UiNode node)
    {
        double width = 800, height = 600;
        foreach (var m in node.Modifiers)
        {
            var (w, h) = ParseWxH(m);
            if (w > 0) { width = w; height = h; }
        }

        var outerStack = new StackPanel { Orientation = Orientation.Vertical };

        // Title bar
        var titleBar = new Border
        {
            Background      = SketchTheme.NavbarFill,
            BorderBrush     = SketchTheme.Border,
            BorderThickness = new Thickness(1),
            Padding         = new Thickness(12, 8, 12, 8),
        };
        var titleRow = new StackPanel { Orientation = Orientation.Horizontal };
        titleRow.Children.Add(new Ellipse { Width = 10, Height = 10, Fill = Brushes.LightCoral, Margin = new Thickness(0,0,4,0) });
        titleRow.Children.Add(new Ellipse { Width = 10, Height = 10, Fill = Brushes.LightYellow, Margin = new Thickness(0,0,4,0) });
        titleRow.Children.Add(new Ellipse { Width = 10, Height = 10, Fill = Brushes.LightGreen, Margin = new Thickness(0,0,12,0) });
        titleRow.Children.Add(MakeText(node.Label ?? "Window", 12, SketchTheme.MutedText));
        titleBar.Child = titleRow;
        outerStack.Children.Add(titleBar);

        // Content
        var contentPanel = ChildrenPanel(node, Orientation.Vertical);
        contentPanel.Margin = new Thickness(0);

        outerStack.Children.Add(contentPanel);

        return new Border
        {
            BorderBrush     = SketchTheme.Border,
            BorderThickness = new Thickness(1),
            Width           = width,
            Child           = outerStack,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin          = new Thickness(8),
        };
    }

    private UIElement RenderPanel(UiNode node)
    {
        var inner = ChildrenPanel(node, Orientation.Vertical, SketchTheme.GapMedium);

        if (node.Label != null)
        {
            var stack = new StackPanel { Orientation = Orientation.Vertical };
            var titleBar = new Border
            {
                Background      = SketchTheme.NavbarFill,
                BorderBrush     = SketchTheme.Border,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding         = new Thickness(12, 6, 12, 6),
                Child           = MakeText(node.Label, 12, SketchTheme.DarkText, FontWeights.SemiBold),
            };
            stack.Children.Add(titleBar);
            inner.Margin = new Thickness(8);
            stack.Children.Add(inner);
            return new Border
            {
                BorderBrush     = SketchTheme.Border,
                BorderThickness = new Thickness(1),
                Margin          = new Thickness(4),
                Child           = stack,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
        }

        inner.Margin = new Thickness(8);
        return inner;
    }

    private UIElement RenderCard(UiNode node)
    {
        bool isWarning = node.HasModifier("warning");
        bool isDanger  = node.HasModifier("danger");

        var titleStack = new StackPanel { Orientation = Orientation.Vertical };

        if (node.Label != null)
        {
            var titleBlock = MakeText(node.Label, 13, SketchTheme.DarkText, FontWeights.SemiBold);
            titleStack.Children.Add(new Border
            {
                Padding         = new Thickness(0, 0, 0, 8),
                BorderBrush     = SketchTheme.Border,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Child           = titleBlock,
                Margin          = new Thickness(0, 0, 0, 8),
            });
        }

        foreach (var child in node.Children)
            titleStack.Children.Add(new Border { Margin = new Thickness(0, 0, 0, SketchTheme.GapMedium), Child = RenderNode(child) });

        return SketchBorder(
            titleStack,
            cornerRadius: 4,
            background:   isWarning ? SketchTheme.WarningFill : isDanger ? new SolidColorBrush(Color.FromRgb(0xFF, 0xEB, 0xEB)) : SketchTheme.Background,
            borderBrush:  isWarning ? SketchTheme.WarningBorder : isDanger ? SketchTheme.DangerFill : SketchTheme.Border,
            margin:       new Thickness(4)
        );
    }

    private UIElement RenderRow(UiNode node)
    {
        bool isVertical = node.HasModifier("vertical");
        var orientation = isVertical ? Orientation.Vertical : Orientation.Horizontal;
        var panel = new StackPanel
        {
            Orientation = orientation,
            Margin      = new Thickness(0, 0, 0, 0),
        };

        if (orientation == Orientation.Horizontal)
        {
            // Equal-width children using UniformGrid
            var grid = new UniformGrid
            {
                Rows    = 1,
                Columns = node.Children.Count,
            };
            foreach (var child in node.Children)
            {
                var wrapper = new Border { Padding = new Thickness(SketchTheme.PadRow, 0, SketchTheme.PadRow, 0) };
                wrapper.Child = RenderNode(child, isHorizontal: true);
                grid.Children.Add(wrapper);
            }
            return grid;
        }
        else
        {
            foreach (var child in node.Children)
                panel.Children.Add(RenderNode(child));
            return panel;
        }
    }

    private UIElement RenderCol(UiNode node) => ChildrenPanel(node, Orientation.Vertical);

    private UIElement RenderDivider() => new Rectangle
    {
        Height     = 1,
        Fill       = SketchTheme.Border,
        Margin     = new Thickness(0, 4, 0, 4),
        HorizontalAlignment = HorizontalAlignment.Stretch,
    };

    private UIElement RenderSpacer() => new Border { Height = 16 };

    // ── Navigation ───────────────────────────────────────────────────────────

    private UIElement RenderNavbar(UiNode node)
    {
        var leftItems  = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        var rightItems = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };

        foreach (var child in node.Children)
        {
            if (child.Type == ElementType.Menu && child.HasModifier("right"))
            {
                foreach (var sub in child.Children)
                    rightItems.Children.Add(RenderNode(sub));
            }
            else
            {
                leftItems.Children.Add(RenderNode(child));
            }
        }

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        Grid.SetColumn(leftItems, 0);
        Grid.SetColumn(rightItems, 2);

        grid.Children.Add(leftItems);
        grid.Children.Add(rightItems);

        return new Border
        {
            Background      = SketchTheme.NavbarFill,
            BorderBrush     = SketchTheme.Border,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Height          = 48,
            Padding         = new Thickness(12, 0, 12, 0),
            Child           = grid,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
    }

    private UIElement RenderSidebar(UiNode node)
    {
        double width = 200;
        foreach (var m in node.Modifiers)
        {
            var px = ParsePx(m);
            if (px > 0) width = px;
        }

        var inner = ChildrenPanel(node, Orientation.Vertical, SketchTheme.GapSmall);
        inner.Margin = new Thickness(8);

        return new Border
        {
            Width           = width,
            BorderBrush     = SketchTheme.Border,
            BorderThickness = new Thickness(0, 0, 1, 0),
            Background      = SketchTheme.Background,
            Child           = inner,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
    }

    private UIElement RenderMenu(UiNode node)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal };
        foreach (var child in node.Children)
            panel.Children.Add(RenderNode(child));
        return panel;
    }

    private UIElement RenderNav(UiNode node) => ChildrenPanel(node, Orientation.Vertical, 2);

    private UIElement RenderItem(UiNode node)
    {
        bool isActive = node.HasModifier("active");
        var text = MakeText(node.Label ?? "", 13,
            isActive ? SketchTheme.DarkText : SketchTheme.MutedText,
            isActive ? FontWeights.SemiBold : FontWeights.Normal);

        return new Border
        {
            Padding         = new Thickness(8, 6, 8, 6),
            Background      = isActive ? new SolidColorBrush(Color.FromRgb(0xE8, 0xE8, 0xE8)) : Brushes.Transparent,
            CornerRadius    = new CornerRadius(4),
            Child           = text,
            Margin          = new Thickness(2, 1, 2, 1),
        };
    }

    private UIElement RenderTabs(UiNode node)
    {
        var tabStrip = new StackPanel { Orientation = Orientation.Horizontal };
        foreach (var child in node.Children)
            tabStrip.Children.Add(RenderNode(child));

        return new Border
        {
            BorderBrush     = SketchTheme.Border,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Child           = tabStrip,
        };
    }

    private UIElement RenderTab(UiNode node)
    {
        bool isActive = node.HasModifier("active");
        return new Border
        {
            Padding         = new Thickness(16, 8, 16, 8),
            BorderBrush     = isActive ? SketchTheme.DarkText : Brushes.Transparent,
            BorderThickness = new Thickness(0, 0, 0, isActive ? 2 : 0),
            Child           = MakeText(node.Label ?? "", 13,
                                 isActive ? SketchTheme.DarkText : SketchTheme.MutedText,
                                 isActive ? FontWeights.SemiBold : FontWeights.Normal),
            Margin          = new Thickness(0, 0, 4, 0),
        };
    }

    private UIElement RenderBrand(UiNode node) => new Border
    {
        Padding = new Thickness(0, 0, 16, 0),
        Child   = MakeText(node.Label ?? "Brand", 15, SketchTheme.DarkText, FontWeights.Bold),
    };

    // ── Form ─────────────────────────────────────────────────────────────────

    private UIElement RenderField(UiNode node)
    {
        bool isWide = node.HasModifier("wide");
        var stack = new StackPanel { Orientation = Orientation.Vertical };

        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, 11, SketchTheme.MutedText));

        var inputBorder = new Border
        {
            BorderBrush     = SketchTheme.Border,
            BorderThickness = new Thickness(1),
            CornerRadius    = new CornerRadius(3),
            Background      = SketchTheme.InputBackground,
            Padding         = new Thickness(8, 5, 8, 5),
            Margin          = new Thickness(0, 2, 0, 0),
            Child           = MakeText(node.Value ?? "", 13, SketchTheme.MutedText, family: SketchTheme.CodeFont),
        };
        stack.Children.Add(inputBorder);

        if (isWide)
            stack.HorizontalAlignment = HorizontalAlignment.Stretch;

        return new Border { Margin = new Thickness(0, 0, 0, 0), Child = stack };
    }

    private UIElement RenderTextarea(UiNode node)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical };
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, 11, SketchTheme.MutedText));

        stack.Children.Add(new Border
        {
            BorderBrush     = SketchTheme.Border,
            BorderThickness = new Thickness(1),
            CornerRadius    = new CornerRadius(3),
            Background      = SketchTheme.InputBackground,
            Height          = 80,
            Padding         = new Thickness(8, 5, 8, 5),
            Margin          = new Thickness(0, 2, 0, 0),
            Child           = MakeText(node.Value ?? "", 13, SketchTheme.MutedText),
        });
        return stack;
    }

    private UIElement RenderCheckbox(UiNode node)
    {
        bool isChecked = node.HasModifier("checked");
        var row = new StackPanel { Orientation = Orientation.Horizontal };

        var box = new Border
        {
            Width           = 14,
            Height          = 14,
            BorderBrush     = SketchTheme.Border,
            BorderThickness = new Thickness(1),
            Background      = isChecked ? SketchTheme.DarkText : SketchTheme.InputBackground,
            CornerRadius    = new CornerRadius(2),
            VerticalAlignment = VerticalAlignment.Center,
            Margin          = new Thickness(0, 0, 6, 0),
        };

        if (isChecked)
            box.Child = MakeText("✓", 10, SketchTheme.White);

        row.Children.Add(box);
        row.Children.Add(MakeText(node.Label ?? "", 13));
        return row;
    }

    private UIElement RenderRadio(UiNode node)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(new Ellipse
        {
            Width   = 14, Height = 14,
            Stroke  = SketchTheme.Border,
            StrokeThickness = 1,
            Fill    = SketchTheme.InputBackground,
            Margin  = new Thickness(0, 0, 6, 0),
            VerticalAlignment = VerticalAlignment.Center,
        });
        row.Children.Add(MakeText(node.Label ?? "", 13));
        return row;
    }

    private UIElement RenderSelect(UiNode node)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical };
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, 11, SketchTheme.MutedText));

        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var valueText = MakeText(node.Value ?? "", 13, SketchTheme.MutedText, family: SketchTheme.CodeFont);
        var chevron   = MakeText("▾", 13, SketchTheme.MutedText);
        Grid.SetColumn(valueText, 0);
        Grid.SetColumn(chevron, 1);
        row.Children.Add(valueText);
        row.Children.Add(chevron);

        stack.Children.Add(new Border
        {
            BorderBrush     = SketchTheme.Border,
            BorderThickness = new Thickness(1),
            CornerRadius    = new CornerRadius(3),
            Background      = SketchTheme.InputBackground,
            Padding         = new Thickness(8, 5, 8, 5),
            Margin          = new Thickness(0, 2, 0, 0),
            Child           = row,
        });
        return stack;
    }

    private UIElement RenderToggle(UiNode node)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal };
        var track = new Border
        {
            Width           = 36, Height = 20,
            Background      = SketchTheme.Border,
            CornerRadius    = new CornerRadius(10),
            Margin          = new Thickness(0, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Center,
        };
        var thumb = new Ellipse { Width = 14, Height = 14, Fill = SketchTheme.White, Margin = new Thickness(2) };
        track.Child = thumb;
        row.Children.Add(track);
        row.Children.Add(MakeText(node.Label ?? "", 13));
        return row;
    }

    private UIElement RenderSlider(UiNode node)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical };
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, 11, SketchTheme.MutedText));

        var track = new Border
        {
            Height          = 4,
            Background      = SketchTheme.Border,
            CornerRadius    = new CornerRadius(2),
            Margin          = new Thickness(0, 6, 0, 0),
        };
        stack.Children.Add(track);
        return stack;
    }

    private UIElement RenderButton(UiNode node)
    {
        bool isPrimary  = node.HasModifier("primary");
        bool isDanger   = node.HasModifier("danger");
        bool isDisabled = node.HasModifier("disabled");
        bool isWide     = node.HasModifier("wide");

        Brush bg   = isPrimary ? SketchTheme.PrimaryFill
                   : isDanger  ? SketchTheme.DangerFill
                   : SketchTheme.Background;
        Brush fg   = (isPrimary || isDanger) ? SketchTheme.White : SketchTheme.DarkText;
        Brush bord = (isPrimary || isDanger) ? bg : SketchTheme.Border;

        if (isDisabled)
        {
            bg   = SketchTheme.Background;
            fg   = SketchTheme.MutedText;
            bord = SketchTheme.Border;
        }

        var btn = new Border
        {
            Background      = bg,
            BorderBrush     = bord,
            BorderThickness = new Thickness(1),
            CornerRadius    = new CornerRadius(4),
            Padding         = new Thickness(16, 7, 16, 7),
            Margin          = new Thickness(0, 0, 4, 0),
            Child           = new TextBlock
            {
                Text                = node.Label ?? "Button",
                Foreground          = fg,
                FontFamily          = SketchTheme.BodyFont,
                FontSize            = 13,
                HorizontalAlignment = HorizontalAlignment.Center,
            },
        };

        if (isWide)
            btn.HorizontalAlignment = HorizontalAlignment.Stretch;

        return btn;
    }

    // ── Display ──────────────────────────────────────────────────────────────

    private UIElement RenderLabel(UiNode node)
    {
        bool isMuted = node.HasModifier("muted");
        return MakeText(node.Label ?? "", 12, isMuted ? SketchTheme.MutedText : SketchTheme.DarkText);
    }

    private UIElement RenderText(UiNode node) => new TextBlock
    {
        Text         = node.Label ?? "",
        FontFamily   = SketchTheme.BodyFont,
        FontSize     = 13,
        Foreground   = SketchTheme.DarkText,
        TextWrapping = TextWrapping.Wrap,
        Margin       = new Thickness(0, 0, 0, 4),
    };

    private UIElement RenderHeading(UiNode node) => MakeText(node.Label ?? "", 18, SketchTheme.DarkText, FontWeights.Bold);

    private UIElement RenderAvatar(UiNode node)
    {
        bool isCircle = node.HasModifier("circle");
        if (isCircle)
            return new Ellipse { Width = 40, Height = 40, Fill = SketchTheme.AvatarFill, Margin = new Thickness(0, 0, 0, 4) };

        return new Rectangle { Width = 40, Height = 40, Fill = SketchTheme.AvatarFill, Margin = new Thickness(0, 0, 0, 4) };
    }

    private UIElement RenderImage(UiNode node)
    {
        double width = 200, height = 150;
        foreach (var m in node.Modifiers)
        {
            var (w, h) = ParseWxH(m);
            if (w > 0) { width = w; height = h; }
        }

        var canvas = new Canvas { Width = width, Height = height, Background = SketchTheme.Background };

        var rect = new Rectangle
        {
            Width  = width, Height = height,
            Stroke = SketchTheme.Border, StrokeThickness = 1,
        };
        Canvas.SetLeft(rect, 0); Canvas.SetTop(rect, 0);
        canvas.Children.Add(rect);

        // Diagonal lines
        var line1 = new Line { X1 = 0, Y1 = 0, X2 = width, Y2 = height, Stroke = SketchTheme.Border, StrokeThickness = 1 };
        var line2 = new Line { X1 = width, Y1 = 0, X2 = 0, Y2 = height, Stroke = SketchTheme.Border, StrokeThickness = 1 };
        canvas.Children.Add(line1);
        canvas.Children.Add(line2);

        if (node.Label != null)
        {
            var lbl = MakeText(node.Label, 11, SketchTheme.MutedText);
            Canvas.SetLeft(lbl, 4); Canvas.SetTop(lbl, 4);
            canvas.Children.Add(lbl);
        }

        return canvas;
    }

    private UIElement RenderBadge(UiNode node) => new Border
    {
        Background      = new SolidColorBrush(Color.FromRgb(0xE2, 0xE8, 0xF0)),
        CornerRadius    = new CornerRadius(10),
        Padding         = new Thickness(8, 2, 8, 2),
        Margin          = new Thickness(0, 0, 4, 0),
        Child           = MakeText(node.Label ?? "", 11, SketchTheme.DarkText),
    };

    private UIElement RenderTag(UiNode node)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(MakeText(node.Label ?? "", 11, SketchTheme.DarkText));
        row.Children.Add(MakeText(" ×", 11, SketchTheme.MutedText));
        return new Border
        {
            Background      = new SolidColorBrush(Color.FromRgb(0xE2, 0xE8, 0xF0)),
            CornerRadius    = new CornerRadius(4),
            Padding         = new Thickness(6, 2, 6, 2),
            Margin          = new Thickness(0, 0, 4, 0),
            Child           = row,
        };
    }

    private UIElement RenderTable(UiNode node)
    {
        var grid = new Grid();

        // Find columns definition
        var colsNode = node.Children.FirstOrDefault(c => c.Type == ElementType.Columns);
        var rows     = node.Children.Where(c => c.Type == ElementType.Row).ToList();

        string[] headers = colsNode?.Label?.Split(", ").Select(s => s.Trim()).ToArray()
                        ?? [];

        int colCount = Math.Max(headers.Length, rows.Count > 0
            ? rows.Max(r => r.Label?.Split(", ").Length ?? 0)
            : 0);

        for (int i = 0; i < colCount; i++)
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        int rowIndex = 0;

        // Header row
        if (headers.Length > 0)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (int c = 0; c < headers.Length; c++)
            {
                var cell = new Border
                {
                    BorderBrush     = SketchTheme.Border,
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    Padding         = new Thickness(8, 6, 8, 6),
                    Background      = SketchTheme.NavbarFill,
                    Child           = MakeText(headers[c], 12, SketchTheme.DarkText, FontWeights.SemiBold),
                };
                Grid.SetRow(cell, 0); Grid.SetColumn(cell, c);
                grid.Children.Add(cell);
            }
            rowIndex++;
        }

        // Data rows
        foreach (var row in rows)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var cells = row.Label?.Split(", ").Select(s => s.Trim()).ToArray() ?? [];
            for (int c = 0; c < Math.Min(cells.Length, colCount); c++)
            {
                var cell = new Border
                {
                    BorderBrush     = SketchTheme.Border,
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    Padding         = new Thickness(8, 6, 8, 6),
                    Child           = MakeText(cells[c], 12),
                };
                Grid.SetRow(cell, rowIndex); Grid.SetColumn(cell, c);
                grid.Children.Add(cell);
            }
            rowIndex++;
        }

        return new Border
        {
            BorderBrush     = SketchTheme.Border,
            BorderThickness = new Thickness(1),
            CornerRadius    = new CornerRadius(4),
            Child           = grid,
            Margin          = new Thickness(0, 4, 0, 4),
        };
    }

    private UIElement RenderIcon(UiNode node) => new Border
    {
        Width           = 24, Height = 24,
        Background      = SketchTheme.AvatarFill,
        CornerRadius    = new CornerRadius(4),
        Margin          = new Thickness(0, 0, 4, 0),
        Child           = MakeText(node.Label?[..1].ToUpper() ?? "?", 11, SketchTheme.DarkText),
        HorizontalAlignment = HorizontalAlignment.Left,
    };

    // ── Feedback ─────────────────────────────────────────────────────────────

    private UIElement RenderAlert(UiNode node)
    {
        bool isWarning = node.HasModifier("warning");
        bool isDanger  = node.HasModifier("danger");
        bool isSuccess = node.HasModifier("success");

        Brush accentColor = isWarning ? SketchTheme.WarningBorder
                          : isDanger  ? SketchTheme.DangerFill
                          : isSuccess ? new SolidColorBrush(Color.FromRgb(0x28, 0xA7, 0x45))
                          : new SolidColorBrush(Color.FromRgb(0x17, 0xA2, 0xB8));

        Brush bgColor = isWarning ? SketchTheme.WarningFill
                      : isDanger  ? new SolidColorBrush(Color.FromRgb(0xFF, 0xEB, 0xEB))
                      : isSuccess ? SketchTheme.SuccessFill
                      : SketchTheme.InfoFill;

        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var accent = new Rectangle { Fill = accentColor };
        var text   = new Border { Padding = new Thickness(10, 8, 10, 8), Child = MakeText(node.Label ?? "", 13) };

        Grid.SetColumn(accent, 0);
        Grid.SetColumn(text, 1);
        row.Children.Add(accent);
        row.Children.Add(text);

        return new Border
        {
            Background      = bgColor,
            BorderBrush     = accentColor,
            BorderThickness = new Thickness(1),
            CornerRadius    = new CornerRadius(4),
            ClipToBounds    = true,
            Child           = row,
            Margin          = new Thickness(0, 4, 0, 4),
        };
    }

    private UIElement RenderToast(UiNode node)
    {
        bool isSuccess = node.HasModifier("success");
        return new Border
        {
            Background      = isSuccess ? SketchTheme.PrimaryFill : SketchTheme.NavbarFill,
            BorderBrush     = SketchTheme.Border,
            BorderThickness = new Thickness(1),
            CornerRadius    = new CornerRadius(6),
            Padding         = new Thickness(12, 8, 12, 8),
            HorizontalAlignment = HorizontalAlignment.Right,
            Child           = MakeText(node.Label ?? "", 13, isSuccess ? SketchTheme.White : SketchTheme.DarkText),
            Margin          = new Thickness(0, 4, 0, 4),
        };
    }

    private UIElement RenderSpinner() => new Ellipse
    {
        Width           = 24, Height = 24,
        Stroke          = SketchTheme.Border,
        StrokeThickness = 3,
        StrokeDashArray = new DoubleCollection([4, 2]),
        Margin          = new Thickness(4),
    };

    private UIElement RenderProgress(UiNode node)
    {
        int fillPct = 0;
        foreach (var m in node.Modifiers)
            if (int.TryParse(m, out var v)) fillPct = v;

        fillPct = Math.Clamp(fillPct, 0, 100);

        var track = new Grid { Height = 8 };
        track.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(fillPct, GridUnitType.Star) });
        track.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100 - fillPct, GridUnitType.Star) });

        var fill = new Rectangle { Fill = SketchTheme.PrimaryFill, RadiusX = 4, RadiusY = 4 };
        Grid.SetColumn(fill, 0);
        track.Children.Add(fill);

        return new Border
        {
            Background      = SketchTheme.Border,
            CornerRadius    = new CornerRadius(4),
            Child           = track,
            Margin          = new Thickness(0, 4, 0, 4),
        };
    }

    // ── Date / Time ───────────────────────────────────────────────────────────

    private UIElement RenderDatePicker(UiNode node)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical };
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, 11, SketchTheme.MutedText));

        var picker = new DatePicker
        {
            Margin              = new Thickness(0, 2, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
            MinWidth            = 160,
        };
        if (node.Value != null && DateTime.TryParse(node.Value, out var dt))
            picker.SelectedDate = dt;

        stack.Children.Add(picker);
        return stack;
    }

    private UIElement RenderDateTimePicker(UiNode node)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical };
        if (node.Label != null)
            stack.Children.Add(MakeText(node.Label, 11, SketchTheme.MutedText));

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin      = new Thickness(0, 2, 0, 0),
        };

        var picker = new DatePicker { MinWidth = 140 };
        if (node.Value != null && DateTime.TryParse(node.Value, out var dt))
            picker.SelectedDate = dt;
        row.Children.Add(picker);

        // Time part: styled text box showing HH:MM
        string timeText = node.Value != null && DateTime.TryParse(node.Value, out var dt2)
            ? dt2.ToString("HH:mm")
            : "HH:MM";

        row.Children.Add(new Border
        {
            BorderBrush     = SketchTheme.Border,
            BorderThickness = new Thickness(1),
            CornerRadius    = new CornerRadius(3),
            Background      = SketchTheme.InputBackground,
            Padding         = new Thickness(8, 5, 8, 5),
            Margin          = new Thickness(6, 0, 0, 0),
            MinWidth        = 80,
            Child           = MakeText(timeText, 13, SketchTheme.MutedText, family: SketchTheme.CodeFont),
        });

        stack.Children.Add(row);
        return stack;
    }

    private UIElement RenderCalendar(UiNode node)
    {
        var cal = new System.Windows.Controls.Calendar
        {
            DisplayMode         = System.Windows.Controls.CalendarMode.Month,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin              = new Thickness(0, 2, 0, 0),
        };

        if (node.Label != null)
        {
            var stack = new StackPanel { Orientation = Orientation.Vertical };
            stack.Children.Add(MakeText(node.Label, 11, SketchTheme.MutedText));
            stack.Children.Add(cal);
            return stack;
        }

        return cal;
    }

    // ── Placeholder ───────────────────────────────────────────────────────────

    private UIElement RenderPlaceholder(UiNode node) => new Border
    {
        BorderBrush     = SketchTheme.Border,
        BorderThickness = new Thickness(1, 1, 1, 1),
        CornerRadius    = new CornerRadius(3),
        Padding         = new Thickness(6, 4, 6, 4),
        Background      = new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0)),
        Child           = MakeText($"[{node.Type}]", 11, SketchTheme.MutedText),
        Margin          = new Thickness(2),
    };
}
