using System.Windows;
using System.Windows.Controls;
using Sketchpad.App.Highlighting;
using Sketchpad.App.Samples;

namespace Sketchpad.App;

public partial class SamplesWindow : Window
{
    /// <summary>Set by the caller after ShowDialog() returns true.</summary>
    public string? ChosenDsl { get; private set; }

    public SamplesWindow()
    {
        InitializeComponent();
        DslPreview.SyntaxHighlighting = SketchpadHighlighting.Definition;
        DslPreview.TextArea.TextView.LineTransformers.Add(new CommentColouriser());
        SampleList.ItemsSource = SampleLibrary.All;
    }

    private void SampleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SampleList.SelectedItem is Sample sample)
        {
            PreviewTitle.Text  = $"{sample.Name} — {sample.Description}";
            DslPreview.Text    = sample.Dsl.TrimStart('\r', '\n');
            UseButton.IsEnabled = true;
        }
    }

    private void UseButton_Click(object sender, RoutedEventArgs e)
    {
        if (SampleList.SelectedItem is Sample sample)
        {
            ChosenDsl    = sample.Dsl.TrimStart('\r', '\n');
            DialogResult = true;
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) =>
        DialogResult = false;
}
