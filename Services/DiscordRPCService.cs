using System.Threading;
using DiscordRPC;

namespace RainbusToolbox.Services;

public class DiscordRPCService
{
    private DiscordRpcClient _client;
    
    public DiscordRPCService()
    {
        _client = new DiscordRpcClient("1477335960785260676");
        _client.Initialize();
    }
    
    private string _projectName = "Unknown";
    public string ProjectName
    {
        get => _projectName;
        set
        {
            _projectName = value;
            SetState(null);
        }
    }
    
    private CancellationTokenSource? _rpcDebounce;

    public async void SetState(string? state)
    {
        _rpcDebounce?.Cancel();
        _rpcDebounce = new CancellationTokenSource();
        
        if(ProjectName == "Unknown")
            return;
        
        try
        {
            await Task.Delay(500, _rpcDebounce.Token);
            
            _client.SetPresence(new RichPresence()
            {
                Details = ProjectName,
                State = string.IsNullOrEmpty(state) 
                    ? _client.CurrentPresence?.State ?? "Готовится делать перевоз"
                    : state,
                Assets = new Assets()
                {
                    LargeImageKey = "icon",
                    LargeImageText = "Кто прочитал тот лох",
                }
            });
        }
        catch (TaskCanceledException) { }
    }
    
    ~DiscordRPCService()
    {
        _client.Dispose();
    }
}