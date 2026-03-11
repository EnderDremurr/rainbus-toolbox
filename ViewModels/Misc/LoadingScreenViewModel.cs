using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RainbusToolbox.ViewModels;

public partial class LoadingScreenViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _progressCompleted;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDeterministic))]
    private float _progressPercent;

    [ObservableProperty]
    private int _progressTotal;

    [ObservableProperty]
    private string _text = "";

    private LoadingScreenViewModel()
    {
    }

    public static LoadingScreenViewModel Instance { get; } = new();


    public bool IsDeterministic => ProgressPercent > 0f;

    public static void StartLoading(string startingText = "Загрузка...", int startingCompleted = 0,
        int startingTotal = 0)
    {
        Dispatcher.UIThread.Post(() =>
            {
                Instance.Text = startingText;
                Instance.IsLoading = true;
                if (startingTotal == 0 || startingCompleted == 0)
                {
                    Instance.ProgressPercent = 0f;
                }
                else
                {
                    Instance.ProgressPercent = (float)startingCompleted / startingTotal;
                    Instance.ProgressTotal = startingTotal;
                    Instance.ProgressCompleted = startingCompleted;
                }
            }
        );
    }

    public static void FinishLoading()
    {
        Dispatcher.UIThread.Post(() => { Instance.IsLoading = false; }
        );
    }


    public static void SetText(string text)
    {
        Dispatcher.UIThread.Post(() => { Instance.Text = text; }
        );
    }

    public static void SetProgress(int completed = 0, int total = 0)
    {
        Dispatcher.UIThread.Post(() =>
            {
                if (total == 0)
                {
                    Instance.ProgressPercent = 0f;
                    return;
                }

                Instance.ProgressPercent = (float)completed / total;
                Instance.ProgressTotal = total;
                Instance.ProgressCompleted = completed;
            }
        );
    }
}