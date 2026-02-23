using System.Windows;

using UefiResourcePackEditor.UI.ViewModels;

namespace UefiResourcePackEditor.UI.Views
{
    public partial class MainWindow: Window
    {
        public MainWindow()
        {
            DataContext = new MainWindowViewModel();
            InitializeComponent();
        }
    }
}
