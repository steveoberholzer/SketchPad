using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace Sketchpad.App.Highlighting;

/// <summary>
/// Colours any line whose first non-whitespace character is '#' in muted grey italic.
/// Implemented as a custom IVisualLineTransformer because AvalonEdit's XSHD engine
/// rejects Rule/Span patterns that can produce zero-length matches, which makes it
/// impossible to express a single-character line-comment marker via XSHD alone.
/// </summary>
public sealed class CommentColouriser : DocumentColorizingTransformer
{
    private static readonly Brush CommentBrush  = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
    private static readonly Typeface ItalicFace = new(
        new FontFamily("Consolas"),
        FontStyles.Italic,
        FontWeights.Normal,
        FontStretches.Normal);

    static CommentColouriser()
    {
        CommentBrush.Freeze();
    }

    protected override void ColorizeLine(DocumentLine line)
    {
        if (line.Length == 0) return;

        var text      = CurrentContext.Document.GetText(line);
        var trimmed   = text.TrimStart();
        if (!trimmed.StartsWith('#')) return;

        // Colour the entire line including leading whitespace
        ChangeLinePart(line.Offset, line.EndOffset, el =>
        {
            el.TextRunProperties.SetForegroundBrush(CommentBrush);
            el.TextRunProperties.SetTypeface(ItalicFace);
        });
    }
}
