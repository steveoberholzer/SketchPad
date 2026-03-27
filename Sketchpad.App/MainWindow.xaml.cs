using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;
using Sketchpad.App.Highlighting;
using Sketchpad.Core.Ast;
using Sketchpad.Core.Rendering;
using Sketchpad.Renderers.Sketch;

namespace Sketchpad.App;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer           _debounce;
    private readonly List<IUiRenderer<UIElement>> _renderers;
    private IUiRenderer<UIElement>            _activeRenderer;
    private string?                           _currentFilePath;
    private UiDocument?                       _lastDocument;

    public MainWindow()
    {
        InitializeComponent();

        // Syntax highlighting (XSHD handles keywords/strings/modifiers;
        // comments are handled by a separate transformer — see CommentColouriser.cs)
        EditorBox.SyntaxHighlighting = SketchpadHighlighting.Definition;
        EditorBox.TextArea.TextView.LineTransformers.Add(new CommentColouriser());

        // Renderers
        _renderers      = [new SketchRenderer()];
        _activeRenderer = _renderers[0];
        RendererCombo.ItemsSource   = _renderers.Select(r => r.DisplayName).ToList();
        RendererCombo.SelectedIndex = 0;

        // Debounced live preview
        _debounce = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        _debounce.Tick += (_, _) => { _debounce.Stop(); ParseAndRender(); };

        EditorBox.TextChanged += (_, _) => { _debounce.Stop(); _debounce.Start(); };

        EditorBox.Text = DefaultContent();
    }

    // ── Keyboard shortcuts ────────────────────────────────────────────────────

    protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == System.Windows.Input.Key.N &&
            e.KeyboardDevice.Modifiers == System.Windows.Input.ModifierKeys.Control)
            New_Click(this, new RoutedEventArgs());

        else if (e.Key == System.Windows.Input.Key.O &&
            e.KeyboardDevice.Modifiers == System.Windows.Input.ModifierKeys.Control)
            Open_Click(this, new RoutedEventArgs());

        else if (e.Key == System.Windows.Input.Key.S &&
            e.KeyboardDevice.Modifiers == System.Windows.Input.ModifierKeys.Control)
            Save_Click(this, new RoutedEventArgs());
    }

    // ── Menu / toolbar events ─────────────────────────────────────────────────

    private void New_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmDiscard()) return;
        _currentFilePath = null;
        EditorBox.Text   = DefaultContent();
        Title            = "Sketchpad";
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmDiscard()) return;
        var dlg = new OpenFileDialog
        {
            Filter     = "Sketchpad files (*.sketchpad)|*.sketchpad|All files (*.*)|*.*",
            DefaultExt = ".sketchpad",
        };
        if (dlg.ShowDialog() == true)
        {
            _currentFilePath = dlg.FileName;
            EditorBox.Text   = File.ReadAllText(dlg.FileName);
            Title            = $"Sketchpad — {Path.GetFileName(dlg.FileName)}";
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (_currentFilePath == null) SaveAs();
        else File.WriteAllText(_currentFilePath, EditorBox.Text);
    }

    private void SaveAs_Click(object sender, RoutedEventArgs e) => SaveAs();

    private void ExportHtml_Click(object sender, RoutedEventArgs e)
    {
        if (_lastDocument == null) return;
        var html = new HtmlRenderer().Render(_lastDocument);
        var dlg  = new SaveFileDialog
        {
            Filter   = "HTML files (*.html)|*.html",
            FileName = Path.GetFileNameWithoutExtension(_currentFilePath ?? "mockup") + ".html",
        };
        if (dlg.ShowDialog() == true)
            File.WriteAllText(dlg.FileName, html);
    }

    private void Samples_Click(object sender, RoutedEventArgs e)
    {
        var win = new SamplesWindow { Owner = this };
        if (win.ShowDialog() == true && win.ChosenDsl != null)
        {
            if (!ConfirmDiscard()) return;
            _currentFilePath = null;
            EditorBox.Text   = win.ChosenDsl;
            Title            = "Sketchpad";
        }
    }

    private void RendererCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RendererCombo.SelectedIndex >= 0 && RendererCombo.SelectedIndex < _renderers.Count)
        {
            _activeRenderer = _renderers[RendererCombo.SelectedIndex];
            ParseAndRender();
        }
    }

    // ── Core logic ────────────────────────────────────────────────────────────

    private void ParseAndRender()
    {
        try
        {
            _lastDocument = Core.Parsing.Parser.Parse(EditorBox.Text);
            PreviewHost.Content = _activeRenderer.Render(_lastDocument);

            int nodeCount = CountNodes(_lastDocument.Roots);

            if (_lastDocument.HasErrors)
            {
                StatusText.Text       = $"⚠ {_lastDocument.Errors.Count} error(s)";
                StatusText.Foreground = System.Windows.Media.Brushes.DarkRed;
            }
            else
            {
                StatusText.Text       = "✓ No errors";
                StatusText.Foreground = System.Windows.Media.Brushes.DarkGreen;
            }

            NodeCountText.Text    = $"{nodeCount} nodes";
            RendererNameText.Text = _activeRenderer.DisplayName;
        }
        catch (Exception ex)
        {
            StatusText.Text       = $"Error: {ex.Message}";
            StatusText.Foreground = System.Windows.Media.Brushes.DarkRed;
        }
    }

    private static int CountNodes(IReadOnlyList<UiNode> nodes)
    {
        int count = nodes.Count;
        foreach (var node in nodes)
            count += CountNodes(node.Children);
        return count;
    }

    private bool ConfirmDiscard()
    {
        if (_currentFilePath != null &&
            File.Exists(_currentFilePath) &&
            File.ReadAllText(_currentFilePath) != EditorBox.Text)
        {
            return MessageBox.Show(
                "You have unsaved changes. Discard them?",
                "Sketchpad", MessageBoxButton.YesNo, MessageBoxImage.Question)
                    == MessageBoxResult.Yes;
        }
        return true;
    }

    private void SaveAs()
    {
        var dlg = new SaveFileDialog
        {
            Filter     = "Sketchpad files (*.sketchpad)|*.sketchpad",
            DefaultExt = ".sketchpad",
            FileName   = "mockup",
        };
        if (dlg.ShowDialog() == true)
        {
            _currentFilePath = dlg.FileName;
            File.WriteAllText(_currentFilePath, EditorBox.Text);
            Title = $"Sketchpad — {Path.GetFileName(_currentFilePath)}";
        }
    }

    private static string DefaultContent() => """
        # Welcome to Sketchpad — describe your UI mockup below.
        # The preview updates live. Use  File > Samples  for real-world examples.

        window "My App" [900x600]

          navbar
            brand "MyApp"
            menu [right]
              item "Settings"
              item "Logout"

          panel
            card "Login"
              heading "Sign in"
              field "Email" = "user@example.com"
              field "Password" = "••••••••"
              row
                button "Sign in" [primary]
                button "Cancel"
              alert "Invalid credentials" [danger]
        """;
}
