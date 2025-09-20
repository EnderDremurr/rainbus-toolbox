using System;
using Microsoft.Extensions.DependencyInjection;
using RainbusToolbox.ViewModels;

public class ViewModelLocator(IServiceProvider serviceProvider)
{
    public ReleaseTabViewModel ReleaseTabViewModel => 
        serviceProvider.GetRequiredService<ReleaseTabViewModel>();
    
    public MainWindowViewModel MainWindowViewModel => 
        serviceProvider.GetRequiredService<MainWindowViewModel>();
}