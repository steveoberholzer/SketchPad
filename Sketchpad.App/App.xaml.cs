using System.Windows;
using System.Windows.Threading;
using Sketchpad.App.Splash;

namespace Sketchpad.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += (_, ex) =>
        {
            ShowError(ex.Exception);
            ex.Handled = true;
            Shutdown(1);
        };

        AppDomain.CurrentDomain.UnhandledException += (_, ex) =>
        {
            if (ex.ExceptionObject is Exception exception)
                ShowError(exception);
        };

        try
        {
            // Splash screen — shown first, fades out after a minimum display time
            var splash = new SplashWindow();
            splash.Show();

            var window = new MainWindow();
            MainWindow = window;

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1800) };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                splash.FadeOutAndClose(() =>
                {
                    window.Show();
                    window.Activate();
                });
            };
            timer.Start();
        }
        catch (Exception ex)
        {
            ShowError(ex);
            Shutdown(1);
        }
    }

    private static void ShowError(Exception ex) =>
        MessageBox.Show(
            $"{ex.GetType().Name}\n\n{ex.Message}\n\n--- Stack trace ---\n{ex.StackTrace}" +
            (ex.InnerException is { } inner ? $"\n\n--- Inner ---\n{inner.Message}\n{inner.StackTrace}" : ""),
            "Sketchpad — startup error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
}
