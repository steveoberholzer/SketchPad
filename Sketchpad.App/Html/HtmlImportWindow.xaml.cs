using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Sketchpad.App.Highlighting;
using Sketchpad.Core.Html;

namespace Sketchpad.App.Html;

public partial class HtmlImportWindow : Window
{
    public string? ImportedDsl { get; private set; }

    private readonly DispatcherTimer _debounce;

    public HtmlImportWindow()
    {
        InitializeComponent();
        DslPreview.SyntaxHighlighting = SketchpadHighlighting.Definition;
        DslPreview.TextArea.TextView.LineTransformers.Add(new CommentColouriser());

        _debounce = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _debounce.Tick += (_, _) => { _debounce.Stop(); RunConversion(); };

        // Pre-populate from clipboard if it looks like HTML
        var clip = Clipboard.GetText();
        if (!string.IsNullOrWhiteSpace(clip) && clip.TrimStart().StartsWith('<'))
        {
            HtmlBox.Text = clip;
            RunConversion();
        }
        else
        {
            HtmlBox.Focus();
        }
    }

    private void HtmlBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        _debounce.Stop();
        _debounce.Start();
    }

    private void RunConversion()
    {
        var html = HtmlBox.Text;
        if (string.IsNullOrWhiteSpace(html))
        {
            DslPreview.Text        = string.Empty;
            ImportedDsl            = null;
            InsertButton.IsEnabled = false;
            SetStatus("Paste or type HTML on the left.", Brushes.Gray);
            return;
        }

        try
        {
            var dsl = HtmlConverter.Convert(html);
            DslPreview.Text        = dsl;
            ImportedDsl            = dsl;
            InsertButton.IsEnabled = !string.IsNullOrWhiteSpace(dsl);
            SetStatus(string.IsNullOrWhiteSpace(dsl)
                ? "No recognisable elements found in the HTML."
                : "Ready — review the DSL then click Insert into Editor.",
                string.IsNullOrWhiteSpace(dsl) ? Brushes.DarkOrange : Brushes.DarkGreen);
        }
        catch (Exception ex)
        {
            SetStatus($"Conversion error: {ex.Message}", Brushes.DarkRed);
        }
    }

    private void Insert_Click(object sender, RoutedEventArgs e) =>
        DialogResult = true;

    private void Close_Click(object sender, RoutedEventArgs e) =>
        DialogResult = false;

    private void SetStatus(string text, Brush colour)
    {
        StatusText.Text       = text;
        StatusText.Foreground = colour;
    }
}
