using System.Text;
using Sketchpad.Core.Ast;

namespace Sketchpad.Core.Html;

/// <summary>
/// Exports a UiDocument as simple, newbie-friendly HTML — the kind the converter
/// can import back.  Intended as a "template" so users can see what HTML maps
/// to which Sketchpad elements.
/// </summary>
public static class BasicHtmlExporter
{
    public static string Export(UiDocument document)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<body>");
        sb.AppendLine();

        foreach (var node in document.Roots)
            ExportNode(node, sb, 0);

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");
        return sb.ToString();
    }

    private static void ExportNode(UiNode node, StringBuilder sb, int depth)
    {
        var ind = I(depth);

        switch (node.Type)
        {
            case ElementType.Window:
                if (node.Label != null)
                    sb.AppendLine($"{ind}<h1>{Esc(node.Label)}</h1>");
                foreach (var child in node.Children)
                    ExportNode(child, sb, depth);
                break;

            case ElementType.Card:
                if (node.Label != null)
                    sb.AppendLine($"{ind}<h2>{Esc(node.Label)}</h2>");
                sb.AppendLine($"{ind}<div>");
                foreach (var child in node.Children)
                    ExportNode(child, sb, depth + 1);
                sb.AppendLine($"{ind}</div>");
                sb.AppendLine();
                break;

            case ElementType.Panel:
            case ElementType.Col:
                sb.AppendLine($"{ind}<div>");
                foreach (var child in node.Children)
                    ExportNode(child, sb, depth + 1);
                sb.AppendLine($"{ind}</div>");
                break;

            case ElementType.Row:
                ExportRow(node, sb, depth);
                break;

            case ElementType.Field:
                var fVal = node.Value ?? "";
                var fLabel = node.Label ?? "";
                if (!string.IsNullOrEmpty(fLabel))
                    sb.AppendLine($"{ind}{Esc(fLabel)}: <input type=\"text\" value=\"{Esc(fVal)}\" /><br/>");
                else
                    sb.AppendLine($"{ind}<input type=\"text\" value=\"{Esc(fVal)}\" /><br/>");
                break;

            case ElementType.Textarea:
                if (node.Label != null)
                    sb.AppendLine($"{ind}{Esc(node.Label)}:<br/>");
                sb.AppendLine($"{ind}<textarea>{Esc(node.Value ?? "")}</textarea><br/>");
                break;

            case ElementType.Checkbox:
                sb.AppendLine($"{ind}<input type=\"checkbox\" /> {Esc(node.Label ?? "")}<br/>");
                break;

            case ElementType.Radio:
                sb.AppendLine($"{ind}<input type=\"radio\" /> {Esc(node.Label ?? "")}<br/>");
                break;

            case ElementType.Select:
                var sLabel = node.Label ?? "";
                if (!string.IsNullOrEmpty(sLabel))
                    sb.AppendLine($"{ind}{Esc(sLabel)}: <select><option>{Esc(node.Value ?? "")}</option></select><br/>");
                else
                    sb.AppendLine($"{ind}<select><option>{Esc(node.Value ?? "")}</option></select><br/>");
                break;

            case ElementType.Button:
                sb.AppendLine($"{ind}<button>{Esc(node.Label ?? "Button")}</button>");
                break;

            case ElementType.Heading:
                sb.AppendLine($"{ind}<h2>{Esc(node.Label ?? "")}</h2>");
                break;

            case ElementType.Text:
                sb.AppendLine($"{ind}<p>{Esc(node.Label ?? "")}</p>");
                break;

            case ElementType.Label:
                sb.AppendLine($"{ind}<span>{Esc(node.Label ?? "")}</span>");
                break;

            case ElementType.Table:
                ExportTable(node, sb, depth);
                break;

            case ElementType.Divider:
                sb.AppendLine($"{ind}<hr/>");
                break;

            case ElementType.Spacer:
                sb.AppendLine($"{ind}<br/>");
                break;

            case ElementType.Alert:
                sb.AppendLine($"{ind}<p><strong>{Esc(node.Label ?? "")}</strong></p>");
                break;

            // Navigation elements — recurse into children
            default:
                foreach (var child in node.Children)
                    ExportNode(child, sb, depth);
                break;
        }
    }

    private static void ExportRow(UiNode node, StringBuilder sb, int depth)
    {
        var ind = I(depth);
        sb.AppendLine($"{ind}<table><tr>");
        foreach (var child in node.Children)
        {
            sb.AppendLine($"{ind}  <td>");
            ExportNode(child, sb, depth + 2);
            sb.AppendLine($"{ind}  </td>");
        }
        sb.AppendLine($"{ind}</tr></table>");
    }

    private static void ExportTable(UiNode node, StringBuilder sb, int depth)
    {
        var ind  = I(depth);
        var ind1 = I(depth + 1);
        sb.AppendLine($"{ind}<table border=\"1\">");

        var cols = node.Children.FirstOrDefault(c => c.Type == ElementType.Columns);
        if (cols?.Label != null)
        {
            sb.AppendLine($"{ind1}<tr>");
            foreach (var col in cols.Label.Split('|').Select(s => s.Trim()))
                sb.AppendLine($"{ind1}  <th>{Esc(col)}</th>");
            sb.AppendLine($"{ind1}</tr>");
        }

        foreach (var row in node.Children.Where(c => c.Type == ElementType.Row))
        {
            var cells = row.Label?.Split('|').Select(s => s.Trim()).ToArray() ?? [];
            sb.AppendLine($"{ind1}<tr>");
            foreach (var cell in cells)
                sb.AppendLine($"{ind1}  <td>{Esc(cell)}</td>");
            sb.AppendLine($"{ind1}</tr>");
        }

        sb.AppendLine($"{ind}</table>");
    }

    private static string I(int depth) => new(' ', depth * 2);

    private static string Esc(string text) =>
        text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
}
