using System;
using System.Threading.Tasks;
using System.Windows;

namespace UefiResourcePackEditor.UI;

public partial class App: Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += ApplicationDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += TaskSchedulerUnobservedTaskException;

        base.OnStartup(e);
    }

    private void ApplicationDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        ShowErrorDialog("UI Thread Error", string.Empty, e.Exception);
        e.Handled = true;
    }

    private void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Exception exception = (Exception) e.ExceptionObject;
        string message = e.IsTerminating ? "A fatal error occurred in a background thread. The application will now close." : "An unhandled error occurred in a background thread.";
        ShowErrorDialog("Unhandled Exception", message, exception);
    }

    private void TaskSchedulerUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        e.SetObserved();
        ShowErrorDialog("Unobserved Task Exception", string.Empty, e.Exception);
    }

    private static void ShowErrorDialog(string title, string message, Exception ex)
    {
        string prefix = string.Empty;
        if (!string.IsNullOrEmpty(message))
        {
            prefix = $"{message}\n\n";
        }
        MessageBox.Show($"{prefix}An unexpected error occurred:\n\n{ex.Message}\n\nDetails:\n{ex.StackTrace}", title, MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
