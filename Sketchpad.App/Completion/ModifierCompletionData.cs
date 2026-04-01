using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Sketchpad.App.Completion;

/// <summary>
/// Completion item for a modifier token inside [...] (Tier 2).
/// </summary>
public sealed class ModifierCompletionData : ICompletionData
{
    public ModifierCompletionData(string text, string description)
    {
        Text        = text;
        Description = description;
    }

    // ── ICompletionData ───────────────────────────────────────────────────────

    public ImageSource? Image       => null;
    public string       Text        { get; }
    public object       Content     => Text;
    public object       Description { get; }
    public double       Priority    => 0;

    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs e)
    {
        textArea.Document.Replace(completionSegment, Text);
    }

    // ── Factory ───────────────────────────────────────────────────────────────

    public static IReadOnlyList<ModifierCompletionData> All { get; } =
    [
        // Variants
        new("primary",   "Primary / accent style"),
        new("secondary", "Secondary style"),
        new("danger",    "Danger / destructive style"),
        new("warning",   "Warning style"),
        new("success",   "Success / positive style"),
        new("info",      "Informational style"),
        new("ghost",     "Ghost / outline style"),
        new("link",      "Looks like a hyperlink"),

        // Size
        new("sm",        "Small size"),
        new("md",        "Medium size (default)"),
        new("lg",        "Large size"),
        new("xl",        "Extra-large size"),

        // Layout
        new("left",      "Left-aligned / left-docked"),
        new("right",     "Right-aligned / right-docked"),
        new("center",    "Centred"),
        new("full",      "Full-width"),
        new("compact",   "Compact / dense"),

        // State
        new("disabled",  "Disabled / non-interactive"),
        new("readonly",  "Read-only"),
        new("checked",   "Initially checked"),
        new("loading",   "Loading state"),

        // Common dimension patterns — user will complete the number
        new("800x600",   "Window size 800×600"),
        new("1024x768",  "Window size 1024×768"),
        new("1280x800",  "Window size 1280×800"),
    ];
}
