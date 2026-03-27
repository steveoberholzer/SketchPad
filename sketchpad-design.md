# Sketchpad — UI Mockup DSL & WPF Renderer
## Design Document v0.1

---

## Overview

Sketchpad is a lightweight, text-based DSL for describing UI mockups, paired with a WPF desktop application that parses and renders them. The language prioritises authoring speed over precision: you describe *what* and *containment*, never *coordinates*. The renderer handles layout.

The architecture is explicitly designed for renderer plug-in extensibility. v1 targets a sketch-quality WPF canvas. Later renderers (clean wireframe, HTML/CSS export, Figma API) reuse the same parser and AST without modification.

---

## Goals

- **Author a mockup in under 2 minutes** for a typical login form or dashboard panel
- **No coordinate system** — indentation implies containment; the renderer handles flow
- **Single C# codebase** for parser, AST, and all renderers
- **Renderer plug-in interface** — swap output fidelity without touching the parser
- **Live preview** — parse and re-render on every keystroke with debounce

### Out of Scope for v1

- Animation or interactive prototyping
- Bi-directional editing (render → DSL round-trip)
- Multi-file includes or component libraries
- Custom themes or colour schemes

---

## The Language — Sketchpad DSL

### Core Syntax Rules

1. **One element per line**
2. **Indentation defines nesting** — use 2 spaces per level (tabs are normalised to 2 spaces)
3. **Element type is the first token** on a line (case-insensitive)
4. **Optional label** follows the type, quoted with double quotes
5. **Optional modifiers** in square brackets, comma-separated: `[primary, wide]`
6. **Optional value** assigned with `=`: `field "Email" = "user@example.com"`
7. **Comments** start with `#` and are ignored by the parser
8. **Blank lines** are ignored

### Modifier Reference

Modifiers are hints to the renderer. Unknown modifiers are silently ignored (forward compatibility).

| Modifier | Applies to | Meaning |
|---|---|---|
| `primary` | button | Primary action style |
| `danger` | button, card | Destructive action style |
| `warning` | card | Warning colour treatment |
| `muted` | label, text | Secondary text style |
| `wide` | field, button | Spans full container width |
| `right` | menu, toolbar | Right-align children |
| `active` | item | Currently selected state |
| `circle` | avatar | Render as circle crop |
| `checked` | checkbox | Pre-checked state |
| `disabled` | any | Greyed out, non-interactive |
| `horizontal` | row, stack | Explicit horizontal layout (default for `row`) |
| `vertical` | row, stack | Explicit vertical layout |

### Element Reference

#### Layout Elements

```
window "Title" [800x600]     # Top-level window chrome. WxH is a hint to canvas size.
panel "Title"                # Generic container with optional title bar
card "Title"                 # Elevated container (border/shadow in hi-fi)
row                          # Horizontal flex container
col                          # Vertical flex container (default layout for most containers)
divider                      # Horizontal rule
spacer                       # Flexible gap
```

#### Navigation Elements

```
navbar                       # Top navigation bar
sidebar [200px]              # Left sidebar, optional width hint
menu [right]                 # Menu group, optional alignment
nav                          # Vertical navigation list
item "Label" [active]        # Navigation or menu item
tabs                         # Tab strip container
tab "Label" [active]         # Individual tab
```

#### Form Elements

```
field "Label" = "value"      # Labelled text input with optional placeholder/default
textarea "Label"             # Multi-line text input
checkbox "Label" [checked]   # Checkbox with label
radio "Label"                # Radio button
select "Label" = "Option A"  # Dropdown select
toggle "Label"               # Toggle switch
slider "Label"               # Range slider
button "Label" [primary]     # Action button
```

#### Display Elements

```
label "Text" [muted]         # Text label
text "Body copy here"        # Paragraph text
heading "Title"              # Section heading (h2-equivalent)
avatar [circle]              # User avatar placeholder
image "alt text" [200x150]   # Image placeholder
badge "Text"                 # Inline status badge
tag "Text"                   # Removable tag chip
table                        # Data table
  columns "Name, Email, Role"
  row "Alice, alice@example.com, Admin"
  row "Bob, bob@example.com, Viewer"
icon "search"                # Named icon placeholder
```

#### Feedback Elements

```
alert "Message" [warning]    # Inline alert banner (info/warning/danger/success modifiers)
toast "Message" [success]    # Toast notification overlay hint
spinner                      # Loading indicator
progress [60]                # Progress bar, optional fill percentage
```

### Complete Example

```sketchpad
# User profile page
window "User Profile" [900x700]

  navbar
    brand "MyApp"
    menu [right]
      item "Settings"
      item "Logout"

  sidebar [220px]
    avatar [circle]
    label "Steven Oberholzer"
    label "Developer" [muted]
    divider
    nav
      item "Overview" [active]
      item "Projects"
      item "Activity"
      item "Billing"

  panel
    card "Personal Details"
      row
        field "First name" = "Steven"
        field "Last name"  = "Oberholzer"
      row
        field "Email" = "steve@example.com" [wide]
      row
        field "Phone" = "+61 4xx xxx xxx"
        select "Country" = "Australia"
      row
        button "Save changes" [primary]
        button "Cancel"

    card "Danger Zone" [warning]
      text "Once you delete your account, there is no going back."
      button "Delete account" [danger]
```

---

## Architecture

```
┌─────────────────────────────────────────────────────┐
│                   WPF Application                   │
│                                                     │
│  ┌─────────────┐    ┌──────────┐    ┌────────────┐  │
│  │  EditorPane │───▶│  Parser  │───▶│    AST     │  │
│  │  (TextBox)  │    │          │    │ UiDocument │  │
│  └─────────────┘    └──────────┘    └─────┬──────┘  │
│                                           │         │
│                                  IUiRenderer        │
│                                           │         │
│                          ┌────────────────┼──────┐  │
│                          ▼                ▼      ▼  │
│                    SketchRenderer  WireRenderer  …   │
│                          │                          │
│                    ┌─────▼──────┐                   │
│                    │ PreviewPane│                   │
│                    │ (Canvas /  │                   │
│                    │  WebView2) │                   │
│                    └────────────┘                   │
└─────────────────────────────────────────────────────┘
```

### Project Structure

```
Sketchpad.sln
├── Sketchpad.Core/                  # Parser, AST, renderer interface — no WPF dependency
│   ├── Parsing/
│   │   ├── Lexer.cs
│   │   ├── Parser.cs
│   │   └── ParseError.cs
│   ├── Ast/
│   │   ├── UiDocument.cs
│   │   ├── UiNode.cs
│   │   ├── ElementType.cs
│   │   └── Modifier.cs
│   └── Rendering/
│       ├── IUiRenderer.cs
│       └── RenderContext.cs
├── Sketchpad.Renderers/
│   ├── Sketch/
│   │   ├── SketchRenderer.cs        # v1 — WPF canvas, pencil style
│   │   └── SketchTheme.cs
│   ├── Wireframe/
│   │   └── WireframeRenderer.cs     # v2 — clean SVG/WPF, no colour
│   └── Html/
│       └── HtmlRenderer.cs          # v3 — HTML/CSS string emitter
└── Sketchpad.App/                   # WPF application shell
    ├── App.xaml
    ├── MainWindow.xaml
    ├── MainWindow.xaml.cs
    └── ViewModels/
        └── MainViewModel.cs
```

---

## Core Data Model

### `UiNode.cs`

```csharp
namespace Sketchpad.Core.Ast;

public class UiNode
{
    public ElementType Type       { get; init; }
    public string?     Label      { get; init; }   // quoted string after type
    public string?     Value      { get; init; }   // rhs of = "..."
    public IReadOnlyList<string> Modifiers { get; init; } = [];
    public IReadOnlyList<UiNode> Children  { get; init; } = [];
    public int         Line       { get; init; }   // 1-based, for error reporting

    public bool HasModifier(string modifier) =>
        Modifiers.Contains(modifier, StringComparer.OrdinalIgnoreCase);
}
```

### `UiDocument.cs`

```csharp
namespace Sketchpad.Core.Ast;

public class UiDocument
{
    public IReadOnlyList<UiNode>  Roots      { get; init; } = [];
    public IReadOnlyList<ParseError> Errors  { get; init; } = [];
    public bool HasErrors => Errors.Count > 0;
}
```

### `ElementType.cs`

```csharp
namespace Sketchpad.Core.Ast;

public enum ElementType
{
    Unknown,

    // Layout
    Window, Panel, Card, Row, Col, Divider, Spacer,

    // Navigation
    Navbar, Sidebar, Menu, Nav, Item, Tabs, Tab,

    // Form
    Field, Textarea, Checkbox, Radio, Select, Toggle, Slider, Button,

    // Display
    Label, Text, Heading, Avatar, Image, Badge, Tag, Table, Columns, Icon,

    // Feedback
    Alert, Toast, Spinner, Progress,

    // Navigation sub-elements
    Brand,
}
```

### `IUiRenderer.cs`

```csharp
namespace Sketchpad.Core.Rendering;

/// <summary>
/// All renderers implement this interface. The output type is generic
/// because sketch renders to a WPF UIElement, HTML renders to a string, etc.
/// </summary>
public interface IUiRenderer<TOutput>
{
    TOutput Render(UiDocument document);
    string  DisplayName { get; }   // shown in renderer selector dropdown
}
```

---

## Parser Design

The parser is a single-pass, line-by-line recursive descent over indented text. No tokeniser is needed beyond splitting each line.

### Algorithm

```
for each non-empty, non-comment line:
    indent = count leading spaces (tabs → 2 spaces each)
    level  = indent / 2
    parse the line into (elementType, label?, value?, modifiers[])
    
    while indent_stack.top.level >= level:
        pop indent_stack   // close containers that are shallower

    attach new node as child of indent_stack.top
    push new node onto indent_stack
```

### Line Parsing

Each line after stripping indent matches the pattern:

```
<type> [<"label">] [= "<value>"] [[ <modifier>, ... ]]
```

Order of `label`, `value`, and `[modifiers]` on the line must be respected in that sequence. Parse with a simple character-walk or `Span<char>` split — no regex needed.

**Examples:**

```
field "Email" = "user@example.com" [wide]
button "Save" [primary, wide]
window "Dashboard" [1024x768]
avatar [circle]
label "Secondary text" [muted]
```

### Error Handling

The parser is lenient by design. It should:

- Emit a `ParseError` with line number and message for unrecognised element types (set `Type = ElementType.Unknown`, still attach to tree)
- Never throw — return a `UiDocument` even on partial parses
- Collect all errors rather than stopping at the first

```csharp
public record ParseError(int Line, string Message);
```

---

## Sketch Renderer — v1

The sketch renderer targets a WPF `Canvas`. It walks the AST depth-first and emits WPF `FrameworkElement` objects. It applies a hand-drawn visual style: slightly irregular borders, off-white fills, a monospaced placeholder font.

### Visual Style Targets

| Property | Value |
|---|---|
| Font family | Segoe UI (body), Consolas (field values) |
| Border style | `RectangleGeometry` with 1–2px jitter on corners |
| Colour palette | Near-white fills (#F8F8F6), mid-grey borders (#9A9A9A), dark text (#2A2A2A) |
| Button primary | Solid dark fill (#2A2A2A), white text |
| Button danger | Red fill (#C0392B), white text |
| Muted text | #888888 |
| Shadow | None in v1; flat borders only |

Jitter is achieved by offsetting each corner of a `PolyLineSegment` by `Random.Shared.NextDouble() * jitterAmount` at render time. Keep `jitterAmount` small (0.5–2.0 px) for readability.

### Layout Model

The renderer uses a simple **vertical stacking** layout within containers, with `row` switching to horizontal stacking. It does not implement a full flexbox — just:

- **Column flow**: children stack top-to-bottom with a fixed gap (8px default)
- **Row flow**: children stack left-to-right with a fixed gap (8px default), equal width by default
- **Padding**: 12px inside card/panel, 8px inside row
- **Width**: containers expand to fill parent width; rows divide width equally among children unless a `[Npx]` modifier is present

There is no wrapping in v1. If content overflows, it clips.

### Renderer Interface (WPF-specific)

```csharp
namespace Sketchpad.Renderers.Sketch;

public class SketchRenderer : IUiRenderer<UIElement>
{
    public string DisplayName => "Sketch";

    public UIElement Render(UiDocument document)
    {
        var canvas = new Canvas { Background = Brushes.White };
        var context = new LayoutContext(canvas, availableWidth: 800);
        foreach (var root in document.Roots)
            RenderNode(root, context);
        return canvas;
    }

    private void RenderNode(UiNode node, LayoutContext ctx) { ... }
}
```

### Element-to-Visual Mapping (v1)

| Element | WPF Visual |
|---|---|
| `window` | Root `Canvas` with title bar `Border` |
| `panel`, `card` | `Border` with rounded corners, inner `StackPanel` |
| `navbar` | `Border` (full width, 48px tall), inner horizontal `StackPanel` |
| `sidebar` | `Border` (fixed width from modifier), inner `StackPanel` |
| `row` | `StackPanel` Horizontal |
| `col` | `StackPanel` Vertical |
| `divider` | 1px `Rectangle` |
| `field` | `StackPanel` with `TextBlock` (label) + `Border` + `TextBlock` (value) |
| `button` | `Border` with fill + `TextBlock` centred |
| `label`, `text` | `TextBlock` |
| `heading` | `TextBlock` with larger font weight |
| `avatar` | `Ellipse` (circle modifier) or `Rectangle`, grey fill |
| `checkbox` | `StackPanel` with small `Border` (square) + `TextBlock` |
| `select` | Like `field` but with a `▾` glyph |
| `badge`, `tag` | Inline `Border` with rounded corners + `TextBlock` |
| `table` | `Grid` with header row + data rows |
| `alert` | `Border` with left accent colour bar + `TextBlock` |
| `spinner` | `Ellipse` with dashed stroke (static in v1) |
| `progress` | Two nested `Rectangle`s |
| `image` | `Rectangle` with diagonal cross lines (placeholder convention) |

---

## WPF Application Shell

### Main Window Layout

```
┌──────────────────────────────────────────────────────────┐
│  Toolbar: [Open] [Save] [New]     Renderer: [Sketch ▾]   │
├────────────────────────┬─────────────────────────────────┤
│                        │                                 │
│   Editor (TextBox)     │   Preview (ScrollViewer)        │
│   monospaced font      │   live-rendered output          │
│   line numbers         │                                 │
│                        │                                 │
├────────────────────────┴─────────────────────────────────┤
│  Status bar:  ✓ No errors  |  23 nodes  |  Sketch        │
└──────────────────────────────────────────────────────────┘
```

The splitter between editor and preview should be draggable. Default split: 40% editor / 60% preview.

### Live Preview

- Bind the `TextChanged` event on the editor `TextBox`
- Debounce: 300ms after last keystroke before re-parsing
- Use `DispatcherTimer` for debounce
- On parse + render completion, replace the `ScrollViewer` content with the new `UIElement`
- Display error count in status bar; show first error message on hover

### Renderer Selection

A `ComboBox` in the toolbar lists available renderers by `DisplayName`. Changing the selection triggers an immediate re-render of the current document. Renderers are registered at startup:

```csharp
var renderers = new IUiRenderer<UIElement>[]
{
    new SketchRenderer(),
    new WireframeRenderer(),   // v2
};
```

The `HtmlRenderer` (which emits a string, not a `UIElement`) is a different output type — in v1 it is wired to a "Export HTML…" menu action that opens a Save dialog, not the live preview.

### File Handling

- `.sketchpad` file extension
- Plain UTF-8 text
- Open / Save / Save As via standard `OpenFileDialog` / `SaveFileDialog`
- No autosave in v1

---

## Extensibility Contract

Any future renderer must:

1. Implement `IUiRenderer<TOutput>` (or a new `IUiRenderer<string>` base for text-output renderers)
2. Handle every `ElementType` value — fall through to a generic placeholder box for unknown types
3. Silently ignore unknown modifiers
4. Never throw — catch and render an error placeholder node instead

The `UiNode` and `UiDocument` types are sealed from the renderer's perspective. Renderers must not mutate the AST.

---

## Implementation Phases

### Phase 1 — Core (target: working end-to-end)

- [ ] `UiNode`, `UiDocument`, `ElementType`, `ParseError` data model
- [ ] `Parser` — line reader, indent tracker, line tokeniser
- [ ] `SketchRenderer` for: `window`, `panel`, `card`, `row`, `col`, `divider`, `field`, `button`, `label`, `text`, `heading`, `navbar`, `sidebar`, `nav`, `item`, `avatar`, `checkbox`
- [ ] WPF shell: editor + live preview + status bar
- [ ] File open / save

### Phase 2 — Element Completeness

- [ ] Remaining elements: `table`, `badge`, `tag`, `alert`, `progress`, `spinner`, `image`, `select`, `tabs`, `tab`, `toggle`, `slider`, `textarea`, `radio`, `icon`
- [ ] Modifier: `[Npx]` width hints on `sidebar`, `image`, `window`
- [ ] Error highlighting in editor (underline parse errors inline)
- [ ] Renderer switcher in toolbar

### Phase 3 — Wireframe Renderer

- [ ] `WireframeRenderer` — clean WPF render, no jitter, system-blue accents, proper typography scale

### Phase 4 — HTML Export

- [ ] `HtmlRenderer` — emits a self-contained HTML file with inline CSS
- [ ] Export to file via menu
- [ ] Optional: WebView2 preview pane for hi-fi output

### Phase 5 — Polish

- [ ] Line numbers in editor
- [ ] Syntax highlighting (element types in blue, modifiers in orange, values in green)
- [ ] Zoom controls on preview pane
- [ ] PNG export (render WPF visual to `RenderTargetBitmap`)

---

## NuGet Dependencies

| Package | Purpose |
|---|---|
| None for Core | Parser and AST are pure C# with no dependencies |
| `Microsoft.Web.WebView2` | Phase 4 HTML preview pane |
| `SkiaSharp.Views.WPF` | Optional: if sketch jitter moves to SkiaSharp in Phase 3+ |

The WPF application itself targets **.NET 8** with the `Microsoft.NET.Sdk` WPF SDK.

---

## Notes for Claude Code

- The `Sketchpad.Core` project must have **zero WPF references** — it is a pure class library. Renderers reference Core; the WPF app references both.
- All parsing logic lives in `Core`. The WPF app only wires events and hosts the window.
- `IUiRenderer<UIElement>` is WPF-specific and lives in `Sketchpad.Renderers`, not `Core`. The `Core` assembly only knows about `IUiRenderer<TOutput>` as a generic concept if needed; the concrete WPF binding lives in Renderers.
- The parser should be unit-testable without WPF. Include a `Sketchpad.Core.Tests` xUnit project from the start.
- Use `record` types for `UiNode` and `ParseError` — immutability is intentional.
- The renderer does **not** need to handle all 100% of edge cases in v1. A graceful placeholder (`?` box with the element type label) is acceptable for unimplemented elements.
- Prefer `StackPanel` over `Grid` in the sketch renderer unless column alignment is semantically important (e.g., `table`). Grid layout math is complex and not needed for sketch fidelity.
- The live preview debounce **must** marshal back to the UI thread. Use `Dispatcher.InvokeAsync` if parsing is moved to a background thread in a later phase.
