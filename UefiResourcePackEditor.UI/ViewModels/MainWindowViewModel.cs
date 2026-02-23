using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Win32;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

using UefiResourcePackEditor.Core.Models;
using UefiResourcePackEditor.UI.Views;

namespace UefiResourcePackEditor.UI.ViewModels;

public partial class MainWindowViewModel: ObservableObject
{
    [ObservableProperty]
    private ResourcePackage? _resourcePackage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WindowTitle))]
    private string? _currentFilePath;

    [ObservableProperty]
    private ResourceItemViewModel? _selectedResource;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WindowTitle))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveAsCommand))]
    private bool _isModified;

    public ObservableCollection<ResourceItemViewModel> Resources { get; } = [];

    public string WindowTitle
    {
        get
        {
            string title = "UEFI Resource Pack Editor";
            return !string.IsNullOrEmpty(CurrentFilePath) ? $"{Path.GetFileName(CurrentFilePath)}{(IsModified ? " *" : string.Empty)} - {title}" : title;
        }
    }

    public bool CanSave => IsModified && !string.IsNullOrEmpty(CurrentFilePath);

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
            Filter = "UEFI Resource Files|*.bin;*.raw|All Files|*.*",
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

            ResourcePackage = package;
            Resources.Clear();
            for (int i = 0; i < package.Blocks.Count; i++)
            {
                Resources.Add(new ResourceItemViewModel(package.Blocks[i], i));
            }

            CurrentFilePath = path;
            IsModified = false;
            MessageBox.Show($"Loaded {Resources.Count} resources", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to open file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync() => await SaveToFileAsync(CurrentFilePath!);

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsAsync()
    {
        SaveFileDialog dialog = new()
        {
            Filter = "UEFI Resource Files|*.bin|All Files|*.*",
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
            MessageBox.Show("File saved successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Save failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

            if (result == MessageBoxResult.Yes)
            {
                await SaveAsync();
            }
            else if (result == MessageBoxResult.Cancel)
            {
                return;
            }
        }

        if (window is Window w)
        {
            w.Close();
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

    partial void OnSelectedResourceChanged(ResourceItemViewModel? oldValue, ResourceItemViewModel? newValue)
    {
        if (oldValue is not null)
        {
            oldValue.PropertyChanged -= OnSelectedResourceItemChanged;
        }

        if (newValue is not null)
        {
            newValue.PropertyChanged += OnSelectedResourceItemChanged;
        }
    }

    private void OnSelectedResourceItemChanged(object sender, PropertyChangedEventArgs e)
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
            foreach (ResourceItemViewModel item in Resources)
            {
                item.IsModified = false;
            }
        }
    }
}
