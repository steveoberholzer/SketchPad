using System.Text;
using Sketchpad.Core.Ast;
using Sketchpad.Core.Rendering;

namespace Sketchpad.Renderers.Sketch;

/// <summary>
/// Emits a self-contained HTML/CSS string from a UiDocument.
/// Output type is string, not UIElement, so it is wired to Export rather than live preview.
/// </summary>
public class HtmlRenderer : IUiRenderer<string>
{
    public string DisplayName => "HTML Export";

    public string Render(UiDocument document)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\"><head><meta charset=\"UTF-8\">");
        sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine("<title>Sketchpad Export</title>");
        sb.AppendLine("<style>");
        sb.AppendLine(Css);
        sb.AppendLine("</style></head><body>");

        foreach (var root in document.Roots)
            RenderNode(root, sb, 0);

        sb.AppendLine("</body></html>");
        return sb.ToString();
    }

    private void RenderNode(UiNode node, StringBuilder sb, int depth)
    {
        string indent = new string(' ', depth * 2);
        switch (node.Type)
        {
            case ElementType.Window:
                sb.AppendLine($"{indent}<div class=\"sp-window\">");
                sb.AppendLine($"{indent}  <div class=\"sp-window-titlebar\"><span>{Esc(node.Label)}</span></div>");
                sb.AppendLine($"{indent}  <div class=\"sp-window-body\">");
                foreach (var c in node.Children) RenderNode(c, sb, depth + 2);
                sb.AppendLine($"{indent}  </div>");
                sb.AppendLine($"{indent}</div>");
                break;

            case ElementType.Panel:
                sb.AppendLine($"{indent}<div class=\"sp-panel\">");
                if (node.Label != null) sb.AppendLine($"{indent}  <div class=\"sp-panel-title\">{Esc(node.Label)}</div>");
                foreach (var c in node.Children) RenderNode(c, sb, depth + 1);
                sb.AppendLine($"{indent}</div>");
                break;

            case ElementType.Card:
                string cardCls = node.HasModifier("warning") ? "sp-card sp-card--warning"
                               : node.HasModifier("danger")  ? "sp-card sp-card--danger"
                               : "sp-card";
                sb.AppendLine($"{indent}<div class=\"{cardCls}\">");
                if (node.Label != null) sb.AppendLine($"{indent}  <div class=\"sp-card-title\">{Esc(node.Label)}</div>");
                foreach (var c in node.Children) RenderNode(c, sb, depth + 1);
                sb.AppendLine($"{indent}</div>");
                break;

            case ElementType.Row:
                sb.AppendLine($"{indent}<div class=\"sp-row\">");
                foreach (var c in node.Children) RenderNode(c, sb, depth + 1);
                sb.AppendLine($"{indent}</div>");
                break;

            case ElementType.Col:
                sb.AppendLine($"{indent}<div class=\"sp-col\">");
                foreach (var c in node.Children) RenderNode(c, sb, depth + 1);
                sb.AppendLine($"{indent}</div>");
                break;

            case ElementType.Navbar:
                sb.AppendLine($"{indent}<nav class=\"sp-navbar\">");
                foreach (var c in node.Children) RenderNode(c, sb, depth + 1);
                sb.AppendLine($"{indent}</nav>");
                break;

            case ElementType.Brand:
                sb.AppendLine($"{indent}<span class=\"sp-brand\">{Esc(node.Label)}</span>");
                break;

            case ElementType.Menu:
                string menuCls = node.HasModifier("right") ? "sp-menu sp-menu--right" : "sp-menu";
                sb.AppendLine($"{indent}<div class=\"{menuCls}\">");
                foreach (var c in node.Children) RenderNode(c, sb, depth + 1);
                sb.AppendLine($"{indent}</div>");
                break;

            case ElementType.Sidebar:
                sb.AppendLine($"{indent}<aside class=\"sp-sidebar\">");
                foreach (var c in node.Children) RenderNode(c, sb, depth + 1);
                sb.AppendLine($"{indent}</aside>");
                break;

            case ElementType.Nav:
                sb.AppendLine($"{indent}<ul class=\"sp-nav\">");
                foreach (var c in node.Children) RenderNode(c, sb, depth + 1);
                sb.AppendLine($"{indent}</ul>");
                break;

            case ElementType.Item:
                string itemCls = node.HasModifier("active") ? "sp-item sp-item--active" : "sp-item";
                sb.AppendLine($"{indent}<li class=\"{itemCls}\">{Esc(node.Label)}</li>");
                break;

            case ElementType.Tab:
                string tabCls = node.HasModifier("active") ? "sp-tab sp-tab--active" : "sp-tab";
                sb.AppendLine($"{indent}<button class=\"{tabCls}\">{Esc(node.Label)}</button>");
                break;

            case ElementType.Tabs:
                sb.AppendLine($"{indent}<div class=\"sp-tabs\">");
                foreach (var c in node.Children) RenderNode(c, sb, depth + 1);
                sb.AppendLine($"{indent}</div>");
                break;

            case ElementType.Divider:
                sb.AppendLine($"{indent}<hr class=\"sp-divider\"/>");
                break;

            case ElementType.Spacer:
                sb.AppendLine($"{indent}<div class=\"sp-spacer\"></div>");
                break;

            case ElementType.Button:
                string btnCls = node.HasModifier("primary") ? "sp-btn sp-btn--primary"
                              : node.HasModifier("danger")  ? "sp-btn sp-btn--danger"
                              : "sp-btn";
                sb.AppendLine($"{indent}<button class=\"{btnCls}\">{Esc(node.Label)}</button>");
                break;

            case ElementType.Field:
                sb.AppendLine($"{indent}<div class=\"sp-field\">");
                if (node.Label != null) sb.AppendLine($"{indent}  <label>{Esc(node.Label)}</label>");
                sb.AppendLine($"{indent}  <input type=\"text\" placeholder=\"{Esc(node.Value)}\"/>");
                sb.AppendLine($"{indent}</div>");
                break;

            case ElementType.Textarea:
                sb.AppendLine($"{indent}<div class=\"sp-field\">");
                if (node.Label != null) sb.AppendLine($"{indent}  <label>{Esc(node.Label)}</label>");
                sb.AppendLine($"{indent}  <textarea rows=\"4\"></textarea>");
                sb.AppendLine($"{indent}</div>");
                break;

            case ElementType.Select:
                sb.AppendLine($"{indent}<div class=\"sp-field\">");
                if (node.Label != null) sb.AppendLine($"{indent}  <label>{Esc(node.Label)}</label>");
                sb.AppendLine($"{indent}  <select><option>{Esc(node.Value)}</option></select>");
                sb.AppendLine($"{indent}</div>");
                break;

            case ElementType.Checkbox:
                bool chkd = node.HasModifier("checked");
                sb.AppendLine($"{indent}<label class=\"sp-checkbox\"><input type=\"checkbox\"{(chkd ? " checked" : "")}/> {Esc(node.Label)}</label>");
                break;

            case ElementType.Radio:
                sb.AppendLine($"{indent}<label class=\"sp-checkbox\"><input type=\"radio\"/> {Esc(node.Label)}</label>");
                break;

            case ElementType.Toggle:
                sb.AppendLine($"{indent}<label class=\"sp-toggle\">{Esc(node.Label)}</label>");
                break;

            case ElementType.Slider:
                sb.AppendLine($"{indent}<div class=\"sp-field\"><label>{Esc(node.Label)}</label><input type=\"range\"/></div>");
                break;

            case ElementType.Label:
                string lblCls = node.HasModifier("muted") ? "sp-label sp-label--muted" : "sp-label";
                sb.AppendLine($"{indent}<span class=\"{lblCls}\">{Esc(node.Label)}</span>");
                break;

            case ElementType.Text:
                sb.AppendLine($"{indent}<p class=\"sp-text\">{Esc(node.Label)}</p>");
                break;

            case ElementType.Heading:
                sb.AppendLine($"{indent}<h2 class=\"sp-heading\">{Esc(node.Label)}</h2>");
                break;

            case ElementType.Avatar:
                string avCls = node.HasModifier("circle") ? "sp-avatar sp-avatar--circle" : "sp-avatar";
                sb.AppendLine($"{indent}<div class=\"{avCls}\"></div>");
                break;

            case ElementType.Image:
                sb.AppendLine($"{indent}<div class=\"sp-image\"><span>{Esc(node.Label ?? "image")}</span></div>");
                break;

            case ElementType.Badge:
                sb.AppendLine($"{indent}<span class=\"sp-badge\">{Esc(node.Label)}</span>");
                break;

            case ElementType.Tag:
                sb.AppendLine($"{indent}<span class=\"sp-tag\">{Esc(node.Label)} ×</span>");
                break;

            case ElementType.Table:
                RenderTable(node, sb, depth);
                break;

            case ElementType.Alert:
                string alertCls = node.HasModifier("warning") ? "sp-alert sp-alert--warning"
                                : node.HasModifier("danger")  ? "sp-alert sp-alert--danger"
                                : node.HasModifier("success") ? "sp-alert sp-alert--success"
                                : "sp-alert sp-alert--info";
                sb.AppendLine($"{indent}<div class=\"{alertCls}\">{Esc(node.Label)}</div>");
                break;

            case ElementType.Spinner:
                sb.AppendLine($"{indent}<div class=\"sp-spinner\"></div>");
                break;

            case ElementType.Progress:
                int pct = 0;
                foreach (var m in node.Modifiers) if (int.TryParse(m, out var v)) pct = v;
                sb.AppendLine($"{indent}<div class=\"sp-progress\"><div class=\"sp-progress-fill\" style=\"width:{pct}%\"></div></div>");
                break;

            case ElementType.Icon:
                sb.AppendLine($"{indent}<span class=\"sp-icon\">{Esc(node.Label?[..1] ?? "?")}</span>");
                break;

            default:
                sb.AppendLine($"{indent}<div class=\"sp-unknown\">[{node.Type}]</div>");
                break;
        }
    }

    private void RenderTable(UiNode node, StringBuilder sb, int depth)
    {
        string indent = new string(' ', depth * 2);
        sb.AppendLine($"{indent}<table class=\"sp-table\">");

        var cols = node.Children.FirstOrDefault(c => c.Type == ElementType.Columns);
        if (cols?.Label != null)
        {
            sb.AppendLine($"{indent}  <thead><tr>");
            foreach (var h in cols.Label.Split('|'))
                sb.AppendLine($"{indent}    <th>{Esc(h.Trim())}</th>");
            sb.AppendLine($"{indent}  </tr></thead>");
        }

        sb.AppendLine($"{indent}  <tbody>");
        foreach (var row in node.Children.Where(c => c.Type == ElementType.Row))
        {
            sb.AppendLine($"{indent}    <tr>");
            foreach (var cell in (row.Label ?? "").Split('|'))
                sb.AppendLine($"{indent}      <td>{Esc(cell.Trim())}</td>");
            sb.AppendLine($"{indent}    </tr>");
        }
        sb.AppendLine($"{indent}  </tbody>");
        sb.AppendLine($"{indent}</table>");
    }

    private static string Esc(string? s) =>
        s == null ? "" :
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");

    private const string Css = """
        *, *::before, *::after { box-sizing: border-box; }
        body { font-family: 'Segoe UI', system-ui, sans-serif; font-size: 14px;
               color: #2A2A2A; background: #fff; margin: 0; padding: 16px; }
        .sp-window { border: 1px solid #9A9A9A; display: inline-block; min-width: 400px; }
        .sp-window-titlebar { background: #F0F0EE; border-bottom: 1px solid #9A9A9A;
                              padding: 8px 12px; font-size: 12px; color: #555; }
        .sp-window-body { padding: 0; }
        .sp-panel { margin: 4px; }
        .sp-panel-title { background: #F0F0EE; border-bottom: 1px solid #ddd;
                         padding: 6px 12px; font-weight: 600; font-size: 12px; }
        .sp-card { border: 1px solid #9A9A9A; border-radius: 4px; padding: 12px; margin: 4px; background: #F8F8F6; }
        .sp-card--warning { background: #FFF3CD; border-color: #FFC107; }
        .sp-card--danger  { background: #FFEBEB; border-color: #C0392B; }
        .sp-card-title { font-weight: 600; font-size: 13px; border-bottom: 1px solid #ddd;
                        padding-bottom: 8px; margin-bottom: 8px; }
        .sp-row { display: flex; gap: 8px; align-items: flex-start; margin-bottom: 8px; }
        .sp-row > * { flex: 1; }
        .sp-col { display: flex; flex-direction: column; gap: 8px; }
        .sp-navbar { display: flex; align-items: center; background: #F0F0EE;
                    border-bottom: 1px solid #9A9A9A; height: 48px; padding: 0 12px; gap: 8px; }
        .sp-brand { font-weight: bold; font-size: 15px; margin-right: 12px; }
        .sp-menu { display: flex; gap: 4px; }
        .sp-menu--right { margin-left: auto; }
        .sp-sidebar { border-right: 1px solid #9A9A9A; padding: 8px; width: 200px;
                     background: #F8F8F6; display: flex; flex-direction: column; gap: 4px; }
        .sp-nav { list-style: none; margin: 0; padding: 0; display: flex; flex-direction: column; gap: 2px; }
        .sp-item { padding: 6px 8px; border-radius: 4px; cursor: pointer; color: #888; font-size: 13px; }
        .sp-item--active { background: #E8E8E8; color: #2A2A2A; font-weight: 600; }
        .sp-tabs { display: flex; border-bottom: 1px solid #9A9A9A; }
        .sp-tab { padding: 8px 16px; background: none; border: none; cursor: pointer;
                 color: #888; font-size: 13px; border-bottom: 2px solid transparent; }
        .sp-tab--active { color: #2A2A2A; font-weight: 600; border-bottom-color: #2A2A2A; }
        .sp-divider { border: none; border-top: 1px solid #9A9A9A; margin: 4px 0; }
        .sp-spacer { height: 16px; }
        .sp-btn { padding: 7px 16px; border: 1px solid #9A9A9A; border-radius: 4px;
                 background: #F8F8F6; cursor: pointer; font-size: 13px; margin-right: 4px; }
        .sp-btn--primary { background: #2A2A2A; color: white; border-color: #2A2A2A; }
        .sp-btn--danger   { background: #C0392B; color: white; border-color: #C0392B; }
        .sp-field { display: flex; flex-direction: column; gap: 2px; margin-bottom: 4px; }
        .sp-field label { font-size: 11px; color: #888; }
        .sp-field input, .sp-field select, .sp-field textarea {
            border: 1px solid #9A9A9A; border-radius: 3px; padding: 5px 8px;
            font-size: 13px; font-family: Consolas, monospace; background: white; }
        .sp-checkbox { display: flex; align-items: center; gap: 6px; font-size: 13px; cursor: pointer; }
        .sp-toggle { font-size: 13px; }
        .sp-label { font-size: 12px; }
        .sp-label--muted { color: #888; }
        .sp-text { margin: 0 0 4px; font-size: 13px; }
        .sp-heading { font-size: 18px; font-weight: bold; margin: 0 0 8px; }
        .sp-avatar { width: 40px; height: 40px; background: #CCC; display: inline-block; margin-bottom: 4px; }
        .sp-avatar--circle { border-radius: 50%; }
        .sp-image { width: 200px; height: 150px; background: #F8F8F6; border: 1px solid #9A9A9A;
                   display: flex; align-items: center; justify-content: center; color: #888; font-size: 11px; }
        .sp-badge { background: #E2E8F0; border-radius: 10px; padding: 2px 8px; font-size: 11px; margin-right: 4px; }
        .sp-tag   { background: #E2E8F0; border-radius: 4px; padding: 2px 6px; font-size: 11px; margin-right: 4px; }
        .sp-table { border-collapse: collapse; width: 100%; margin: 4px 0; border: 1px solid #9A9A9A; border-radius: 4px; }
        .sp-table th { background: #F0F0EE; padding: 6px 8px; font-size: 12px; font-weight: 600;
                      border-bottom: 1px solid #9A9A9A; text-align: left; }
        .sp-table td { padding: 6px 8px; font-size: 12px; border-bottom: 1px solid #EEE; }
        .sp-alert { padding: 10px 14px; border-radius: 4px; border-left: 4px solid; margin: 4px 0; font-size: 13px; }
        .sp-alert--info    { background: #CCE5FF; border-color: #17A2B8; }
        .sp-alert--warning { background: #FFF3CD; border-color: #FFC107; }
        .sp-alert--danger  { background: #FFEBEB; border-color: #C0392B; }
        .sp-alert--success { background: #D4EDDA; border-color: #28A745; }
        .sp-spinner { width: 24px; height: 24px; border: 3px dashed #9A9A9A; border-radius: 50%; display: inline-block; }
        .sp-progress { height: 8px; background: #9A9A9A; border-radius: 4px; overflow: hidden; margin: 4px 0; }
        .sp-progress-fill { height: 100%; background: #2A2A2A; }
        .sp-icon { display: inline-block; width: 24px; height: 24px; background: #CCC;
                  border-radius: 4px; text-align: center; line-height: 24px; font-size: 11px; }
        .sp-unknown { border: 1px dashed #9A9A9A; border-radius: 3px; padding: 4px 6px;
                     color: #888; font-size: 11px; display: inline-block; }
        """;
}
