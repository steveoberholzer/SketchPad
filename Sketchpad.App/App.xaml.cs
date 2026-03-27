using System.Windows;
using System.Windows.Threading;

namespace Sketchpad.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Catch exceptions that reach the dispatcher message loop
        DispatcherUnhandledException += (_, ex) =>
        {
            ShowError(ex.Exception);
            ex.Handled = true;
            Shutdown(1);
        };

        // Catch exceptions on background threads
        AppDomain.CurrentDomain.UnhandledException += (_, ex) =>
        {
            if (ex.ExceptionObject is Exception exception)
                ShowError(exception);
        };

        try
        {
            var window = new MainWindow();
            MainWindow = window;
            window.Show();
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
