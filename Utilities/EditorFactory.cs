using Microsoft.Extensions.DependencyInjection;

namespace RainbusToolbox.Services.ExternalServices;

public class EditorFactory
{
    public IFileEditor CreateFileEditor(Type editorType)
    {
        return App.Current.ServiceProvider.GetKeyedService<IFileEditor>(editorType) ??
               App.Current.ServiceProvider.GetRequiredService<IFileEditor>();
    }
}