using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.ViewModels;

namespace RainbusToolbox.Views;

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
