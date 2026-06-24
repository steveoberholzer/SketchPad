using System.Net;
using System.Text;
using HtmlAgilityPack;

namespace Sketchpad.Core.Html;

/// <summary>
/// Converts simple, newbie-style HTML into Sketchpad DSL.
///
/// Rules:
///   h1–h6                      → card "title"  (sections wrapped by heading level)
///   table with inputs inside   → layout table → row + field/checkbox/etc.
///   table without inputs       → data table  → table + columns + row
///   text-node before input     → field label
///   label element wrapping input → field label
///   p / bare text              → text "…"
///   button / a                 → button "…"
///   textarea / select          → textarea / select
///   input type=checkbox/radio  → checkbox / radio
///   hr                         → divider
/// </summary>
public static class HtmlConverter
{
    public static string Convert(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var body = doc.DocumentNode.SelectSingleNode("//body") ?? doc.DocumentNode;
        var sb   = new StringBuilder();
        ProcessSiblings(Children(body), sb, 0);
        return sb.ToString().TrimEnd();
    }

    // ── Sibling-level dispatch ───────────────────────────────────────────────

    private static void ProcessSiblings(List<HtmlNode> nodes, StringBuilder sb, int depth)
    {
        nodes = nodes.Where(n => !IsIgnored(n)).ToList();

        if (nodes.Any(IsHeading))
            ProcessWithHeadings(nodes, sb, depth);
        else
            ProcessInline(nodes, sb, depth);
    }

    private static void ProcessWithHeadings(List<HtmlNode> nodes, StringBuilder sb, int depth)
    {
        int i = 0;

        // Content before first heading
        var pre = new List<HtmlNode>();
        while (i < nodes.Count && !IsHeading(nodes[i]))
            pre.Add(nodes[i++]);
        if (pre.Count > 0) ProcessInline(pre, sb, depth);

        while (i < nodes.Count)
        {
            var heading = nodes[i++];
            int level   = HeadingLevel(heading);
            var text    = Text(heading).Trim();

            // Collect body until next heading at same or higher level
            var body = new List<HtmlNode>();
            while (i < nodes.Count && !(IsHeading(nodes[i]) && HeadingLevel(nodes[i]) <= level))
                body.Add(nodes[i++]);

            Emit(sb, depth, $"card \"{Esc(text)}\"");
            ProcessSiblings(body, sb, depth + 1);
        }
    }

    private static void ProcessInline(List<HtmlNode> nodes, StringBuilder sb, int depth)
    {
        var items = BuildLineItems(nodes);
        if (items.Count == 0) return;

        // Group consecutive items that share the same HTML source line
        var groups = new List<List<LineItem>>();
        var current = new List<LineItem> { items[0] };

        for (int i = 1; i < items.Count; i++)
        {
            if (items[i].SourceLine == current[0].SourceLine && items[i].SourceLine > 0)
                current.Add(items[i]);
            else
            {
                groups.Add(current);
                current = [items[i]];
            }
        }
        groups.Add(current);

        foreach (var group in groups)
        {
            // Wrap in row only when 2+ leaf form/action elements share a line
            bool shouldRow = group.Count > 1 && group.All(IsRowable);
            if (shouldRow)
            {
                Emit(sb, depth, "row");
                foreach (var item in group)
                    EmitLineItem(item, sb, depth + 1);
            }
            else
            {
                foreach (var item in group)
                    EmitLineItem(item, sb, depth);
            }
        }
    }

    // ── Line-item helpers ────────────────────────────────────────────────────

    private sealed record LineItem(string? Label, HtmlNode Node, int SourceLine);

    /// <summary>
    /// Pairs text-label+input combos and annotates every item with its source
    /// line number (from the element itself, so the pair's line = input's line).
    /// </summary>
    private static List<LineItem> BuildLineItems(List<HtmlNode> nodes)
    {
        var result = new List<LineItem>();
        int i = 0;
        while (i < nodes.Count)
        {
            var node = nodes[i];

            // Text node immediately before a form input → label+input pair
            if (node.NodeType == HtmlNodeType.Text &&
                i + 1 < nodes.Count && IsFormInput(nodes[i + 1]))
            {
                var label = CleanLabel(node.InnerText);
                if (!string.IsNullOrWhiteSpace(label))
                {
                    // Use the *input's* line so pressing Enter before the label
                    // correctly places the pair on the next line.
                    result.Add(new LineItem(label, nodes[i + 1], nodes[i + 1].Line));
                    i += 2;
                    continue;
                }
            }

            // Skip pure-whitespace text nodes
            if (node.NodeType == HtmlNodeType.Text &&
                string.IsNullOrWhiteSpace(node.InnerText.Replace(" ", "")))
            {
                i++;
                continue;
            }

            result.Add(new LineItem(null, node, node.Line));
            i++;
        }
        return result;
    }

    private static void EmitLineItem(LineItem item, StringBuilder sb, int depth)
    {
        if (item.Label != null && IsFormInput(item.Node))
            EmitFormInput(item.Node, item.Label, sb, depth);
        else
            ConvertNode(item.Node, sb, depth);
    }

    /// <summary>True for leaf form/action elements that make sense inside a row.</summary>
    private static bool IsRowable(LineItem item) =>
        item.Label != null || // labelled input pair always rowable
        (item.Node.NodeType == HtmlNodeType.Element &&
         item.Node.Name.ToLowerInvariant() is "input" or "button" or "textarea" or "select" or "a");

    // ── Per-node conversion ──────────────────────────────────────────────────

    private static void ConvertNode(HtmlNode node, StringBuilder sb, int depth)
    {
        switch (node.Name.ToLowerInvariant())
        {
            case "#text":
                var t = CleanText(node.InnerText);
                if (!string.IsNullOrWhiteSpace(t))
                    Emit(sb, depth, $"text \"{Esc(t)}\"");
                break;

            case "p":
                var pText = Text(node).Trim();
                if (!string.IsNullOrWhiteSpace(pText))
                    Emit(sb, depth, $"text \"{Esc(pText)}\"");
                break;

            case "label":
                ConvertLabelElement(node, sb, depth);
                break;

            case "table":
                ConvertTable(node, sb, depth);
                break;

            case "input":
            case "textarea":
            case "select":
                EmitFormInput(node, null, sb, depth);
                break;

            case "button":
                Emit(sb, depth, $"button \"{Esc(Text(node).Trim())}\"");
                break;

            case "a":
                var aText = Text(node).Trim();
                if (!string.IsNullOrWhiteSpace(aText))
                    Emit(sb, depth, $"button \"{Esc(aText)}\"");
                break;

            case "ul":
            case "ol":
                foreach (var li in Nodes(node, ".//li"))
                {
                    var liText = Text(li).Trim();
                    if (!string.IsNullOrWhiteSpace(liText))
                        Emit(sb, depth, $"item \"{Esc(liText)}\"");
                }
                break;

            case "hr":
                Emit(sb, depth, "divider");
                break;

            // Transparent containers — recurse
            case "div":
            case "span":
            case "form":
            case "fieldset":
            case "section":
            case "article":
            case "main":
            case "header":
            case "footer":
            case "html":
            case "body":
                ProcessSiblings(Children(node), sb, depth);
                break;

            // Silently skip non-content tags
            case "br":
            case "script":
            case "style":
            case "head":
            case "meta":
            case "link":
            case "noscript":
            case "img":
                break;

            default:
                if (node.HasChildNodes)
                    ProcessSiblings(Children(node), sb, depth);
                break;
        }
    }

    private static void ConvertLabelElement(HtmlNode node, StringBuilder sb, int depth)
    {
        // <label>Text <input .../></label>  or  <label>Text</label>
        var input = node.SelectSingleNode(".//input|.//textarea|.//select");
        if (input != null)
        {
            var labelText = string.Concat(node.ChildNodes
                .Where(c => c.NodeType == HtmlNodeType.Text)
                .Select(c => c.InnerText));
            EmitFormInput(input, CleanLabel(labelText), sb, depth);
        }
        else
        {
            var text = Text(node).Trim();
            if (!string.IsNullOrWhiteSpace(text))
                Emit(sb, depth, $"label \"{Esc(text)}\"");
        }
    }

    private static void ConvertTable(HtmlNode tableNode, StringBuilder sb, int depth)
    {
        var rows = Nodes(tableNode, ".//tr");

        // Layout table if it contains any form controls
        bool isLayout = tableNode.SelectSingleNode(".//input|.//textarea|.//select") != null;

        if (isLayout)
        {
            foreach (var tr in rows)
            {
                var cells      = Nodes(tr, "./td|./th");
                int fieldCount = CountFields(cells);

                if (fieldCount > 1)
                {
                    Emit(sb, depth, "row");
                    EmitLayoutCells(cells, sb, depth + 1);
                }
                else
                {
                    EmitLayoutCells(cells, sb, depth);
                }
            }
        }
        else
        {
            // Data table → table / columns / row
            Emit(sb, depth, "table");
            bool headerDone = false;
            foreach (var tr in rows)
            {
                var cells = Nodes(tr, "./td|./th");
                if (cells.Count == 0) continue;

                var joined  = string.Join(" | ", cells.Select(c => Text(c).Trim()));
                var keyword = headerDone ? "row" : "columns";
                Emit(sb, depth + 1, $"{keyword} \"{Esc(joined)}\"");
                headerDone = true;
            }
        }
    }

    private static void EmitLayoutCells(List<HtmlNode> cells, StringBuilder sb, int depth)
    {
        int i = 0;
        while (i < cells.Count)
        {
            var cell  = cells[i];
            var input = cell.SelectSingleNode(".//input|.//textarea|.//select");

            if (input != null)
            {
                // Cell contains an input — its own text content is the label
                EmitFormInput(input, CellLabel(cell), sb, depth);
                i++;
                continue;
            }

            // Label-only cell followed by an input cell?
            var cellText = Text(cell).Trim();
            if (!string.IsNullOrWhiteSpace(cellText) && i + 1 < cells.Count)
            {
                var nextInput = cells[i + 1].SelectSingleNode(".//input|.//textarea|.//select");
                if (nextInput != null)
                {
                    EmitFormInput(nextInput, CleanLabel(cellText), sb, depth);
                    i += 2;
                    continue;
                }
            }

            if (!string.IsNullOrWhiteSpace(cellText))
                Emit(sb, depth, $"label \"{Esc(cellText)}\"");

            i++;
        }
    }

    private static void EmitFormInput(HtmlNode node, string? label, StringBuilder sb, int depth)
    {
        // Fall back to the name attribute if no label was found
        var cleanLabel = Esc(string.IsNullOrWhiteSpace(label)
            ? CleanLabel(node.GetAttributeValue("name", ""))
            : label!);

        switch (node.Name.ToLowerInvariant())
        {
            case "textarea":
                var ph = Esc(node.GetAttributeValue("placeholder", ""));
                var taSuffix = string.IsNullOrWhiteSpace(ph) ? "" : $" = \"{ph}\"";
                Emit(sb, depth, $"textarea \"{cleanLabel}\"{taSuffix}");
                break;

            case "select":
                Emit(sb, depth, $"select \"{cleanLabel}\"");
                break;

            default: // <input …>
                var type  = node.GetAttributeValue("type", "text").ToLowerInvariant();
                var value = Esc(node.GetAttributeValue("value", ""));
                var hint  = Esc(node.GetAttributeValue("placeholder", ""));

                switch (type)
                {
                    case "checkbox":
                        Emit(sb, depth, $"checkbox \"{cleanLabel}\"");
                        break;
                    case "radio":
                        Emit(sb, depth, $"radio \"{cleanLabel}\"");
                        break;
                    case "submit":
                    case "button":
                    case "reset":
                        var btnVal = Esc(node.GetAttributeValue("value", cleanLabel));
                        Emit(sb, depth, $"button \"{btnVal}\"");
                        break;
                    default:
                        var suffix = !string.IsNullOrWhiteSpace(value) ? $" = \"{value}\""
                                   : !string.IsNullOrWhiteSpace(hint)  ? $" = \"{hint}\""
                                   : "";
                        Emit(sb, depth, $"field \"{cleanLabel}\"{suffix}");
                        break;
                }
                break;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static List<HtmlNode> Children(HtmlNode node) => node.ChildNodes.ToList();

    private static List<HtmlNode> Nodes(HtmlNode node, string xpath) =>
        node.SelectNodes(xpath)?.ToList() ?? [];

    private static bool IsHeading(HtmlNode n) =>
        n.NodeType == HtmlNodeType.Element &&
        n.Name.Length == 2 && n.Name[0] == 'h' && n.Name[1] >= '1' && n.Name[1] <= '6';

    private static int HeadingLevel(HtmlNode n) =>
        IsHeading(n) ? n.Name[1] - '0' : 99;

    private static bool IsFormInput(HtmlNode n) =>
        n.NodeType == HtmlNodeType.Element &&
        (n.Name == "input" || n.Name == "textarea" || n.Name == "select");

    private static bool IsIgnored(HtmlNode n) =>
        n.NodeType == HtmlNodeType.Comment ||
        (n.NodeType == HtmlNodeType.Text &&
         string.IsNullOrWhiteSpace(n.InnerText.Replace(" ", "")));

    private static string Text(HtmlNode node) =>
        WebUtility.HtmlDecode(node.InnerText);

    private static string CellLabel(HtmlNode cell)
    {
        var text = string.Concat(cell.ChildNodes
            .Where(n => n.NodeType == HtmlNodeType.Text)
            .Select(n => n.InnerText));
        return CleanLabel(text);
    }

    private static string CleanLabel(string? text)
    {
        if (text == null) return "";
        text = WebUtility.HtmlDecode(text);
        text = text.Replace(" ", " ").Trim();
        if (text.EndsWith(':')) text = text[..^1].Trim();
        return text;
    }

    private static string CleanText(string text) =>
        WebUtility.HtmlDecode(text).Replace(" ", " ").Trim();

    // Escape double-quotes inside label text so the DSL stays valid
    private static string Esc(string text) =>
        text.Replace("\"", "'").Replace("\n", " ").Replace("\r", "");

    private static void Emit(StringBuilder sb, int depth, string line) =>
        sb.AppendLine(new string(' ', depth * 2) + line);

    private static int CountFields(List<HtmlNode> cells)
    {
        int count = 0;
        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].SelectSingleNode(".//input|.//textarea|.//select") != null)
            {
                count++;
            }
            else if (!string.IsNullOrWhiteSpace(Text(cells[i])) &&
                     i + 1 < cells.Count &&
                     cells[i + 1].SelectSingleNode(".//input|.//textarea|.//select") != null)
            {
                count++;
                i++;
            }
        }
        return count;
    }
}
