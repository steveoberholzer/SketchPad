using Sketchpad.Core.Parsing;

namespace Sketchpad.Core.Tests;

public class LexerTests
{
    [Fact]
    public void JustType_ReturnsTypeOnly()
    {
        var parts = Lexer.Tokenise("button");
        Assert.Equal("button", parts.TypeToken);
        Assert.Null(parts.Label);
        Assert.Null(parts.Value);
        Assert.Empty(parts.Modifiers);
    }

    [Fact]
    public void TypeAndLabel()
    {
        var parts = Lexer.Tokenise("button \"Save\"");
        Assert.Equal("button", parts.TypeToken);
        Assert.Equal("Save", parts.Label);
    }

    [Fact]
    public void TypeLabelValue()
    {
        var parts = Lexer.Tokenise("field \"Email\" = \"user@example.com\"");
        Assert.Equal("field", parts.TypeToken);
        Assert.Equal("Email", parts.Label);
        Assert.Equal("user@example.com", parts.Value);
    }

    [Fact]
    public void TypeLabelModifiers()
    {
        var parts = Lexer.Tokenise("button \"Save\" [primary, wide]");
        Assert.Equal("Save", parts.Label);
        Assert.Contains("primary", parts.Modifiers);
        Assert.Contains("wide", parts.Modifiers);
    }

    [Fact]
    public void TypeLabelValueModifiers()
    {
        var parts = Lexer.Tokenise("field \"Email\" = \"foo\" [wide]");
        Assert.Equal("Email", parts.Label);
        Assert.Equal("foo", parts.Value);
        Assert.Contains("wide", parts.Modifiers);
    }

    [Fact]
    public void NoLabel_WithModifiers()
    {
        var parts = Lexer.Tokenise("avatar [circle]");
        Assert.Equal("avatar", parts.TypeToken);
        Assert.Null(parts.Label);
        Assert.Contains("circle", parts.Modifiers);
    }

    [Fact]
    public void WindowWithSizeModifier()
    {
        var parts = Lexer.Tokenise("window \"Dashboard\" [1024x768]");
        Assert.Equal("Dashboard", parts.Label);
        Assert.Contains("1024x768", parts.Modifiers);
    }

    [Fact]
    public void LeadingWhitespace_IsIgnored()
    {
        var parts = Lexer.Tokenise("   button \"X\"");
        Assert.Equal("button", parts.TypeToken);
        Assert.Equal("X", parts.Label);
    }

    [Fact]
    public void ModifierWithSpaces_Trimmed()
    {
        var parts = Lexer.Tokenise("button [ primary , wide ]");
        Assert.Contains("primary", parts.Modifiers);
        Assert.Contains("wide", parts.Modifiers);
    }
}
