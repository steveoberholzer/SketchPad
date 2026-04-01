using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Sketchpad.App.Completion;

/// <summary>
/// Completion item for a DSL element keyword (Tier 1 + Tier 3 snippet insertion).
/// </summary>
public sealed class DslCompletionData : ICompletionData
{
    private readonly string _insertText;
    private readonly int    _selectOffset; // offset within _insertText where selection starts (-1 = no selection)
    private readonly int    _selectLength;

    public DslCompletionData(string keyword, string description,
                              string insertText, int selectOffset = -1, int selectLength = 0)
    {
        Text          = keyword;
        Description   = description;
        _insertText   = insertText;
        _selectOffset = selectOffset;
        _selectLength = selectLength;
    }

    // ── Public snippet data (used by ContextMenuController) ──────────────────

    public string InsertText   => _insertText;
    public int    SelectOffset => _selectOffset;
    public int    SelectLength => _selectLength;

    // ── ICompletionData ───────────────────────────────────────────────────────

    public ImageSource? Image       => null;
    public string       Text        { get; }
    public object       Content     => Text;
    public object       Description { get; }
    public double       Priority    => 0;

    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs e)
    {
        textArea.Document.Remove(completionSegment);
        int offset = textArea.Caret.Offset;
        textArea.Document.Insert(offset, _insertText);

        if (_selectOffset >= 0 && _selectLength > 0)
        {
            int start = offset + _selectOffset;
            textArea.Caret.Offset = start + _selectLength;
            textArea.Selection    = Selection.Create(textArea, start, start + _selectLength);
        }
    }

    // ── Factory ───────────────────────────────────────────────────────────────

    public static IReadOnlyList<DslCompletionData> All { get; } = Build();

    private static List<DslCompletionData> Build() =>
    [
        // Layout
        Snippet("window",   "Container window",              "window \"Title\" [800x600]",  8, 5),
        Snippet("panel",    "Layout panel",                   "panel"),
        Snippet("card",     "Card with title",                "card \"Title\"",              6, 5),
        Snippet("row",      "Horizontal row",                 "row"),
        Snippet("col",      "Vertical column",                "col"),
        Snippet("divider",  "Horizontal divider",             "divider"),
        Snippet("spacer",   "Empty spacing element",          "spacer"),

        // Navigation
        Snippet("navbar",   "Navigation bar",                 "navbar"),
        Snippet("sidebar",  "Side navigation",                "sidebar"),
        Snippet("menu",     "Menu container",                 "menu"),
        Snippet("nav",      "Navigation group",               "nav"),
        Snippet("item",     "Menu / list item",               "item \"Label\"",              6, 5),
        Snippet("tabs",     "Tab strip container",            "tabs"),
        Snippet("tab",      "Single tab",                     "tab \"Label\"",               5, 5),
        Snippet("brand",    "Brand / logo",                   "brand \"Name\"",              7, 4),

        // Form
        Snippet("field",    "Text input field",               "field \"Label\" = \"value\"", 7, 5),
        Snippet("textarea", "Multi-line text area",           "textarea \"Label\"",           10, 5),
        Snippet("checkbox", "Checkbox",                       "checkbox \"Label\"",           10, 5),
        Snippet("radio",    "Radio button",                   "radio \"Label\"",              7, 5),
        Snippet("select",   "Dropdown select",                "select \"Label\"",             8, 5),
        Snippet("toggle",   "Toggle switch",                  "toggle \"Label\"",             8, 5),
        Snippet("slider",   "Slider control",                 "slider \"Label\"",             8, 5),
        Snippet("button",   "Button",                         "button \"Label\"",             8, 5),

        // Display
        Snippet("label",    "Text label",                     "label \"Text\"",               7, 4),
        Snippet("text",     "Body text",                      "text \"Content\"",             6, 7),
        Snippet("heading",  "Section heading",                "heading \"Title\"",            9, 5),
        Snippet("avatar",   "User avatar",                    "avatar \"AL\"",                8, 2),
        Snippet("image",    "Image placeholder",              "image \"Alt text\"",           7, 8),
        Snippet("badge",    "Badge / counter",                "badge \"1\"",                  7, 1),
        Snippet("tag",      "Tag / chip",                     "tag \"Label\"",                5, 5),
        Snippet("table",    "Data table",                     "table"),
        Snippet("columns",  "Table columns definition",       "columns \"Col1\" \"Col2\"",   9, 4),
        Snippet("icon",     "Icon",                           "icon \"name\"",                6, 4),

        // Feedback
        Snippet("alert",    "Alert / notification banner",    "alert \"Message\" [info]",    7, 7),
        Snippet("toast",    "Toast notification",             "toast \"Message\"",            7, 7),
        Snippet("spinner",  "Loading spinner",                "spinner"),
        Snippet("progress", "Progress bar",                   "progress [50]"),
    ];

    private static DslCompletionData Snippet(string kw, string desc, string insert,
                                              int selOffset = -1, int selLen = 0)
        => new(kw, desc, insert, selOffset, selLen);
}
