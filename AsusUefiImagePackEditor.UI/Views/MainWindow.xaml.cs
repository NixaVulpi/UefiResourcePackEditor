using System.Windows;

using AsusUefiImagePackEditor.UI.ViewModels;

namespace AsusUefiImagePackEditor.UI.Views
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
