using System.Threading;
using DiscordRPC;

namespace RainbusToolbox.Services.ExternalServices;

public sealed class DiscordRPCService : IDisposable
{
    public const int MaxAssets = 37; // type how much images are currently added to the rpc, absolute max is 300
    private readonly DiscordRpcClient _client;

    private readonly PersistentDataManager _persistentDataManager;

    private CancellationTokenSource? _rpcDebounce;

    private int _selectedAsset; // assets are numerated from 1
    public string ProjectName = "Unknown";

    public string ProjectUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";

    public DiscordRPCService(PersistentDataManager persistentDataManager)
    {
        _persistentDataManager = persistentDataManager;
        _selectedAsset = _persistentDataManager.Settings.SelectedDiscordRPCAssetId;

        _client = new DiscordRpcClient("1477335960785260676");
        _client.Initialize();

        _persistentDataManager.OnRPCAssetIdChanged += () =>
            SetActiveAssetKey(_persistentDataManager.Settings.SelectedDiscordRPCAssetId);
    }

    public void Dispose()
    {
        _rpcDebounce?.Cancel();
        _rpcDebounce?.Dispose();
        _client.Dispose();
    }

    public void SetActiveAssetKey(int assetId)
    {
        Log.Debug("Got an event call to change Discord RPC large asset");
        _selectedAsset = Math.Clamp(assetId, 0, MaxAssets);
        _client.UpdateLargeAsset($"{_selectedAsset}");
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
                    LargeImageKey = $"{_selectedAsset}",
                    LargeImageUrl = "https://github.com/EnderDremurr/rainbus-toolbox",
                    LargeImageText = "Rainbus Toolbox"
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