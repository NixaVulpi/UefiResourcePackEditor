using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;

namespace UefiResourcePackEditor.UI.Views;

public partial class AboutWindow: Window
{
    public AboutWindow()
    {
        InitializeComponent();
        LoadVersionInfo();
    }

    private void LoadVersionInfo()
    {
        string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "Unknown";
        VersionTextBlock.Text = $"Version: {version}";
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }

    private void CopyInfoButton_Click(object sender, RoutedEventArgs e)
    {
        string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "Unknown";
        string info = $"UEFI Resource Pack Editor v{version}\nAuthor: NixaVulpi\nGitHub: https://github.com/NixaVulpi";
        Clipboard.SetText(info);
        MessageBox.Show("Version information copied to clipboard.", "Copied", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
