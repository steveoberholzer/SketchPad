using System.Windows;
using System.Windows.Media;
using Sketchpad.App.Highlighting;

namespace Sketchpad.App.AI;

public partial class GenerateWindow : Window
{
    public string? GeneratedDsl { get; private set; }

    public GenerateWindow()
    {
        InitializeComponent();
        DslPreview.SyntaxHighlighting = SketchpadHighlighting.Definition;
        DslPreview.TextArea.TextView.LineTransformers.Add(new CommentColouriser());

        ApiKeyBox.Password = AiSettings.AnthropicApiKey;

        PromptBox.Focus();
    }

    private async void Generate_Click(object sender, RoutedEventArgs e)
    {
        var prompt = PromptBox.Text.Trim();
        var apiKey = ApiKeyBox.Password.Trim();

        if (string.IsNullOrEmpty(prompt))
        {
            SetStatus("Please describe the screen you want to generate.", Brushes.DarkRed);
            PromptBox.Focus();
            return;
        }

        if (string.IsNullOrEmpty(apiKey))
        {
            SetStatus("Please enter your Anthropic API key.", Brushes.DarkRed);
            ApiKeyBox.Focus();
            return;
        }

        AiSettings.AnthropicApiKey = apiKey;

        GenerateButton.IsEnabled = false;
        GenerateButton.Content   = "Generating…";
        InsertButton.IsEnabled   = false;
        DslPreview.Text          = string.Empty;
        GeneratedDsl             = null;
        SetStatus("Calling Claude API…", Brushes.DarkGoldenrod);

        try
        {
            var dsl = await AiService.GenerateDslAsync(prompt, apiKey);
            DslPreview.Text      = dsl;
            GeneratedDsl         = dsl;
            InsertButton.IsEnabled = true;
            SetStatus("Done — review the DSL then click Insert into Editor.", Brushes.DarkGreen);
        }
        catch (Exception ex)
        {
            SetStatus($"Error: {ex.Message}", Brushes.DarkRed);
        }
        finally
        {
            GenerateButton.IsEnabled = true;
            GenerateButton.Content   = "Generate";
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
