using Sketchpad.Core.Ast;

namespace Sketchpad.Core.Parsing;

public static class Parser
{
    public static UiDocument Parse(string source)
    {
        var errors = new List<ParseError>();
        var roots  = new List<UiNode>();

        // Stack entries: (node, indent-level). Sentinel at level -1 holds roots.
        var stack = new Stack<(List<UiNode> children, int level)>();
        stack.Push((roots, -1));

        int lineNumber = 0;
        foreach (var rawLine in source.Split('\n'))
        {
            lineNumber++;
            var line = rawLine.TrimEnd('\r');

            // Normalise tabs → 2 spaces
            line = line.Replace("\t", "  ");

            // Count leading spaces
            int indent = 0;
            while (indent < line.Length && line[indent] == ' ')
                indent++;

            // Skip blank and comment lines
            var trimmed = line.TrimStart();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
                continue;

            int level = indent / 2;

            // Pop stack until top is a shallower level
            while (stack.Count > 1 && stack.Peek().level >= level)
                stack.Pop();

            Lexer.LineParts parts;
            try
            {
                parts = Lexer.Tokenise(trimmed);
            }
            catch
            {
                errors.Add(new ParseError(lineNumber, $"Failed to tokenise line: {trimmed}"));
                continue;
            }

            var elementType = ParseElementType(parts.TypeToken);
            if (elementType == ElementType.Unknown)
                errors.Add(new ParseError(lineNumber, $"Unknown element type: '{parts.TypeToken}'"));

            var children = new List<UiNode>();
            var node = new UiNode
            {
                Type      = elementType,
                Label     = parts.Label,
                Value     = parts.Value,
                Modifiers = parts.Modifiers,
                Children  = children,
                Line      = lineNumber,
            };

            stack.Peek().children.Add(node);
            stack.Push((children, level));
        }

        return new UiDocument { Roots = roots, Errors = errors };
    }

    private static ElementType ParseElementType(string token) =>
        Enum.TryParse<ElementType>(token, ignoreCase: true, out var result)
            ? result
            : ElementType.Unknown;
}
