using System.Threading;
using DiscordRPC;

namespace RainbusToolbox.Services;

public class DiscordRPCService : IDisposable
{
    private readonly DiscordRpcClient _client;

    private CancellationTokenSource? _rpcDebounce;
    public string ProjectName = "Unknown";

    public string ProjectUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";

    public DiscordRPCService()
    {
        _client = new DiscordRpcClient("1477335960785260676");
        _client.Initialize();
    }

    public void Dispose()
    {
        _rpcDebounce?.Cancel();
        _rpcDebounce?.Dispose();
        _client.Dispose();
    }

    public async void SetState(string? state)
    {
        _rpcDebounce?.Cancel();
        _rpcDebounce = new CancellationTokenSource();

        try
        {
            await Task.Delay(500, _rpcDebounce.Token);

            _client.SetPresence(new RichPresence
            {
                Details = ProjectName,
                State = string.IsNullOrWhiteSpace(state)
                    ? _client.CurrentPresence?.State ?? "Готовится делать перевоз"
                    : state,
                Assets = new Assets
                {
                    LargeImageKey = "icon",
                    LargeImageUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                    LargeImageText = "Нажми сюда"
                },
                Buttons =
                [
                    new Button { Label = "Открыть перевод", Url = ProjectUrl }
                ]
            });
        }
        catch (TaskCanceledException)
        {
        }
    }
}