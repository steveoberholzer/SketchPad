# Sketchpad

A lightweight, text-based UI mockup tool. Describe your interface in a simple DSL — Sketchpad parses it and renders a live sketch-style preview as you type.

![Sketchpad layout: editor on the left, rendered preview on the right](docs/screenshot.png)

---

## What it looks like

```sketchpad
window "Invoice INV-2024-0089" [1020x820]

  navbar
    brand "Billing"
    menu [right]
      item "Print"
      item "Export PDF"

  panel
    card "Line Items"
      table
        columns "Description, Qty, Unit Price, Amount"
        row "Strategy Consulting, 15 hrs, $200.00, $3,000.00"
        row "UX Design Sprint, 1 sprint, $4,500.00, $4,500.00"
      divider
      row
        spacer
        col
          row
            label "Subtotal" [muted]
            label "$7,500.00"
          row
            label "Total due"
            heading "$8,250.00"

    row
      button "Mark as paid" [primary]
      button "Void invoice" [danger]
```

Type that on the left, see a rendered wireframe on the right — instantly, on every keystroke.

---

## Features

- **Live preview** — 300 ms debounced re-render on every keystroke
- **Sketch-style rendering** — off-white fills, grey borders, hand-picked typography
- **Syntax highlighting** — keywords in blue, strings in green, modifiers in orange, comments in grey
- **Line numbers** — via AvalonEdit
- **Sample library** — six real-world layouts you can load and adapt (File > Samples)
- **HTML export** — emits a self-contained HTML + inline CSS file
- **Lenient parser** — never crashes; collects all errors and still renders what it can
- **Renderer plug-in interface** — swap the output fidelity without touching the parser

---

## Getting started

### Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (Windows only — WPF)
- Visual Studio 2022 or `dotnet` CLI

### Run

```bash
git clone https://github.com/you/sketchpad.git
cd sketchpad
dotnet run --project Sketchpad.App
```

### Test

```bash
dotnet test Sketchpad.Core.Tests
```

---

## The DSL

One element per line. Indentation defines nesting (2 spaces per level). Tabs are normalised to 2 spaces.

```
<type> ["label"] [= "value"] [modifier, modifier, ...]
```

### Quick reference

#### Layout

| Element | Description |
|---|---|
| `window "Title" [900x600]` | Top-level window chrome; width×height is a canvas hint |
| `panel "Title"` | Generic container with optional title bar |
| `card "Title"` | Elevated card (border + background) |
| `row` | Horizontal flex container; children share width equally |
| `col` | Vertical flex container |
| `divider` | Horizontal rule |
| `spacer` | Flexible gap |

#### Navigation

| Element | Description |
|---|---|
| `navbar` | Full-width top bar (48 px) |
| `brand "MyApp"` | Logo/name inside a navbar |
| `menu [right]` | Group of items; `[right]` aligns to the right of a navbar |
| `sidebar [220px]` | Left sidebar; optional pixel-width modifier |
| `nav` | Vertical navigation list |
| `item "Label" [active]` | Nav or menu item; `[active]` highlights it |
| `tabs` | Tab strip container |
| `tab "Label" [active]` | Individual tab |

#### Form

| Element | Description |
|---|---|
| `field "Label" = "placeholder"` | Labelled text input |
| `textarea "Label"` | Multi-line text input |
| `checkbox "Label" [checked]` | Checkbox |
| `radio "Label"` | Radio button |
| `select "Label" = "Option A"` | Dropdown |
| `toggle "Label"` | Toggle switch |
| `slider "Label"` | Range slider |
| `button "Label" [primary]` | Action button |

#### Display

| Element | Description |
|---|---|
| `label "Text" [muted]` | Short text label |
| `text "Body copy"` | Paragraph text |
| `heading "Title"` | Section heading |
| `avatar [circle]` | User avatar placeholder |
| `image "alt" [200x150]` | Image placeholder with diagonal cross |
| `badge "Text"` | Inline status pill |
| `tag "Text"` | Removable chip |
| `table` | Data table (with `columns` and `row` children) |
| `icon "search"` | Named icon placeholder |

#### Feedback

| Element | Description |
|---|---|
| `alert "Message" [warning]` | Inline alert banner |
| `toast "Message" [success]` | Toast notification hint |
| `spinner` | Loading indicator (static) |
| `progress [60]` | Progress bar; modifier is fill percentage |

### Modifiers

| Modifier | Applies to | Effect |
|---|---|---|
| `primary` | button | Dark fill, white text |
| `danger` | button, card | Red treatment |
| `warning` | card, alert | Amber/yellow treatment |
| `success` | alert, toast | Green treatment |
| `muted` | label, text | Secondary grey colour |
| `wide` | field, button | Spans full container width |
| `right` | menu | Right-aligns children within a navbar |
| `active` | item, tab | Selected/highlighted state |
| `circle` | avatar | Circular crop |
| `checked` | checkbox | Pre-checked state |
| `disabled` | any | Greyed out |
| `NNNpx` | sidebar | Explicit pixel width, e.g. `[220px]` |
| `NNNxNNN` | window, image | Width × height hint, e.g. `[1024x768]` |
| `NN` | progress | Fill percentage, e.g. `[60]` |

Unknown modifiers are silently ignored (forward-compatibility).

### Comments and blank lines

```sketchpad
# This is a comment — ignored by the parser
# Blank lines are also ignored

window "My App"
```

### Complete example

```sketchpad
# User profile — sidebar + multi-section form
window "User Profile" [980x700]

  navbar
    brand "MyApp"
    menu [right]
      item "Settings"
      item "Logout"

  row
    sidebar [220px]
      avatar [circle]
      label "Steven Oberholzer"
      label "Developer" [muted]
      divider
      nav
        item "Overview" [active]
        item "Projects"
        item "Billing"

    panel
      card "Personal Details"
        row
          field "First name" = "Steven"
          field "Last name"  = "Oberholzer"
        row
          field "Email" = "steve@example.com" [wide]
        row
          button "Save changes" [primary]
          button "Cancel"

      card "Danger Zone" [warning]
        text "Once you delete your account, there is no going back."
        button "Delete account" [danger]
```

---

## Sample library

Open **File > Samples** (or click **Samples…** in the toolbar) to browse six built-in real-world layouts:

| Sample | What it demonstrates |
|---|---|
| **Item View** | Read-only record — sidebar avatar, 3-column detail rows, activity table |
| **Item Edit** | Editable form — Personal / Organisation / Address cards |
| **List View** | Searchable, filterable data table with pagination hint |
| **List Edit** | Master/detail split — list on the left, edit form on the right |
| **Parent + Child Grid** | Invoice header with line-items table and running subtotal |
| **Multi-column Form** | Settings page mixing labels-on-top, labels-to-the-left, and 3-column rows |

Select a sample, preview its markup (with syntax highlighting), then click **Use This Sample** to load it into the editor.

---

## Project structure

```
Sketchpad.sln
├── Sketchpad.Core/                  # Pure C# — no WPF dependency
│   ├── Ast/
│   │   ├── ElementType.cs           # Enum of all element types
│   │   ├── UiNode.cs                # Immutable AST node (record)
│   │   └── UiDocument.cs            # Parse result: roots + errors
│   ├── Parsing/
│   │   ├── ParseError.cs            # record(Line, Message)
│   │   ├── Lexer.cs                 # Tokenises a single DSL line
│   │   └── Parser.cs                # Single-pass indent-tracking parser
│   └── Rendering/
│       └── IUiRenderer.cs           # Generic renderer interface
│
├── Sketchpad.Renderers/             # WPF class library
│   ├── Sketch/
│   │   ├── SketchRenderer.cs        # Sketch-style WPF UIElement renderer
│   │   └── SketchTheme.cs           # Colours, fonts, spacing constants
│   └── Html/
│       └── HtmlRenderer.cs          # Self-contained HTML + CSS emitter
│
├── Sketchpad.App/                   # WPF application
│   ├── Highlighting/
│   │   ├── Sketchpad.xshd           # AvalonEdit syntax definition
│   │   └── SketchpadHighlighting.cs # Loads the XSHD as an embedded resource
│   ├── Samples/
│   │   └── SampleLibrary.cs         # Six built-in sample DSL strings
│   ├── MainWindow.xaml / .cs        # Editor + live preview shell
│   └── SamplesWindow.xaml / .cs     # Sample browser dialog
│
└── Sketchpad.Core.Tests/            # xUnit — parser and lexer tests (27 tests)
```

### Key design decisions

- **`Sketchpad.Core` has zero WPF references.** The parser and AST are plain .NET 8 and can be used headlessly (e.g. in a CLI or server-side tool).
- **`IUiRenderer<TOutput>` is generic.** `SketchRenderer` outputs `UIElement`; `HtmlRenderer` outputs `string`. Both share the same `UiDocument` input.
- **The parser is lenient.** It never throws. Unknown element types produce a `ParseError` with a line number but are still attached to the tree as `ElementType.Unknown` nodes.
- **AST nodes are immutable records.** Renderers must not mutate the tree.

---

## Adding a renderer

1. Create a class in `Sketchpad.Renderers` (or a new project) that implements `IUiRenderer<UIElement>`.
2. Handle every `ElementType` — render a labelled placeholder box for anything you haven't implemented yet.
3. Silently ignore unknown modifiers.
4. Never throw — catch exceptions and emit an error placeholder node.
5. Register your renderer in `MainWindow.xaml.cs`:

```csharp
_renderers = [new SketchRenderer(), new YourRenderer()];
```

It will appear automatically in the Renderer dropdown.

---

## Adding a sample

Add a new `Sample` entry to `SampleLibrary.cs`:

```csharp
public static readonly Sample MyNewSample = new(
    "My Layout",
    "One-line description shown in the samples list.",
    """
    window "My Layout" [900x600]
      ...
    """);
```

Then add it to the `All` list at the bottom of the class. The samples window picks it up automatically.

---

## Roadmap

| Phase | Status | Scope |
|---|---|---|
| 1 — Core | Done | Parser, AST, SketchRenderer, WPF shell, file I/O |
| 2 — Element completeness | Planned | Remaining elements, px width hints, inline error squiggles, renderer switcher |
| 3 — Wireframe renderer | Planned | Clean WPF render — no jitter, system-blue accents, proper type scale |
| 4 — HTML export | Done (basic) | Self-contained HTML/CSS; optional WebView2 preview |
| 5 — Polish | Planned | Syntax highlighting ✓, zoom controls, PNG export |

---

## License

MIT
