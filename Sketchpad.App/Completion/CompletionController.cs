using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;

namespace Sketchpad.App.Completion;

/// <summary>
/// Wires IntelliSense (Tiers 1-3) to an AvalonEdit <see cref="TextEditor"/>.
/// </summary>
public sealed class CompletionController
{
    private readonly TextEditor      _editor;
    private          CompletionWindow? _window;

    public CompletionController(TextEditor editor)
    {
        _editor = editor;
        editor.TextArea.TextEntered += OnTextEntered;
        editor.TextArea.KeyDown     += OnKeyDown;
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    private void OnTextEntered(object sender, TextCompositionEventArgs e)
    {
        char ch = e.Text.Length == 1 ? e.Text[0] : '\0';

        if (IsElementContext())
        {
            if (char.IsLetter(ch))
                OpenElementWindow();
            return;
        }

        if (IsModifierContext())
        {
            if (ch == '[' || ch == ',' || char.IsLetter(ch))
                OpenModifierWindow();
        }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space && (Keyboard.Modifiers & ModifierKeys.Control) != 0)
        {
            e.Handled = true;
            if (IsModifierContext())
                OpenModifierWindow();
            else
                OpenElementWindow();
        }
    }

    // ── Context detection ─────────────────────────────────────────────────────

    /// <summary>True when the caret is in an element-keyword position.</summary>
    private bool IsElementContext()
    {
        var (lineText, caretCol) = CurrentLineInfo();
        var beforeCaret = lineText[..caretCol];

        // Line must not already have an unclosed [
        if (HasUnclosedBracket(beforeCaret)) return false;

        // From the start of indentation to the caret: only letters (the keyword being typed)
        var trimmed = beforeCaret.TrimStart();
        return trimmed.Length > 0
            && trimmed.All(char.IsLetter)
            && !beforeCaret.Contains('"')
            && !beforeCaret.Contains('=');
    }

    /// <summary>True when the caret is inside [...] on the current line.</summary>
    private bool IsModifierContext()
    {
        var (lineText, caretCol) = CurrentLineInfo();
        return HasUnclosedBracket(lineText[..caretCol]);
    }

    private static bool HasUnclosedBracket(string text)
    {
        int open  = text.LastIndexOf('[');
        int close = text.LastIndexOf(']');
        return open > close;
    }

    // ── Completion windows ────────────────────────────────────────────────────

    private void OpenElementWindow()
    {
        _window?.Close();

        var (lineText, caretCol) = CurrentLineInfo();
        var beforeCaret  = lineText[..caretCol];
        int leadingSpaces = beforeCaret.Length - beforeCaret.TrimStart().Length;

        // The completion segment starts at the beginning of the typed keyword
        var doc         = _editor.Document;
        var line        = doc.GetLineByOffset(_editor.CaretOffset);
        int segStart    = line.Offset + leadingSpaces;

        _window = new CompletionWindow(_editor.TextArea)
        {
            StartOffset = segStart,
            EndOffset   = _editor.CaretOffset,
        };

        // Filter to items that start with what has already been typed
        var typed = beforeCaret.TrimStart().ToLowerInvariant();
        var items = DslCompletionData.All
            .Where(d => d.Text.StartsWith(typed, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (items.Count == 0) return;

        foreach (var item in items)
            _window.CompletionList.CompletionData.Add(item);

        _window.Closed += (_, _) => _window = null;
        _window.Show();
    }

    private void OpenModifierWindow()
    {
        _window?.Close();

        var (lineText, caretCol) = CurrentLineInfo();
        var beforeCaret = lineText[..caretCol];

        // Segment: from after the last '[' or ',' (skipping spaces) to caret
        int bracketPos  = beforeCaret.LastIndexOf('[');
        int commaPos    = beforeCaret.LastIndexOf(',');
        int segStartCol = Math.Max(bracketPos, commaPos) + 1;
        // skip spaces
        while (segStartCol < caretCol && beforeCaret[segStartCol] == ' ')
            segStartCol++;

        var doc      = _editor.Document;
        var line     = doc.GetLineByOffset(_editor.CaretOffset);
        int segStart = line.Offset + segStartCol;

        _window = new CompletionWindow(_editor.TextArea)
        {
            StartOffset = segStart,
            EndOffset   = _editor.CaretOffset,
        };

        var typed = beforeCaret[segStartCol..caretCol].Trim().ToLowerInvariant();
        var items = ModifierCompletionData.All
            .Where(d => typed.Length == 0 ||
                        d.Text.StartsWith(typed, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (items.Count == 0) return;

        foreach (var item in items)
            _window.CompletionList.CompletionData.Add(item);

        _window.Closed += (_, _) => _window = null;
        _window.Show();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Returns (full line text, caret column within that line).</summary>
    private (string lineText, int caretCol) CurrentLineInfo()
    {
        var doc    = _editor.Document;
        int offset = _editor.CaretOffset;
        var line   = doc.GetLineByOffset(offset);
        string text = doc.GetText(line.Offset, line.Length);
        int col  = offset - line.Offset;
        return (text, Math.Clamp(col, 0, text.Length));
    }
}
