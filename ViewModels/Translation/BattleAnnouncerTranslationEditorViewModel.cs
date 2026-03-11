using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;
using Path = System.IO.Path;

namespace RainbusToolbox.ViewModels;

public partial class BattleAnnouncerTranslationEditorViewModel
    : TranslationEditorViewModel<AnnouncerVoiceLocalizationFile, AnnouncerVoice>
{
    // Original image size (AnnouncerBG.png)
    private const double BaseWidth = 673.0;
    private const double BaseHeight = 246.0;

    // Original margins at base resolution: left=255, top=30, right=0, bottom=45
    private static readonly Thickness BaseDialogueMargin = new(400, 30, 0, 45);

    private double _currentImageHeight = BaseHeight;

    // image size reported by view (defaults to base so layout starts sane)
    private double _currentImageWidth = BaseWidth;

    [ObservableProperty]
    private string _localizedAnnouncerName = "";

    [ObservableProperty]
    private string _referenceAnnouncerName = "";

    private RepositoryManager _repositoryManager =>
        (RepositoryManager)App.Current.ServiceProvider.GetService(typeof(RepositoryManager));

    private AnnouncerLocalizationFile _announcerLocalizationFile => _repositoryManager.AnnouncerNames;
    private AnnouncerLocalizationFile _referenceAnnouncerLocalizationFile => _repositoryManager.AnnouncerNamesReference;

    /// <summary>
    ///     Margin for the reference text box (scaled).
    /// </summary>
    public Thickness ReferenceMargins => CalculateRelativeMargins();

    /// <summary>
    ///     Margin for the editor text box (scaled).
    /// </summary>
    public Thickness EditorMargins => CalculateRelativeMargins();

    public double CurrentImageWidth
    {
        get => _currentImageWidth;
        set
        {
            if (Math.Abs(_currentImageWidth - value) > double.Epsilon)
            {
                _currentImageWidth = value;
                OnPropertyChanged(nameof(ReferenceMargins));
                OnPropertyChanged(nameof(EditorMargins));
            }
        }
    }

    public double CurrentImageHeight
    {
        get => _currentImageHeight;
        set
        {
            if (Math.Abs(_currentImageHeight - value) > double.Epsilon)
            {
                _currentImageHeight = value;
                OnPropertyChanged(nameof(ReferenceMargins));
                OnPropertyChanged(nameof(EditorMargins));
            }
        }
    }

    public override void LoadEditableFile(AnnouncerVoiceLocalizationFile file)
    {
        EditableFile = file;
        CurrentIndex = 0;

        var announcerId = Path.GetFileNameWithoutExtension(file.FullPath).Split('_')[^1];

        LocalizedAnnouncerName =
            _announcerLocalizationFile.DataList.FirstOrDefault(a => a.Id == announcerId)?.Name ??
            "Сюжетный чел";
        ReferenceAnnouncerName =
            _referenceAnnouncerLocalizationFile.DataList.FirstOrDefault(a => a.Id == announcerId)?.Name ?? "Story guy";

        UpdateCurrentItem();
        UpdateReferenceItem();
        UpdateNavigation();
        OnPropertyChanged(nameof(IsFileLoaded));
    }

    private Thickness CalculateRelativeMargins()
    {
        var scaleX = CurrentImageWidth / BaseWidth;
        var scaleY = CurrentImageHeight / BaseHeight;

        // Keep the margins scaled but don't allow negative or absurd values.
        var left = Math.Max(0, BaseDialogueMargin.Left * scaleX);
        var top = Math.Max(0, BaseDialogueMargin.Top * scaleY);
        var right = Math.Max(0, BaseDialogueMargin.Right * scaleX);
        var bottom = Math.Max(0, BaseDialogueMargin.Bottom * scaleY);

        return new Thickness(left, top, right, bottom);
    }
}