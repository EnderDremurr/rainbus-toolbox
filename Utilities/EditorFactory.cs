using Microsoft.Extensions.DependencyInjection;

namespace RainbusToolbox.Utilities;

public class EditorFactory
{
    public IFileEditor CreateFileEditor(Type editorType)
    {
        return App.Current.ServiceProvider.GetKeyedService<IFileEditor>(editorType) ??
               App.Current.ServiceProvider.GetRequiredService<IFileEditor>();
    }
}