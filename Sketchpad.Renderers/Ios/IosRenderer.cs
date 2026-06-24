using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using Sketchpad.Core.Ast;
using Sketchpad.Core.Rendering;

namespace Sketchpad.Renderers.Ios;

/// <summary>
/// Apple iOS / UIKit renderer.
/// White navigation bars, inset grouped cards, blue accent, and the classic
/// green UISwitch. Window elements render with a simulated phone status bar.
/// </summary>
public class IosRenderer : IUiRenderer<UIElement>
{
    public string DisplayName => "Apple iOS";

    public UIElement Render(UiDocument document)
    {
        var root = new StackPanel
        {
            Orientation         = Orientation.Vertical,
            Background          = IosTheme.PageBg,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        foreach (var node in document.Roots)
            root.Children.Add(RenderNode(node));

        if (document.HasErrors)
        {
            var errPanel = new StackPanel { Margin = new Thickness(IosTheme.Pad) };
            foreach (var err in document.Errors)
                errPanel.Children.Add(MakeText($"Line {err.Line}: {err.Message}", IosTheme.Destructive));
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
                ElementType.Spacer   => new Border { Height = IosTheme.Gap },

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

                ElementType.Label    => MakeText(node.Label ?? "", IosTheme.SecondaryText, IosTheme.Small),
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

    private static TextBlock MakeText(string text, Brush? fg = null, double size = IosTheme.Body,
        FontWeight? weight = null)
        => new()
        {
            Text         = text,
            FontFamily   = IosTheme.Font,
            FontSize     = size,
            Foreground   = fg ?? IosTheme.DarkText,
            FontWeight   = weight ?? FontWeights.Normal,
            TextWrapping = TextWrapping.Wrap,
        };

    private StackPanel RenderChildren(UiNode node, Orientation orientation, double gap = IosTheme.Gap)
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
        double width = 390; // iPhone 14 logical width
        foreach (var m in node.Modifiers) { var (w, _) = ParseWxH(m); if (w > 0) width = w; }

        var stack = new StackPanel { Orientation = Orientation.Vertical };

        // Status bar: white strip with "9:41" and battery/signal
        var statusBar = new Border
        {
            Background = IosTheme.NavbarBg,
            Height     = IosTheme.StatusHeight,
            Padding    = new Thickness(IosTheme.Pad, 0, IosTheme.Pad, 0),
        };
        var statusGrid = new Grid();
        statusGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        statusGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        statusGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var timeText = new TextBlock
        {
            Text = "9:41", FontFamily = IosTheme.Font, FontSize = IosTheme.Caption,
            FontWeight = FontWeights.SemiBold, Foreground = IosTheme.DarkText,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var statusIcons = new TextBlock
        {
            Text = "▲▲▲  ◯  ▮▮▮", FontFamily = IosTheme.Font, FontSize = IosTheme.Caption,
            Foreground = IosTheme.DarkText, VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetColumn(timeText, 0);
        Grid.SetColumn(statusIcons, 2);
        statusGrid.Children.Add(timeText);
        statusGrid.Children.Add(statusIcons);
        statusBar.Child = statusGrid;
        stack.Children.Add(statusBar);

        // Navigation bar with window title
        var navbar = new Border
        {
            Background      = IosTheme.NavbarBg,
            Height          = IosTheme.NavbarHeight,
            BorderBrush     = IosTheme.Separator,
            BorderThickness = new Thickness(0, 0, 0, 0.5),
            Padding         = new Thickness(IosTheme.Pad, 0, IosTheme.Pad, 0),
        };
        navbar.Child = new TextBlock
        {
            Text                = node.Label ?? "App",
            FontFamily          = IosTheme.Font,
            FontSize            = 17,
            FontWeight          = FontWeights.SemiBold,
            Foreground          = IosTheme.DarkText,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment   = VerticalAlignment.Center,
        };
        stack.Children.Add(navbar);

        stack.Children.Add(new Border
        {
            Background = IosTheme.PageBg,
            Padding    = new Thickness(0, IosTheme.Gap, 0, IosTheme.Gap),
            Child      = RenderChildren(node, Orientation.Vertical),
        });

        return new Border
        {
            Child               = stack,
            Width               = width,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin              = new Thickness(8),
            CornerRadius        = new CornerRadius(IosTheme.CardRadius),
            ClipToBounds        = true,
            BorderBrush         = IosTheme.Separator,
            BorderThickness     = new Thickness(1),
        };
    }

    private UIElement RenderPanel(UiNode node)
    {
        if (node.Label != null)
        {
            var stack = new StackPanel { Orientation = Orientation.Vertical };
            stack.Children.Add(new Border
            {
                Padding = new Thickness(IosTheme.Pad, 8, IosTheme.Pad, 4),
                Child   = MakeText(node.Label.ToUpperInvariant(), IosTheme.SecondaryText, IosTheme.Small),
            });
            stack.Children.Add(GroupedCard(RenderChildren(node, Orientation.Vertical, 0)));
            return stack;
        }

        return GroupedCard(RenderChildren(node, Orientation.Vertical, 0));
    }

    private UIElement RenderCard(UiNode node)
    {
        var inner = new StackPanel { Orientation = Orientation.Vertical };

        if (node.Label != null)
            inner.Children.Add(new Border
            {
                Padding = new Thickness(IosTheme.Pad, 8, IosTheme.Pad, 4),
                Child   = MakeText(node.Label.ToUpperInvariant(), IosTheme.SecondaryText, IosTheme.Small),
            });

        // Children rendered as grouped inset card rows
        var cells = node.Children.Select(RenderNode).ToList();
        inner.Children.Add(InsetGroupedCard(cells));

        return new Border { Margin = new Thickness(0, 4, 0, 4), Child = inner };
    }

    /// Renders a list of pre-rendered elements inside an iOS inset-grouped card.
    private static UIElement InsetGroupedCard(List<UIElement> cells)
    {
        var card = new StackPanel { Orientation = Orientation.Vertical };
        for (int i = 0; i < cells.Count; i++)
        {
            card.Children.Add(new Border
            {
                Background = IosTheme.CardBg,
                Padding    = new Thickness(IosTheme.Pad, 0, IosTheme.Pad, 0),
                Child      = cells[i],
            });
            if (i < cells.Count - 1)
                card.Children.Add(new Border
                {
                    Height      = 0.5,
                    Background  = IosTheme.Separator,
                    Margin      = new Thickness(IosTheme.Pad, 0, 0, 0),
                });
        }
        return new Border
        {
            Child         = card,
            CornerRadius  = new CornerRadius(IosTheme.CornerRadius),
            ClipToBounds  = true,
            Margin        = new Thickness(IosTheme.Pad, 0, IosTheme.Pad, 0),
        };
    }

    private static UIElement GroupedCard(StackPanel content)
    {
        return new Border
        {
            Child        = content,
            Background   = IosTheme.CardBg,
            CornerRadius = new CornerRadius(IosTheme.CornerRadius),
            ClipToBounds = true,
            Margin       = new Thickness(IosTheme.Pad, 0, IosTheme.Pad, 0),
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
                Padding = new Thickness(IosTheme.Gap / 2, 0, IosTheme.Gap / 2, 0),
                Child   = RenderNode(child),
            });
        return grid;
    }

    // ── Navigation ───────────────────────────────────────────────────────────

    private UIElement RenderNavbar(UiNode node)
    {
        var left  = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        var right = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        TextBlock? title = null;

        foreach (var child in node.Children)
        {
            if (child.Type == ElementType.Brand)
            {
                title = new TextBlock
                {
                    Text       = child.Label ?? "",
                    FontFamily = IosTheme.Font,
                    FontSize   = 17,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = IosTheme.DarkText,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment   = VerticalAlignment.Center,
                };
            }
            else if (child.Type == ElementType.Menu && child.HasModifier("right"))
            {
                foreach (var sub in child.Children)
                    right.Children.Add(new Border
                    {
                        Padding = new Thickness(8, 0, 0, 0),
                        Child = MakeText(sub.Label ?? "", IosTheme.Accent, IosTheme.Body),
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
        if (title != null) { Grid.SetColumn(title, 1); grid.Children.Add(title); }
        Grid.SetColumn(right, 2);
        grid.Children.Add(left);
        grid.Children.Add(right);

        return new Border
        {
            Background      = IosTheme.NavbarBg,
            Height          = IosTheme.NavbarHeight,
            BorderBrush     = IosTheme.Separator,
            BorderThickness = new Thickness(0, 0, 0, 0.5),
            Padding         = new Thickness(IosTheme.Pad, 0, IosTheme.Pad, 0),
            Child           = grid,
        };
    }

    private UIElement RenderSidebar(UiNode node)
    {
        double width = 240;
        foreach (var m in node.Modifiers) { var px = ParsePx(m); if (px > 0) width = px; }

        var inner = RenderNav(node);
        return new Border
        {
            Width           = width,
            Background      = IosTheme.PageBg,
            BorderBrush     = IosTheme.Separator,
            BorderThickness = new Thickness(0, 0, 0.5, 0),
            Child           = inner,
        };
    }

    private UIElement RenderMenu(UiNode node)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal };
        foreach (var child in node.Children)
            panel.Children.Add(new Border
            {
                Padding = new Thickness(0, 0, IosTheme.Pad, 0),
                Child   = MakeText(child.Label ?? "", IosTheme.Accent, IosTheme.Body),
            });
        return panel;
    }

    private UIElement RenderNav(UiNode node)
    {
        var cells = node.Children.Select(RenderNode).ToList();
        return cells.Count > 0
            ? InsetGroupedCard(cells)
            : new StackPanel();
    }

    private UIElement RenderItem(UiNode node)
    {
        bool active = node.HasModifier("active");
        var row = new Grid { MinHeight = IosTheme.CellHeight };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var label = MakeText(node.Label ?? "", active ? IosTheme.Accent : IosTheme.DarkText);
        var chevron = MakeText("›", IosTheme.Disclosure, 20);
        chevron.VerticalAlignment = VerticalAlignment.Center;
        label.VerticalAlignment   = VerticalAlignment.Center;

        Grid.SetColumn(label, 0);
        Grid.SetColumn(chevron, 1);
        row.Children.Add(label);
        row.Children.Add(chevron);
        return row;
    }

    private UIElement RenderTabs(UiNode node)
    {
        // iOS segmented control style
        var strip = new StackPanel { Orientation = Orientation.Horizontal };
        foreach (var child in node.Children) strip.Children.Add(RenderNode(child));

        return new Border
        {
            Background   = IosTheme.SegmentBg,
            CornerRadius = new CornerRadius(IosTheme.Gap),
            Padding      = new Thickness(2),
            Margin       = new Thickness(IosTheme.Pad, IosTheme.Gap, IosTheme.Pad, IosTheme.Gap),
            Child        = strip,
        };
    }

    private UIElement RenderTab(UiNode node)
    {
        bool active = node.HasModifier("active");
        return new Border
        {
            Background   = active ? IosTheme.SegmentActive : Brushes.Transparent,
            CornerRadius = new CornerRadius(IosTheme.Gap - 2),
            Padding      = new Thickness(12, 5, 12, 5),
            Margin       = new Thickness(1),
            Child        = MakeText(node.Label ?? "",
                fg: IosTheme.DarkText,
                size: IosTheme.Small,
                weight: active ? FontWeights.SemiBold : FontWeights.Normal),
        };
    }

    private UIElement RenderBrand(UiNode node) => new Border
    {
        Padding = new Thickness(0, 0, IosTheme.Pad, 0),
        Child   = MakeText(node.Label ?? "App", weight: FontWeights.SemiBold),
    };

    // ── Form ─────────────────────────────────────────────────────────────────

    private UIElement RenderField(UiNode node)
    {
        var row = new Grid { MinHeight = IosTheme.CellHeight };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var lbl = MakeText(node.Label ?? "", IosTheme.DarkText);
        lbl.VerticalAlignment = VerticalAlignment.Center;
        lbl.Margin = new Thickness(0, 0, IosTheme.Pad, 0);

        var val = MakeText(node.Value ?? "", IosTheme.SecondaryText);
        val.VerticalAlignment   = VerticalAlignment.Center;
        val.HorizontalAlignment = HorizontalAlignment.Right;
        val.TextAlignment       = TextAlignment.Right;

        Grid.SetColumn(lbl, 0);
        Grid.SetColumn(val, 1);
        row.Children.Add(lbl);
        row.Children.Add(val);
        return row;
    }

    private UIElement RenderTextarea(UiNode node)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical };
        if (node.Label != null)
            stack.Children.Add(new Border
            {
                Padding = new Thickness(IosTheme.Pad, 8, IosTheme.Pad, 4),
                Child   = MakeText(node.Label.ToUpperInvariant(), IosTheme.SecondaryText, IosTheme.Small),
            });

        stack.Children.Add(new Border
        {
            Background      = IosTheme.CardBg,
            CornerRadius    = new CornerRadius(IosTheme.CornerRadius),
            Margin          = new Thickness(IosTheme.Pad, 0, IosTheme.Pad, 0),
            Height          = 80,
        });
        return stack;
    }

    private UIElement RenderCheckbox(UiNode node)
    {
        bool chk = node.HasModifier("checked");
        var row = new Grid { MinHeight = IosTheme.CellHeight };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var lbl = MakeText(node.Label ?? "");
        lbl.VerticalAlignment = VerticalAlignment.Center;

        // iOS checkmark: filled blue circle with ✓
        UIElement check = chk
            ? (UIElement)new Border
              {
                  Width = 22, Height = 22,
                  CornerRadius = new CornerRadius(11),
                  Background = IosTheme.Accent,
                  VerticalAlignment = VerticalAlignment.Center,
                  Child = new TextBlock
                  {
                      Text = "✓", FontSize = 13, Foreground = IosTheme.AccentText,
                      HorizontalAlignment = HorizontalAlignment.Center,
                      VerticalAlignment   = VerticalAlignment.Center,
                  },
              }
            : (UIElement)new Ellipse
              {
                  Width = 22, Height = 22,
                  Stroke = IosTheme.Separator, StrokeThickness = 1.5,
                  VerticalAlignment = VerticalAlignment.Center,
              };

        Grid.SetColumn(lbl, 0);
        Grid.SetColumn(check, 1);
        row.Children.Add(lbl);
        row.Children.Add(check);
        return row;
    }

    private UIElement RenderRadio(UiNode node)
    {
        bool chk = node.HasModifier("checked");
        var row = new Grid { MinHeight = IosTheme.CellHeight };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var lbl = MakeText(node.Label ?? "");
        lbl.VerticalAlignment = VerticalAlignment.Center;

        UIElement dot = chk
            ? (UIElement)new Border
              {
                  Width = 22, Height = 22,
                  CornerRadius = new CornerRadius(11),
                  BorderBrush = IosTheme.Accent, BorderThickness = new Thickness(1.5),
                  Background = Brushes.Transparent,
                  VerticalAlignment = VerticalAlignment.Center,
                  Child = new Ellipse { Width = 12, Height = 12, Fill = IosTheme.Accent },
              }
            : (UIElement)new Ellipse
              {
                  Width = 22, Height = 22,
                  Stroke = IosTheme.Separator, StrokeThickness = 1.5,
                  VerticalAlignment = VerticalAlignment.Center,
              };

        Grid.SetColumn(lbl, 0);
        Grid.SetColumn(dot, 1);
        row.Children.Add(lbl);
        row.Children.Add(dot);
        return row;
    }

    private UIElement RenderSelect(UiNode node)
    {
        var row = new Grid { MinHeight = IosTheme.CellHeight };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var lbl  = MakeText(node.Label ?? "", IosTheme.DarkText);
        var val  = MakeText(node.Value ?? "", IosTheme.SecondaryText);
        var chev = MakeText("›", IosTheme.Disclosure, 20);

        lbl.VerticalAlignment  = VerticalAlignment.Center;
        lbl.Margin             = new Thickness(0, 0, IosTheme.Pad, 0);
        val.VerticalAlignment  = VerticalAlignment.Center;
        val.HorizontalAlignment = HorizontalAlignment.Right;
        val.TextAlignment      = TextAlignment.Right;
        chev.VerticalAlignment = VerticalAlignment.Center;

        Grid.SetColumn(lbl, 0); Grid.SetColumn(val, 1); Grid.SetColumn(chev, 2);
        row.Children.Add(lbl); row.Children.Add(val); row.Children.Add(chev);
        return row;
    }

    private UIElement RenderToggle(UiNode node)
    {
        bool on = node.HasModifier("on") || node.HasModifier("checked");
        var row = new Grid { MinHeight = IosTheme.CellHeight };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var lbl = MakeText(node.Label ?? "");
        lbl.VerticalAlignment = VerticalAlignment.Center;

        // Classic iOS UISwitch
        var knob = new Ellipse
        {
            Width  = 24, Height = 24, Fill = Brushes.White,
            VerticalAlignment   = VerticalAlignment.Center,
            HorizontalAlignment = on ? HorizontalAlignment.Right : HorizontalAlignment.Left,
            Margin              = new Thickness(1),
        };
        var track = new Border
        {
            Width        = 50, Height = 28,
            CornerRadius = new CornerRadius(14),
            Background   = on ? IosTheme.SwitchOn : IosTheme.SwitchOff,
            VerticalAlignment = VerticalAlignment.Center,
            Child        = knob,
        };

        Grid.SetColumn(lbl, 0);
        Grid.SetColumn(track, 1);
        row.Children.Add(lbl);
        row.Children.Add(track);
        return row;
    }

    private UIElement RenderSlider(UiNode node)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical };
        if (node.Label != null)
            stack.Children.Add(new Border
            {
                Padding = new Thickness(IosTheme.Pad, 8, IosTheme.Pad, 2),
                Child   = MakeText(node.Label, IosTheme.SecondaryText, IosTheme.Small),
            });

        var track = new Grid { Height = 28, Margin = new Thickness(IosTheme.Pad, 4, IosTheme.Pad, 4) };
        track.Children.Add(new Border
        {
            Height = 4, Background = IosTheme.Separator,
            CornerRadius = new CornerRadius(2), VerticalAlignment = VerticalAlignment.Center,
        });
        track.Children.Add(new Border
        {
            Height = 4, Width = 100, Background = IosTheme.Accent,
            CornerRadius = new CornerRadius(2), VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
        });
        track.Children.Add(new Ellipse
        {
            Width = 22, Height = 22, Fill = Brushes.White,
            Stroke = IosTheme.Separator, StrokeThickness = 0.5,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(88, 0, 0, 0),
        });

        stack.Children.Add(track);
        return stack;
    }

    private UIElement RenderButton(UiNode node)
    {
        bool primary  = node.HasModifier("primary");
        bool danger   = node.HasModifier("danger");
        bool disabled = node.HasModifier("disabled");

        if (!primary && !danger)
        {
            // Text-style button (blue link)
            return new Border
            {
                Padding = new Thickness(0, 12, 0, 12),
                HorizontalAlignment = HorizontalAlignment.Center,
                Child = MakeText(node.Label ?? "Button",
                    disabled ? IosTheme.SecondaryText : IosTheme.Accent,
                    IosTheme.Body),
            };
        }

        Brush bg = danger ? IosTheme.Destructive : IosTheme.Accent;
        if (disabled) bg = IosTheme.Separator;

        return new Border
        {
            Background          = bg,
            CornerRadius        = new CornerRadius(IosTheme.CornerRadius),
            Padding             = new Thickness(IosTheme.Pad, 14, IosTheme.Pad, 14),
            Margin              = new Thickness(IosTheme.Pad, 4, IosTheme.Pad, 4),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Child               = new TextBlock
            {
                Text                = node.Label ?? "Button",
                FontFamily          = IosTheme.Font,
                FontSize            = IosTheme.Body,
                FontWeight          = FontWeights.SemiBold,
                Foreground          = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
            },
        };
    }

    // ── Display ──────────────────────────────────────────────────────────────

    private UIElement RenderHeading(UiNode node) =>
        new Border
        {
            Padding = new Thickness(IosTheme.Pad, IosTheme.Gap, IosTheme.Pad, IosTheme.Gap),
            Child   = MakeText(node.Label ?? "", size: 28, weight: FontWeights.Bold),
        };

    private UIElement RenderAvatar(UiNode node)
    {
        bool circle = node.HasModifier("circle");
        int size = 40;
        if (circle)
            return new Ellipse
            {
                Width = size, Height = size,
                Fill = IosTheme.Accent, Margin = new Thickness(0, 0, 0, 4),
            };
        return new Rectangle
        {
            Width = size, Height = size, Fill = IosTheme.Accent,
            RadiusX = IosTheme.CornerRadius / 2, RadiusY = IosTheme.CornerRadius / 2,
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
            Width = w, Height = h, Fill = IosTheme.InputFill,
            RadiusX = IosTheme.CornerRadius, RadiusY = IosTheme.CornerRadius,
        });
        canvas.Children.Add(new Line { X1 = 0, Y1 = 0, X2 = w, Y2 = h, Stroke = IosTheme.Separator, StrokeThickness = 1 });
        canvas.Children.Add(new Line { X1 = w, Y1 = 0, X2 = 0, Y2 = h, Stroke = IosTheme.Separator, StrokeThickness = 1 });
        return canvas;
    }

    private UIElement RenderBadge(UiNode node) => new Border
    {
        Background      = IosTheme.Destructive,
        CornerRadius    = new CornerRadius(10),
        Padding         = new Thickness(7, 2, 7, 2),
        Margin          = new Thickness(0, 0, 4, 0),
        HorizontalAlignment = HorizontalAlignment.Left,
        Child           = MakeText(node.Label ?? "", IosTheme.AccentText, IosTheme.Small),
    };

    private UIElement RenderTag(UiNode node)
    {
        return new Border
        {
            Background      = IosTheme.InputFill,
            BorderBrush     = IosTheme.Separator,
            BorderThickness = new Thickness(1),
            CornerRadius    = new CornerRadius(IosTheme.Gap),
            Padding         = new Thickness(10, 4, 10, 4),
            Margin          = new Thickness(0, 0, 4, 0),
            Child           = MakeText(node.Label ?? "", size: IosTheme.Small),
        };
    }

    private UIElement RenderTable(UiNode node)
    {
        var colsNode = node.Children.FirstOrDefault(c => c.Type == ElementType.Columns);
        var rows     = node.Children.Where(c => c.Type == ElementType.Row).ToList();
        var headers  = colsNode?.Label?.Split('|').Select(s => s.Trim()).ToArray() ?? [];
        int colCount = Math.Max(headers.Length, rows.Count > 0 ? rows.Max(r => r.Label?.Split('|').Length ?? 0) : 0);

        var grid = new Grid();
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
                    Background      = IosTheme.InputFill,
                    Padding         = new Thickness(IosTheme.Pad, 8, IosTheme.Pad, 8),
                    BorderBrush     = IosTheme.Separator,
                    BorderThickness = new Thickness(0, 0, 0, 0.5),
                    Child           = MakeText(headers[c], IosTheme.SecondaryText, IosTheme.Small, FontWeights.SemiBold),
                };
                Grid.SetRow(cell, 0); Grid.SetColumn(cell, c);
                grid.Children.Add(cell);
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
                    Background      = alt ? IosTheme.InputFill : IosTheme.CardBg,
                    Padding         = new Thickness(IosTheme.Pad, 10, IosTheme.Pad, 10),
                    BorderBrush     = IosTheme.Separator,
                    BorderThickness = new Thickness(0, 0, 0, 0.5),
                    Child           = MakeText(cells[c]),
                };
                Grid.SetRow(cell, rowIdx); Grid.SetColumn(cell, c);
                grid.Children.Add(cell);
            }
            rowIdx++;
            alt = !alt;
        }

        return new Border
        {
            BorderBrush     = IosTheme.Separator,
            BorderThickness = new Thickness(0.5),
            CornerRadius    = new CornerRadius(IosTheme.CornerRadius),
            ClipToBounds    = true,
            Margin          = new Thickness(IosTheme.Pad, 4, IosTheme.Pad, 4),
            Child           = grid,
        };
    }

    private UIElement RenderIcon(UiNode node) => new Border
    {
        Width        = 30,
        Height       = 30,
        Background   = IosTheme.Accent,
        CornerRadius = new CornerRadius(7),
        Margin       = new Thickness(0, 0, 8, 0),
        Child        = MakeText(node.Label?[..1].ToUpper() ?? "?", IosTheme.AccentText, IosTheme.Small),
        HorizontalAlignment = HorizontalAlignment.Left,
    };

    // ── Feedback ─────────────────────────────────────────────────────────────

    private static UIElement RenderDivider() => new Border
    {
        Height              = 0.5,
        Background          = IosTheme.Separator,
        Margin              = new Thickness(IosTheme.Pad, 4, 0, 4),
        HorizontalAlignment = HorizontalAlignment.Stretch,
    };

    private UIElement RenderAlert(UiNode node)
    {
        bool warning = node.HasModifier("warning");
        bool danger  = node.HasModifier("danger") || node.HasModifier("error");
        bool success = node.HasModifier("success");

        Brush accent = warning ? IosTheme.Warning
                     : danger  ? IosTheme.Destructive
                     : success ? IosTheme.Success
                     : IosTheme.Accent;

        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var stripe = new Rectangle { Fill = accent };
        var text   = new Border { Padding = new Thickness(IosTheme.Pad, 10, IosTheme.Pad, 10), Child = MakeText(node.Label ?? "", size: IosTheme.Small) };
        Grid.SetColumn(stripe, 0); Grid.SetColumn(text, 1);
        row.Children.Add(stripe); row.Children.Add(text);

        return new Border
        {
            Background      = IosTheme.CardBg,
            BorderBrush     = accent,
            BorderThickness = new Thickness(0.5),
            CornerRadius    = new CornerRadius(IosTheme.Gap),
            ClipToBounds    = true,
            Margin          = new Thickness(IosTheme.Pad, 4, IosTheme.Pad, 4),
            Child           = row,
        };
    }

    private UIElement RenderToast(UiNode node) => new Border
    {
        Background          = new SolidColorBrush(Color.FromArgb(0xEE, 0x1C, 0x1C, 0x1E)),
        CornerRadius        = new CornerRadius(IosTheme.CardRadius),
        Padding             = new Thickness(IosTheme.Pad, IosTheme.Gap, IosTheme.Pad, IosTheme.Gap),
        Margin              = new Thickness(IosTheme.Pad * 2, 4, IosTheme.Pad * 2, 4),
        HorizontalAlignment = HorizontalAlignment.Center,
        Child               = MakeText(node.Label ?? "", Brushes.White, IosTheme.Small),
    };

    private static UIElement RenderSpinner() => new Ellipse
    {
        Width           = 24,
        Height          = 24,
        Stroke          = IosTheme.SecondaryText,
        StrokeThickness = 3,
        StrokeDashArray = new DoubleCollection([5, 3]),
        Margin          = new Thickness(4),
    };

    private UIElement RenderProgress(UiNode node)
    {
        int pct = 0;
        foreach (var m in node.Modifiers) if (int.TryParse(m, out var v)) pct = v;
        pct = Math.Clamp(pct, 0, 100);

        var track = new Grid { Height = 4 };
        track.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(pct, GridUnitType.Star) });
        track.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100 - pct, GridUnitType.Star) });
        var fill = new Rectangle { Fill = IosTheme.Accent, RadiusX = 2, RadiusY = 2 };
        Grid.SetColumn(fill, 0);
        track.Children.Add(fill);

        return new Border
        {
            Background   = IosTheme.Separator,
            CornerRadius = new CornerRadius(2),
            Child        = track,
            Margin       = new Thickness(IosTheme.Pad, 4, IosTheme.Pad, 4),
        };
    }

    // ── Date / Time ───────────────────────────────────────────────────────────

    private UIElement RenderDatePicker(UiNode node)
    {
        var row = new Grid { MinHeight = IosTheme.CellHeight };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var lbl  = MakeText(node.Label ?? "Date", IosTheme.DarkText);
        var val  = MakeText(node.Value ?? "MM / DD / YYYY", IosTheme.Accent);
        var chev = MakeText("›", IosTheme.Disclosure, 20);

        lbl.VerticalAlignment = val.VerticalAlignment = chev.VerticalAlignment = VerticalAlignment.Center;
        val.HorizontalAlignment = HorizontalAlignment.Right;
        val.TextAlignment = TextAlignment.Right;

        Grid.SetColumn(lbl, 0); Grid.SetColumn(val, 1); Grid.SetColumn(chev, 2);
        row.Children.Add(lbl); row.Children.Add(val); row.Children.Add(chev);
        return row;
    }

    private UIElement RenderDateTimePicker(UiNode node)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical };
        stack.Children.Add(RenderDatePicker(node));
        var timeRow = new Grid { MinHeight = IosTheme.CellHeight };
        timeRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        timeRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        var lbl = MakeText("Time", IosTheme.DarkText);
        var val = MakeText("HH : MM", IosTheme.Accent);
        lbl.VerticalAlignment = val.VerticalAlignment = VerticalAlignment.Center;
        val.HorizontalAlignment = HorizontalAlignment.Right;
        Grid.SetColumn(lbl, 0); Grid.SetColumn(val, 1);
        timeRow.Children.Add(lbl); timeRow.Children.Add(val);
        stack.Children.Add(timeRow);
        return stack;
    }

    private UIElement RenderCalendar(UiNode node)
    {
        var stack = new StackPanel { Orientation = Orientation.Vertical };

        var header = new StackPanel
        {
            Orientation         = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin              = new Thickness(0, 0, 0, IosTheme.Gap),
        };
        header.Children.Add(MakeText("‹  ", IosTheme.Accent, IosTheme.Body));
        header.Children.Add(MakeText("April 2024", size: IosTheme.Body, weight: FontWeights.SemiBold));
        header.Children.Add(MakeText("  ›", IosTheme.Accent, IosTheme.Body));
        stack.Children.Add(header);

        var dowGrid = new UniformGrid { Columns = 7, Rows = 1 };
        foreach (var d in new[] { "S", "M", "T", "W", "T", "F", "S" })
            dowGrid.Children.Add(IosCalCell(d, IosTheme.SecondaryText, FontWeights.Medium));
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
            var tb = IosCalCell(cell, today ? Brushes.White : IosTheme.DarkText);
            dayGrid.Children.Add(today
                ? new Border
                  {
                      Background   = IosTheme.Accent,
                      CornerRadius = new CornerRadius(16),
                      Child        = tb,
                      Margin       = new Thickness(2),
                  }
                : tb);
        }
        stack.Children.Add(dayGrid);

        return new Border
        {
            Background      = IosTheme.CardBg,
            CornerRadius    = new CornerRadius(IosTheme.CardRadius),
            Padding         = new Thickness(IosTheme.Pad),
            Margin          = new Thickness(IosTheme.Pad, 4, IosTheme.Pad, 4),
            Child           = stack,
        };
    }

    private static TextBlock IosCalCell(string text, Brush? fg = null, FontWeight? weight = null) => new()
    {
        Text                = text,
        FontFamily          = IosTheme.Font,
        FontSize            = IosTheme.Small,
        Foreground          = fg ?? IosTheme.DarkText,
        FontWeight          = weight ?? FontWeights.Normal,
        TextAlignment       = TextAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment   = VerticalAlignment.Center,
        Padding             = new Thickness(2, 5, 2, 5),
        MinWidth            = 30,
    };

    private UIElement RenderPlaceholder(UiNode node) => new Border
    {
        Background      = IosTheme.InputFill,
        BorderBrush     = IosTheme.Separator,
        BorderThickness = new Thickness(0.5),
        CornerRadius    = new CornerRadius(IosTheme.Gap),
        Padding         = new Thickness(IosTheme.Gap, 4, IosTheme.Gap, 4),
        Margin          = new Thickness(2),
        Child           = MakeText($"[{node.Type}]", IosTheme.SecondaryText, IosTheme.Small),
    };
}
