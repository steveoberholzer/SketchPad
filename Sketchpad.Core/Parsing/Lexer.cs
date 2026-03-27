namespace Sketchpad.Core.Parsing;

/// <summary>
/// Tokenises a single DSL line (after stripping leading whitespace).
/// Pattern: &lt;type&gt; [&lt;"label"&gt;] [= "&lt;value&gt;"] [[ modifier, ... ]]
/// </summary>
public static class Lexer
{
    public record LineParts(
        string              TypeToken,
        string?             Label,
        string?             Value,
        IReadOnlyList<string> Modifiers);

    public static LineParts Tokenise(string line)
    {
        int pos = 0;
        // Skip leading whitespace
        while (pos < line.Length && char.IsWhiteSpace(line[pos])) pos++;

        // Extract type token (first word)
        int typeStart = pos;
        while (pos < line.Length && !char.IsWhiteSpace(line[pos])) pos++;
        string typeToken = line[typeStart..pos];

        SkipSpaces(line, ref pos);

        string? label = null;
        string? value = null;
        var mods = new List<string>();

        // Optional label: "..."
        if (pos < line.Length && line[pos] == '"')
            label = ReadQuotedString(line, ref pos);

        SkipSpaces(line, ref pos);

        // Optional value: = "..."
        if (pos < line.Length && line[pos] == '=')
        {
            pos++; // consume '='
            SkipSpaces(line, ref pos);
            if (pos < line.Length && line[pos] == '"')
                value = ReadQuotedString(line, ref pos);
        }

        SkipSpaces(line, ref pos);

        // Optional modifiers: [a, b, c]
        if (pos < line.Length && line[pos] == '[')
        {
            int close = line.IndexOf(']', pos);
            if (close > pos)
            {
                string inner = line[(pos + 1)..close];
                foreach (var part in inner.Split(','))
                {
                    var m = part.Trim();
                    if (m.Length > 0) mods.Add(m);
                }
            }
        }

        return new LineParts(typeToken, label, value, mods);
    }

    private static void SkipSpaces(string s, ref int pos)
    {
        while (pos < s.Length && char.IsWhiteSpace(s[pos])) pos++;
    }

    private static string ReadQuotedString(string s, ref int pos)
    {
        // pos points at the opening "
        pos++; // skip opening quote
        int start = pos;
        while (pos < s.Length && s[pos] != '"')
        {
            if (s[pos] == '\\') pos++; // skip escaped char
            pos++;
        }
        string text = s[start..pos];
        if (pos < s.Length) pos++; // skip closing quote
        return text;
    }
}
