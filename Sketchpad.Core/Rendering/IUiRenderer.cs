using Sketchpad.Core.Ast;

namespace Sketchpad.Core.Rendering;

/// <summary>
/// All renderers implement this interface. TOutput varies by renderer:
/// WPF renderers use UIElement; the HTML renderer uses string.
/// </summary>
public interface IUiRenderer<TOutput>
{
    TOutput Render(UiDocument document);
    string  DisplayName { get; }
}
