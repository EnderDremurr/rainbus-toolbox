using Avalonia.Controls;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Services.RepositoryServices;
using RainbusToolbox.ViewModels;

namespace RainbusToolbox.Views.Misc;

public partial class RegexEditor : UserControl
{
    private RegexEditorViewModel _viewModel;

    public RegexEditor()
    {
        InitializeComponent();
        DataContext = _viewModel =
            new RegexEditorViewModel(
                (RepositoryManager)App.Current.ServiceProvider.GetService(typeof(RepositoryManager)),
                (MassReplacementService)App.Current.ServiceProvider.GetService(typeof(MassReplacementService)));
    }
}