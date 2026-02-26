using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;

namespace AsusUefiImagePackEditor.UI;

public partial class App: Application
{
    public App()
    {
        CultureInfo cultureInfo = CultureInfo.GetCultureInfo(0x0409);
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += ApplicationDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += TaskSchedulerUnobservedTaskException;

        base.OnStartup(e);
    }

    private void ApplicationDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        ShowErrorDialog("UI Thread Unhandled Exception", e.Exception);
    }

    private void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        string title = e.IsTerminating ? "Fatal Application Error" : "Global Unhandled Exception";
        ShowErrorDialog(title, (Exception) e.ExceptionObject);
    }

    private void TaskSchedulerUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        e.SetObserved();
        ShowErrorDialog("Unobserved Task Exception", e.Exception);
    }

    private static void ShowErrorDialog(string title, Exception ex)
    {
        string message = $"An unexpected error occurred:\n\n{ex.Message}\n\nDetails:\n{ex.StackTrace}";
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
