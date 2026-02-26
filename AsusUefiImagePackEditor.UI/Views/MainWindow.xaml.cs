using AsusUefiImagePackEditor.UI.ViewModels;

using System.ComponentModel;
using System.Windows;

namespace AsusUefiImagePackEditor.UI.Views
{
    public partial class MainWindow: Window
    {
        private readonly MainWindowViewModel _viewModel;

        public MainWindow()
        {
            _viewModel = new();
            DataContext = _viewModel;
            InitializeComponent();
        }

        protected override async void OnClosing(CancelEventArgs e)
        {
            if (!_viewModel.IsModified)
            {
                base.OnClosing(e);
                return;
            }

            e.Cancel = true;
            await _viewModel.ExitCommand.ExecuteAsync(this);
        }
    }
}
