using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RainbusToolbox.ViewModels;

public partial class PopUpWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _hasInput;

    [ObservableProperty]
    private string _inputPlaceholder;

    [ObservableProperty]
    private string _inputValue;

    [ObservableProperty]
    private string _message;

    [ObservableProperty]
    private string _title;

    public string? Result { get; set; }
    public ObservableCollection<PopupButton> Buttons { get; set; } = new();
}

public class PopupButton
{
    public string Label { get; set; }
    public string ResultValue { get; set; }
    public bool KeepOpen { get; set; }
    public Action? OnClick { get; set; }
    public ICommand? ClickCommand { get; set; }
}