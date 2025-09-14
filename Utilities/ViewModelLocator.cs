using System;
using Microsoft.Extensions.DependencyInjection;
using RainbusToolbox.ViewModels;

public class ViewModelLocator
{
    private readonly IServiceProvider _serviceProvider;
    
    public ViewModelLocator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public ReleaseTabViewModel ReleaseTabViewModel => 
        _serviceProvider.GetRequiredService<ReleaseTabViewModel>();
    
    // Add other ViewModels as needed
    public MainWindowViewModel MainWindowViewModel => 
        _serviceProvider.GetRequiredService<MainWindowViewModel>();
}