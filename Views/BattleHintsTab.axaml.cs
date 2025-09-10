using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using RainbusTools.Models.Managers;
using RainbusTools.ViewModels;

namespace RainbusTools.Views;

public partial class BattleHintsTab : UserControl
{
    public BattleHintsTab()
    {
        InitializeComponent();

        // Resolve RepositoryManager via DI
        var repoManager = ((App)Application.Current).ServiceProvider.GetRequiredService<RepositoryManager>();
        var vm = new BattleHintsTabViewModel(repoManager);
        DataContext = vm;

        vm.HintsUpdated += () =>
        {
            BattleHintsScrollViewer.ScrollToEnd();
        };
    }
}
