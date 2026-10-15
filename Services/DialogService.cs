using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using RainbusToolbox.ViewModels.Misc;
using RainbusToolbox.Views.Misc;

namespace RainbusToolbox.Services;

public class DialogService
{
    private static Window GetOwner()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            throw new Exception("Couldn't get any owner window!");

        return (desktop.Windows.FirstOrDefault(w => w.IsActive)
                ?? desktop.MainWindow) ?? throw new InvalidOperationException();
    }

    public async Task<string?> PickSingleFolderAsync(FolderPickerOpenOptions options)
    {
        var owner = GetOwner();

        var folders = await owner.StorageProvider.OpenFolderPickerAsync(options);
        return folders.ToList().FirstOrDefault()?.TryGetLocalPath();
    }

    public async Task<List<string>?> PickMultipleFoldersAsync(FolderPickerOpenOptions options)
    {
        var owner = GetOwner();

        var folders = await owner.StorageProvider.OpenFolderPickerAsync(options);
        var nonNullFolderPaths = folders.Select(f => f.TryGetLocalPath()).OfType<string>().ToList();
        return nonNullFolderPaths.Count != 0 ? nonNullFolderPaths : null;
    }

    public async Task<string?> PickSingleFileAsync(FilePickerOpenOptions options)
    {
        var owner = GetOwner();

        var files = await owner.StorageProvider.OpenFilePickerAsync(options);
        return files.ToList().FirstOrDefault()?.TryGetLocalPath();
    }

    public async Task<List<string>?> PickMultipleFilesAsync(FilePickerOpenOptions options)
    {
        var owner = GetOwner();

        var files = await owner.StorageProvider.OpenFilePickerAsync(options);
        var nonNullFilePaths = files.Select(f => f.TryGetLocalPath()).OfType<string>().ToList();
        return nonNullFilePaths.Count != 0 ? nonNullFilePaths : null;
    }

    public async Task<PopUpWindowViewModel?> ShowPopupAsync(string title,
        string message,
        bool hasInput = false,
        string inputPlaceholder = "",
        string? initialValue = null,
        params PopupButton[] buttons)
    {
        var owner = GetOwner();

        return await PopUpWindow.ShowAsync(owner, title, message, hasInput, inputPlaceholder, initialValue, buttons);
    }
}