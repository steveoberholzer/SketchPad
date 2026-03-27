namespace Sketchpad.Core.Ast;

public record UiNode
{
    public ElementType              Type      { get; init; }
    public string?                  Label     { get; init; }
    public string?                  Value     { get; init; }
    public IReadOnlyList<string>    Modifiers { get; init; } = [];
    public IReadOnlyList<UiNode>    Children  { get; init; } = [];
    public int                      Line      { get; init; }

    public bool HasModifier(string modifier) =>
        Modifiers.Contains(modifier, StringComparer.OrdinalIgnoreCase);
}
