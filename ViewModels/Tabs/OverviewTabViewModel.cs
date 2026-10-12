using CommunityToolkit.Mvvm.ComponentModel;

namespace RainbusToolbox.ViewModels.Tabs;

public class OverviewTabViewModel : ObservableObject, IMainWindowTab
{
    public Task OnTabOpened()
    {
        return Task.CompletedTask;
    }
}