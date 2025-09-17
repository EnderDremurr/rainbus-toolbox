using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Services;

namespace RainbusToolbox.ViewModels;

public partial class FilesTabViewModel : ObservableObject
{
    private RepositoryManager _repositoryManager;
    private KeyWordConversionService _keyWordConversionService;

    public FilesTabViewModel()
    {
        _repositoryManager = (App.Current.ServiceProvider.GetService(typeof(RepositoryManager)) as RepositoryManager)!;
        _keyWordConversionService =(App.Current.ServiceProvider.GetService(typeof(KeyWordConversionService)) as KeyWordConversionService)!;
    }
    [RelayCommand]
    private void ReplaceAllTagsWithMeshes()
    {
        _keyWordConversionService.ReplaceEveryTagWithMesh(_repositoryManager.PathToLocalization);
    }
    public void OnParseButtonClick(object? sender, RoutedEventArgs e)
    {
        _repositoryManager.ParseNewAdditionsFromGame();
    }
}