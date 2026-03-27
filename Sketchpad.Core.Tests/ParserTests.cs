using Sketchpad.Core.Ast;
using Sketchpad.Core.Parsing;

namespace Sketchpad.Core.Tests;

public class ParserTests
{
    [Fact]
    public void EmptySource_ReturnsEmptyDocument()
    {
        var doc = Parser.Parse("");
        Assert.Empty(doc.Roots);
        Assert.Empty(doc.Errors);
    }

    [Fact]
    public void CommentsAndBlankLines_AreIgnored()
    {
        var doc = Parser.Parse("# comment\n\n  # indented comment\n");
        Assert.Empty(doc.Roots);
        Assert.Empty(doc.Errors);
    }

    [Fact]
    public void SingleElement_ParsesType()
    {
        var doc = Parser.Parse("button");
        Assert.Single(doc.Roots);
        Assert.Equal(ElementType.Button, doc.Roots[0].Type);
    }

    [Fact]
    public void Element_WithLabel_ParsesLabel()
    {
        var doc = Parser.Parse("button \"Save\"");
        Assert.Equal("Save", doc.Roots[0].Label);
    }

    [Fact]
    public void Element_WithValue_ParsesValue()
    {
        var doc = Parser.Parse("field \"Email\" = \"user@example.com\"");
        Assert.Equal("Email", doc.Roots[0].Label);
        Assert.Equal("user@example.com", doc.Roots[0].Value);
    }

    [Fact]
    public void Element_WithModifiers_ParsesModifiers()
    {
        var doc = Parser.Parse("button \"Save\" [primary, wide]");
        var node = doc.Roots[0];
        Assert.Contains("primary", node.Modifiers);
        Assert.Contains("wide", node.Modifiers);
    }

    [Fact]
    public void Element_HasModifier_IsCaseInsensitive()
    {
        var doc = Parser.Parse("button \"x\" [Primary]");
        Assert.True(doc.Roots[0].HasModifier("primary"));
    }

    [Fact]
    public void Indentation_ImpliesNesting()
    {
        var source = """
            panel
              button "A"
              button "B"
            """;
        var doc = Parser.Parse(source);
        Assert.Single(doc.Roots);
        Assert.Equal(2, doc.Roots[0].Children.Count);
        Assert.All(doc.Roots[0].Children, n => Assert.Equal(ElementType.Button, n.Type));
    }

    [Fact]
    public void DeepNesting_IsCorrect()
    {
        var source = """
            window
              panel
                card
                  button "X"
            """;
        var doc = Parser.Parse(source);
        Assert.Single(doc.Roots);
        var window = doc.Roots[0];
        Assert.Equal(ElementType.Window, window.Type);
        var panel = Assert.Single(window.Children);
        Assert.Equal(ElementType.Panel, panel.Type);
        var card = Assert.Single(panel.Children);
        Assert.Equal(ElementType.Card, card.Type);
        var button = Assert.Single(card.Children);
        Assert.Equal(ElementType.Button, button.Type);
        Assert.Equal("X", button.Label);
    }

    [Fact]
    public void UnknownType_ProducesError_AndAttachesToTree()
    {
        var doc = Parser.Parse("frobnicator \"test\"");
        Assert.Single(doc.Roots);
        Assert.Equal(ElementType.Unknown, doc.Roots[0].Type);
        Assert.Single(doc.Errors);
        Assert.Contains("frobnicator", doc.Errors[0].Message);
    }

    [Fact]
    public void MultipleErrors_AreCollected()
    {
        var source = "foo\nbar\nbutton";
        var doc = Parser.Parse(source);
        Assert.Equal(2, doc.Errors.Count);
        Assert.Equal(3, doc.Roots.Count); // all 3 lines parsed
    }

    [Fact]
    public void CaseInsensitiveElementType()
    {
        var doc = Parser.Parse("BUTTON \"Go\"");
        Assert.Equal(ElementType.Button, doc.Roots[0].Type);
        Assert.Empty(doc.Errors);
    }

    [Fact]
    public void TabsNormalisedToTwoSpaces()
    {
        var source = "panel\n\tbutton";
        var doc = Parser.Parse(source);
        Assert.Single(doc.Roots);
        Assert.Single(doc.Roots[0].Children);
    }

    [Fact]
    public void LineNumber_IsOneBasedAndCorrect()
    {
        var source = "\n\nbutton";
        var doc = Parser.Parse(source);
        Assert.Equal(3, doc.Roots[0].Line);
    }

    [Fact]
    public void CompleteExample_ParsesWithoutErrors()
    {
        var source = """
            window "User Profile" [900x700]

              navbar
                brand "MyApp"
                menu [right]
                  item "Settings"
                  item "Logout"

              panel
                card "Personal Details"
                  row
                    field "First name" = "Steven"
                    field "Last name"  = "Oberholzer"
                  row
                    button "Save changes" [primary]
                    button "Cancel"
            """;

        var doc = Parser.Parse(source);
        Assert.False(doc.HasErrors, string.Join(", ", doc.Errors.Select(e => e.Message)));
        Assert.Single(doc.Roots);
        Assert.Equal(ElementType.Window, doc.Roots[0].Type);
    }

    [Fact]
    public void SiblingElements_ArePeersNotNested()
    {
        var source = """
            button "A"
            button "B"
            button "C"
            """;
        var doc = Parser.Parse(source);
        Assert.Equal(3, doc.Roots.Count);
        Assert.All(doc.Roots, n => Assert.Empty(n.Children));
    }

    [Fact]
    public void ModifierWithPx_IsPreserved()
    {
        var doc = Parser.Parse("sidebar [200px]");
        Assert.Contains("200px", doc.Roots[0].Modifiers);
    }

    [Fact]
    public void AvatarWithCircleModifier()
    {
        var doc = Parser.Parse("avatar [circle]");
        Assert.True(doc.Roots[0].HasModifier("circle"));
    }
}
