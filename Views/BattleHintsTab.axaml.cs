using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;
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
    private void RadioButton_Checked(object? sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb && DataContext is BattleHintsTabViewModel vm)
        {
            if (Enum.TryParse<BattleHintTypes>(rb.Tag.ToString(), out var type))
            {
                vm.SelectedType = type;
            }
        }
    }

}
