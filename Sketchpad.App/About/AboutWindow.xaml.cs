using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Sketchpad.App.About;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        IconImage.Source = AppIcon.Generate(48);
        Loaded += (_, _) =>
            BeginAnimation(OpacityProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200)));
    }

    private void Ok_Click(object sender, RoutedEventArgs e) => Close();

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) Close();
    }
}
