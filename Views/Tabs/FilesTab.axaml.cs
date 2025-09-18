using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.ViewModels;

namespace RainbusToolbox.Views;

public partial class FilesTab : UserControl
{
    private FilesTabViewModel _viewModel;
    public FilesTab()
    {
        InitializeComponent();
        DataContext = _viewModel = new FilesTabViewModel();
        // Pull the RepositoryManager out of DI
    }

}