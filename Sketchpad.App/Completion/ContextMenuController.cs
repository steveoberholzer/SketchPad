using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;

namespace Sketchpad.App.Completion;

/// <summary>
/// Builds and wires a context menu onto the DSL editor.
///
/// Structure:
///   Cut / Copy / Paste / Select All
///   ──────────────────────────────
///   Insert ▶
///     window ▶              ← container: submenu, first child = self-insert
///       window              ← inserts just "window …"
///       ─────────────
///       navbar ▶            ← child container: another submenu level
///       panel ▶
///       …
///     panel ▶
///     …
///     ─────────────
///     field               ← flat leaf items
///     button
///     …
/// </summary>
public sealed class ContextMenuController
{
    private readonly TextEditor _editor;

    // ── Hierarchy definition ──────────────────────────────────────────────────

    /// <summary>
    /// Recommended direct children for each container keyword.
    /// Order matters — it becomes the menu item order.
    /// </summary>
    private static readonly Dictionary<string, string[]> CanonicalChildren = new()
    {
        ["window"]  = ["navbar", "sidebar", "panel", "card", "tabs"],
        ["panel"]   = ["card", "row", "col", "heading", "text", "divider", "spacer", "table", "alert"],
        ["card"]    = ["heading", "field", "textarea", "button", "row", "text", "label", "badge"],
        ["navbar"]  = ["brand", "menu", "item", "button"],
        ["sidebar"] = ["nav", "item", "divider"],
        ["row"]     = ["button", "field", "label", "text", "icon", "badge", "tag", "col", "spacer"],
        ["col"]     = ["heading", "text", "field", "button", "label", "card"],
        ["tabs"]    = ["tab"],
        ["tab"]     = ["panel", "card", "heading", "text", "field", "button"],
        ["menu"]    = ["item"],
        ["nav"]     = ["item"],
        ["table"]   = ["columns", "row"],
    };

    /// <summary>Top-level containers shown as the first group in Insert.</summary>
    private static readonly string[] TopLevelContainers =
        ["window", "panel", "card", "navbar", "sidebar", "row", "col", "tabs", "table"];

    /// <summary>Leaf elements shown as the second (flat) group in Insert.</summary>
    private static readonly string[] FormLeaves =
    [
        "field", "textarea", "button", "checkbox", "radio",
        "select", "toggle", "slider", "datepicker", "datetimepicker",
        "label", "text", "heading", "badge", "tag", "icon", "avatar", "image",
        "calendar", "alert", "toast", "spinner", "progress",
    ];

    // ── Construction ──────────────────────────────────────────────────────────

    public ContextMenuController(TextEditor editor)
    {
        _editor = editor;
        editor.ContextMenu = BuildContextMenu();
    }

    // ── Menu building ─────────────────────────────────────────────────────────

    private ContextMenu BuildContextMenu()
    {
        var menu = new ContextMenu();

        // Standard editing commands — AvalonEdit honours ApplicationCommands natively
        menu.Items.Add(Cmd(ApplicationCommands.Cut,       "Cut",        "Ctrl+X"));
        menu.Items.Add(Cmd(ApplicationCommands.Copy,      "Copy",       "Ctrl+C"));
        menu.Items.Add(Cmd(ApplicationCommands.Paste,     "Paste",      "Ctrl+V"));
        menu.Items.Add(new Separator());
        menu.Items.Add(Cmd(ApplicationCommands.SelectAll, "Select All", "Ctrl+A"));
        menu.Items.Add(new Separator());

        // Insert submenu
        var insertMenu = new MenuItem { Header = "Insert" };

        foreach (var kw in TopLevelContainers)
            insertMenu.Items.Add(BuildMenuItem(kw, depth: 0));

        insertMenu.Items.Add(new Separator());

        foreach (var kw in FormLeaves)
            insertMenu.Items.Add(LeafItem(kw));

        menu.Items.Add(insertMenu);
        return menu;
    }

    /// <summary>
    /// Recursively builds a menu item for <paramref name="keyword"/>.
    /// If the keyword has canonical children and we are not too deep, it becomes
    /// a submenu whose first child inserts just the element itself.
    /// </summary>
    private MenuItem BuildMenuItem(string keyword, int depth)
    {
        if (depth >= 3 || !CanonicalChildren.TryGetValue(keyword, out var children))
            return LeafItem(keyword);

        var parent = new MenuItem { Header = keyword };

        // First child: insert just this element (no children)
        parent.Items.Add(LeafItem(keyword));
        parent.Items.Add(new Separator());

        foreach (var child in children)
            parent.Items.Add(BuildMenuItem(child, depth + 1));

        return parent;
    }

    /// <summary>Creates a directly clickable (non-submenu) menu item.</summary>
    private MenuItem LeafItem(string keyword)
    {
        var item = new MenuItem { Header = keyword };
        item.Click += (_, _) => InsertSnippet(keyword);
        return item;
    }

    private static MenuItem Cmd(ICommand command, string header, string gesture)
        => new() { Command = command, Header = header, InputGestureText = gesture };

    // ── Snippet insertion ─────────────────────────────────────────────────────

    private void InsertSnippet(string keyword)
    {
        // Resolve snippet text and optional selection range
        var snippet    = DslCompletionData.All.FirstOrDefault(d => d.Text == keyword);
        string text    = snippet?.InsertText   ?? keyword;
        int selOffset  = snippet?.SelectOffset ?? -1;
        int selLength  = snippet?.SelectLength ?? 0;

        var doc        = _editor.Document;
        var line       = doc.GetLineByOffset(_editor.CaretOffset);
        string lineText = doc.GetText(line.Offset, line.Length);

        // Preserve the current line's indentation for the new element
        int indentLen  = lineText.Length - lineText.TrimStart().Length;
        string indent  = new(' ', indentLen);

        int insertAt;
        string toInsert;

        if (string.IsNullOrWhiteSpace(lineText))
        {
            // Current line is empty — use it (clear any stray spaces first)
            doc.Remove(line.Offset, line.Length);
            insertAt = line.Offset;
            toInsert = indent + text;
        }
        else
        {
            // Insert on a fresh line immediately after the current one
            insertAt = line.EndOffset;
            toInsert = "\n" + indent + text;
        }

        doc.Insert(insertAt, toInsert);

        // Move caret and optionally select the placeholder text
        int snippetStart = insertAt + toInsert.Length - text.Length;
        if (selOffset >= 0 && selLength > 0)
        {
            int selStart           = snippetStart + selOffset;
            _editor.CaretOffset    = selStart + selLength;
            _editor.SelectionStart  = selStart;
            _editor.SelectionLength = selLength;
        }
        else
        {
            _editor.CaretOffset = insertAt + toInsert.Length;
        }

        _editor.Focus();
    }
}
