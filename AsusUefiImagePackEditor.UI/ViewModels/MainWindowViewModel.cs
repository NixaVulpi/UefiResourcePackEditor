using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Win32;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

using AsusUefiImagePackEditor.Core.Models;
using AsusUefiImagePackEditor.UI.Views;

namespace AsusUefiImagePackEditor.UI.ViewModels;

public partial class MainWindowViewModel: ObservableObject
{
    [ObservableProperty]
    private ResourcePackage? _resourcePackage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WindowTitle))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveAsCommand))]
    private string? _currentFilePath;

    [ObservableProperty]
    private ResourceItemViewModel? _selectedResource;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WindowTitle))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _isModified;

    public ObservableCollection<ResourceItemViewModel> Resources { get; } = [];

    public string WindowTitle
    {
        get
        {
            string title = "ASUS UEFI Image Pack Editor";
            return !string.IsNullOrEmpty(CurrentFilePath) ? $"{Path.GetFileName(CurrentFilePath)}{(IsModified ? " *" : string.Empty)} - {title}" : title;
        }
    }

    public bool CanSaveAs => !string.IsNullOrEmpty(CurrentFilePath);

    public bool CanSave => IsModified && CanSaveAs;

    [RelayCommand]
    private async Task OpenFileAsync()
    {
        if (IsModified)
        {
            MessageBoxResult result = MessageBox.Show(
                "You have unsaved changes. Would you like to save before opening another file?",
                "Unsaved Changes",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                await SaveAsync();
            }
            else if (result == MessageBoxResult.Cancel)
            {
                return;
            }
        }

        OpenFileDialog dialog = new()
        {
            Filter = "Binary Files|*.bin;*.raw|All Files|*.*",
            CheckFileExists = true,
            CheckPathExists = true
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        string path = dialog.FileName;

        try
        {
            using FileStream fileStream = new(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
            ResourcePackage package = await ResourcePackage.ParseFromAsync(fileStream);

            foreach (ResourceItemViewModel resource in Resources)
            {
                resource.PropertyChanged -= OnResourceItemChanged;
            }
            Resources.Clear();
            for (int i = 0; i < package.Blocks.Count; i++)
            {
                ResourceItemViewModel resourceItemViewModel = new(package.Blocks[i], i);
                resourceItemViewModel.PropertyChanged += OnResourceItemChanged;
                Resources.Add(resourceItemViewModel);
            }
            ResourcePackage = package;
            CurrentFilePath = path;
            IsModified = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Failed to open file", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync() => await SaveToFileAsync(CurrentFilePath!);

    [RelayCommand(CanExecute = nameof(CanSaveAs))]
    private async Task SaveAsAsync()
    {
        SaveFileDialog dialog = new()
        {
            Filter = "Binary Files (*.bin)|*.bin|Raw Files (*.raw)|*.raw|All Files (*.*)|*.*",
            DefaultExt = ".bin",
            AddExtension = true
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        string path = dialog.FileName;
        await SaveToFileAsync(path);
        CurrentFilePath = path;
    }

    private async Task SaveToFileAsync(string path)
    {
        if (ResourcePackage is null)
        {
            return;
        }

        try
        {
            using FileStream fileStream = new(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous);
            await ResourcePackage.SerializeToAsync(fileStream);

            IsModified = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Save failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task ExitAsync(object? window)
    {
        if (IsModified)
        {
            MessageBoxResult result = MessageBox.Show(
            "You have unsaved changes. Would you like to save before exiting?",
            "Unsaved Changes",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Warning);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    await SaveAsync();
                    break;
                case MessageBoxResult.No:
                    IsModified = false;
                    break;
                case MessageBoxResult.Cancel:
                    return;
            }
        }

        if (window is Window w)
        {
            await w.Dispatcher.BeginInvoke(new Action(w.Close));
        }
    }

    [RelayCommand]
    private void ShowAbout()
    {
        AboutWindow aboutWindow = new()
        {
            Owner = Application.Current.MainWindow
        };
        aboutWindow.ShowDialog();
    }

    private void OnResourceItemChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IsModified) &&
            ((ResourceItemViewModel) sender).IsModified)
        {
            IsModified = true;
        }
    }

    partial void OnIsModifiedChanged(bool value)
    {
        if (!value)
        {
            foreach (ResourceItemViewModel resource in Resources)
            {
                resource.IsModified = false;
            }
        }
    }
}
