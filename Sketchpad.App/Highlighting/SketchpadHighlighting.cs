using System.Reflection;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace Sketchpad.App.Highlighting;

public static class SketchpadHighlighting
{
    private static IHighlightingDefinition? _definition;
    private static bool _loaded;

    public static IHighlightingDefinition? Definition
    {
        get
        {
            if (!_loaded) { _definition = TryLoad(); _loaded = true; }
            return _definition;
        }
    }

    private static IHighlightingDefinition? TryLoad()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Enumerate to find the right name regardless of casing / path separator differences
            const string suffix = "Highlighting.Sketchpad.xshd";
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));

            if (resourceName is null)
                return null;

            using var stream = assembly.GetManifestResourceStream(resourceName)!;
            using var reader = new XmlTextReader(stream);
            return HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }
        catch
        {
            return null;   // degraded gracefully — editor still works, just no colours
        }
    }
}
