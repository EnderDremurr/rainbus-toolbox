using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace RainbusToolbox.Services;

public class ClipboardService
{
    private static Window GetOwner()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            throw new Exception("Couldn't get any owner window!");

        return (desktop.Windows.FirstOrDefault(w => w.IsActive)
                ?? desktop.MainWindow) ?? throw new InvalidOperationException();
    }

    public async Task SetClipboardTextAsync(string text)
    {
        var owner = GetOwner();
        var clipboard = owner.Clipboard;
        if(clipboard is null)
        {
            _ = App.Current.HandleNonFatalExceptionAsync(new Exception("Couldn't access clipboard!"));
            return;
        }
        await clipboard.SetTextAsync(text);
    }
}