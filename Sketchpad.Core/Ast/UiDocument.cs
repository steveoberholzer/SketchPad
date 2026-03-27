using Sketchpad.Core.Parsing;

namespace Sketchpad.Core.Ast;

public record UiDocument
{
    public IReadOnlyList<UiNode>     Roots  { get; init; } = [];
    public IReadOnlyList<ParseError> Errors { get; init; } = [];
    public bool HasErrors => Errors.Count > 0;
}
