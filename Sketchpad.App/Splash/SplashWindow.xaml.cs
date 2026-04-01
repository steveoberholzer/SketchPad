using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Sketchpad.App.Splash;

public partial class SplashWindow : Window
{
    public SplashWindow()
    {
        InitializeComponent();
        IconImage.Source = AppIcon.Generate(68);
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Fade the whole window in
        BeginAnimation(OpacityProperty,
            new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(350)));

        // Scanning bar — slides from left to right, repeating
        var scan = new DoubleAnimation
        {
            From           = -90,
            To             = 380,   // track width = 460 - 2×40 margin
            Duration       = TimeSpan.FromSeconds(1.5),
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut },
        };
        LoadingTranslate.BeginAnimation(TranslateTransform.XProperty, scan);
    }

    /// <summary>
    /// Fades the splash out and invokes <paramref name="onComplete"/> when done.
    /// </summary>
    public void FadeOutAndClose(Action onComplete)
    {
        var fade = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(400));
        fade.Completed += (_, _) => onComplete();
        BeginAnimation(OpacityProperty, fade);
    }
}
