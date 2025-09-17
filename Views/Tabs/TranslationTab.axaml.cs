using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using RainbusToolbox.ViewModels;

namespace RainbusToolbox.Views;

public partial class TranslationTab : UserControl
{
    public TranslationTab()
    {
        InitializeComponent();
        DataContext ??= new TranslationTabViewModel();
    }

    private async void OnSelectFileClick(object sender, RoutedEventArgs e)
    {
        var storageProvider = TopLevel.GetTopLevel(this)?.StorageProvider;
        if (storageProvider == null) return;

        var fileTypes = new[]
        {
            new FilePickerFileType("Translation Files")
            {
                Patterns = new[] { "*.json" },
                AppleUniformTypeIdentifiers = new[] { "public.json" },
                MimeTypes = new[] { "application/json" }
            },
            FilePickerFileTypes.All
        };

        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Translation File",
            AllowMultiple = false,
            FileTypeFilter = fileTypes
        });

        if (files.Count > 0 && DataContext is TranslationTabViewModel vm)
        {
            var filePath = files[0].Path.LocalPath;
            vm.LoadFileCommand.Execute(filePath);
        }
    }
}