using Avalonia.Controls;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.RepositoryServices;
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
                (LocalizationManager)App.Current.ServiceProvider.GetService(typeof(LocalizationManager)),
                (MassReplacementService)App.Current.ServiceProvider.GetService(typeof(MassReplacementService)));
    }
}