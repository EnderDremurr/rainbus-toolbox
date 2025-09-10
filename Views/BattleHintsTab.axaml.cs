using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using RainbusTools.Converters.Managers;
using RainbusTools.ViewModels;

namespace RainbusTools.Views;

public partial class BattleHintsTab : UserControl
{
    public BattleHintsTab()
    {
        InitializeComponent();
        var vm = new BattleHintsTabViewModel(App.RepositoryManager);
        DataContext = vm;
        
        vm.HintsUpdated += () =>
        {
            // Scroll to bottom
            BattleHintsScrollViewer.ScrollToEnd();
        };
    }
}