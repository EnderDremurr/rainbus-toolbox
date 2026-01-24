using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using RainbusToolbox.Models.Data;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.ViewModels;

public partial class StoryTranslationEditorViewModel : TranslationEditorViewModel<StoryDataFile, StoryDataItem>
{
    private readonly RepositoryManager _repositoryManager = (App.Current.ServiceProvider.GetService(typeof(RepositoryManager)) as RepositoryManager)!;

    [ObservableProperty] private ScenarioModelCode? _scenarioModel;
    [ObservableProperty] private ScenarioModelCode? _scenarioModelReference;

    public string DisplayTeller
    {
        get => CurrentItem?.Teller ?? ScenarioModel?.Name ?? string.Empty;
        set
        {
            if (CurrentItem != null && CurrentItem.Teller != null)
            {
                CurrentItem.Teller = value;
                OnPropertyChanged();
            }
        }
    }

    public string DisplayTitle
    {
        get => CurrentItem?.Title ?? ScenarioModel?.NickName ?? string.Empty;
        set
        {
            if (CurrentItem != null && CurrentItem.Title != null)
            {
                CurrentItem.Title = value;
                OnPropertyChanged();
            }
        }
    }

    
    public string DisplayTellerReference => ReferenceItem?.Teller ?? ScenarioModelReference?.Name ?? string.Empty;
    public string DisplayTitleReference => ReferenceItem?.Title ?? ScenarioModelReference?.NickName ?? string.Empty;

    public string? EditableTeller
    {
        get => CurrentItem?.Teller;
        set
        {
            if (CurrentItem != null && CurrentItem.Teller != null)
            {
                CurrentItem.Teller = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayTeller));
            }
        }
    }

    public string? EditableTitle
    {
        get => CurrentItem?.Title;
        set
        {
            if (CurrentItem != null && CurrentItem.Title != null)
            {
                CurrentItem.Title = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayTitle));
            }
        }
    }

    public bool CanEditTeller => CurrentItem?.Teller is not null;
    public bool CanEditTitle => CurrentItem?.Title is not null;

    protected override void UpdateCurrentItem()
    {
        base.UpdateCurrentItem();
        
        if (CurrentItem?.Model != null)
        {
            ScenarioModel = _repositoryManager.ScenarioModelCodes?.DataList
                .FirstOrDefault(x => x.Id == CurrentItem.Model);
        }
        else
        {
            ScenarioModel = null;
        }
        
        OnPropertyChanged(nameof(DisplayTeller));
        OnPropertyChanged(nameof(DisplayTitle));
        OnPropertyChanged(nameof(EditableTeller));
        OnPropertyChanged(nameof(EditableTitle));
        OnPropertyChanged(nameof(CanEditTeller));
        OnPropertyChanged(nameof(CanEditTitle));
    }

    protected override void UpdateReferenceItem()
    {
        base.UpdateReferenceItem();
        
        if (ReferenceItem?.Model != null)
        {
            ScenarioModelReference = _repositoryManager.ScenarioModelCodesReference?.DataList
                .FirstOrDefault(x => x.Id == ReferenceItem.Model);
        }
        else
        {
            ScenarioModelReference = null;
        }
        
        OnPropertyChanged(nameof(DisplayTellerReference));
        OnPropertyChanged(nameof(DisplayTitleReference));
    }
}