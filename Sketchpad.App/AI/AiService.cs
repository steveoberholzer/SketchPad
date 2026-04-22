using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Sketchpad.App.AI;

public static class AiService
{
    private static readonly HttpClient _http = new();

    public static async Task<string> GenerateDslAsync(string userPrompt, string apiKey)
    {
        var body = new
        {
            model = "claude-sonnet-4-6",
            max_tokens = 2048,
            system = SystemPrompt,
            messages = new[] { new { role = "user", content = userPrompt } }
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        req.Headers.Add("x-api-key", apiKey);
        req.Headers.Add("anthropic-version", "2023-06-01");
        req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var resp = await _http.SendAsync(req);
        var raw  = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
        {
            var msg = $"API returned {(int)resp.StatusCode}";
            try
            {
                using var err = JsonDocument.Parse(raw);
                if (err.RootElement.TryGetProperty("error", out var e) &&
                    e.TryGetProperty("message", out var m))
                    msg = m.GetString() ?? msg;
            }
            catch (JsonException) { }
            throw new InvalidOperationException(msg);
        }

        using var doc = JsonDocument.Parse(raw);
        var text = doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? string.Empty;

        return StripFences(text.Trim());
    }

    private static string StripFences(string text)
    {
        if (!text.StartsWith("```")) return text;
        var nl = text.IndexOf('\n');
        if (nl < 0) return text;
        text = text[(nl + 1)..];
        if (text.EndsWith("```")) text = text[..^3].TrimEnd();
        return text;
    }

    // ── System prompt ─────────────────────────────────────────────────────────
    // Embeds the full DSL grammar, the fallback substitution rule, and three
    // representative examples so Claude can produce correct output zero-shot.

    private const string SystemPrompt = """
        You generate UI mockup DSL for the Sketchpad tool.
        Given a plain-English description of a screen, output ONLY valid Sketchpad DSL.
        No explanation. No markdown code fences. No preamble. Raw DSL only.

        ── DSL Rules ──────────────────────────────────────────────────────────────
        One element per line. Indentation = nesting (2 spaces per level).
        Syntax:  <type> ["label"] [= "value"] [modifier, ...]
        Comments start with #. Blank lines are ignored.

        ── Layout ─────────────────────────────────────────────────────────────────
        window "Title" [900x600]  — top-level window (width×height is a canvas hint)
        panel "Title"             — generic container with optional title
        card "Title"              — elevated card (border + background)
        row                       — horizontal flex container
        col                       — vertical flex container
        divider                   — horizontal rule
        spacer                    — flexible gap

        ── Navigation ─────────────────────────────────────────────────────────────
        navbar                    — full-width top bar
        brand "Name"              — logo/name inside a navbar
        menu [right]              — group of items; [right] aligns to the navbar's right edge
        sidebar [220px]           — left sidebar; optional pixel-width modifier
        nav                       — vertical navigation list
        item "Label" [active]     — nav or menu item; [active] = highlighted
        tabs                      — tab strip container
        tab "Label" [active]      — individual tab

        ── Form ───────────────────────────────────────────────────────────────────
        field "Label" = "hint"          — labelled text input
        textarea "Label"                — multi-line text area
        checkbox "Label" [checked]      — checkbox
        radio "Label"                   — radio button
        select "Label" = "Value"        — dropdown
        toggle "Label"                  — toggle switch
        slider "Label"                  — range slider
        datepicker "Label" = "YYYY-MM-DD"
        datetimepicker "Label" = "YYYY-MM-DD HH:MM"
        button "Label" [primary]        — action button

        ── Display ────────────────────────────────────────────────────────────────
        label "Text" [muted]      — short text; [muted] = secondary grey
        text "Copy"               — paragraph text
        heading "Title"           — section heading
        avatar [circle]           — user avatar placeholder
        image "alt" [200x150]     — image placeholder with diagonal cross
        badge "Text"              — inline status pill
        tag "Text"                — removable chip
        table                     — data table; children are columns and row
          columns "Col A | Col B | Col C"
          row "Value A | Value B | Value C"
        calendar "Label"          — month-view calendar
        icon "name"               — named icon placeholder

        ── Feedback ───────────────────────────────────────────────────────────────
        alert "Message" [warning] — inline alert; modifiers: warning, danger, success
        toast "Message" [success] — toast notification
        spinner                   — loading indicator
        progress [60]             — progress bar; modifier = fill %

        ── Modifiers ──────────────────────────────────────────────────────────────
        primary   button: dark fill + white text
        danger    button/card: red
        warning   card/alert: amber
        success   alert/toast: green
        muted     label/text: secondary grey
        wide      field/button: full container width
        right     menu: right-align children in navbar
        active    item/tab: selected state
        circle    avatar: circular crop
        checked   checkbox: pre-checked
        disabled  any: greyed out
        NNNpx     sidebar explicit width, e.g. [240px]
        NNNxNNN   window/image width×height, e.g. [1024x768]
        NN        progress fill %, e.g. [75]

        ── Fallback Rule ───────────────────────────────────────────────────────────
        If the user requests a widget that does not exist in the DSL above, substitute
        the closest available element and add a # comment on the same line explaining:
          tree view / treeview / hierarchy     →  table
          date range / from–to date pair       →  two datepicker elements in a row
          rich text editor / WYSIWYG           →  textarea
          map / chart / graph / diagram        →  image "Description" [400x250]
          rating / stars                       →  row of badge elements
          unknown selector / combo / picker    →  select
          unknown single-line text input       →  field
          unknown multi-line text input        →  textarea
          unknown toggle / switch              →  toggle
          any other unknown display widget     →  label

        ── Examples ────────────────────────────────────────────────────────────────

        ### Example 1 — read-only contact view (sidebar + detail cards + activity table)
        # Contact Detail
        window "Contact Detail" [980x680]

          navbar
            brand "CRM"
            menu [right]
              item "Edit"
              item "Delete"

          row
            sidebar [240px]
              avatar [circle]
              heading "Sarah Mitchell"
              label "Senior Engineer" [muted]
              label "Acme Corp" [muted]
              divider
              label "Tags" [muted]
              row
                badge "VIP"
                badge "Technical"
              divider
              label "Last contact" [muted]
              label "10 Mar 2024"

            col
              card "Contact Information"
                row
                  col
                    label "Email" [muted]
                    label "sarah@acmecorp.com"
                  col
                    label "Phone" [muted]
                    label "+1 (555) 234-5678"
                  col
                    label "Location" [muted]
                    label "San Francisco, CA"
              card "Recent Activity"
                table
                  columns "Date | Activity | Notes"
                  row "2024-03-10 | Meeting | Discussed Q2 roadmap"
                  row "2024-02-28 | Email | Sent proposal"
                  row "2024-02-15 | Call | Product demo"

        ### Example 2 — searchable list view with pagination
        # Product List
        window "Products" [1100x700]

          navbar
            brand "Inventory"
            menu [right]
              item "New Product"
              item "Import CSV"

          panel
            row
              heading "All Products"
              button "New Product" [primary]
            row
              field "Search" = "Filter products..."
              select "Category" = "All"
              select "Status" = "All statuses"
            table
              columns "SKU | Name | Price | Stock | Status"
              row "PRD-001 | Wireless Headphones | $129.99 | 48 | Active"
              row "PRD-002 | Laptop Stand | $49.99 | 112 | Active"
              row "PRD-003 | USB-C Hub | $79.99 | 23 | Low stock"
              row "PRD-004 | Monitor Arm | $89.99 | 7 | Low stock"
            row
              label "Showing 4 of 38 products" [muted]
              button "Previous" [disabled]
              button "Next"

        ### Example 3 — settings page (sidebar nav + multi-section form)
        # Account Settings
        window "Account Settings" [1020x800]

          navbar
            brand "MyApp"
            menu [right]
              item "Log out"

          row
            sidebar [200px]
              nav
                item "Profile" [active]
                item "Security"
                item "Notifications"
                item "Billing"

            panel
              card "Personal Information"
                row
                  field "First name" = "Steven"
                  field "Last name" = "Oberholzer"
                  field "Display name" = "steve_o"
                row
                  field "Email address" = "steve@example.com" [wide]
                row
                  select "Time zone" = "UTC+11"
                  select "Language" = "English (AU)"

              card "Preferences"
                row
                  label "Theme"
                  select = "System default"
                row
                  label "Compact mode"
                  toggle "Enable compact UI"

              card "Danger Zone" [warning]
                row
                  col
                    label "Delete account"
                    text "Permanently remove your account. This cannot be undone."
                  button "Delete account" [danger]

              row
                button "Save changes" [primary]
                button "Cancel"
        """;
}
