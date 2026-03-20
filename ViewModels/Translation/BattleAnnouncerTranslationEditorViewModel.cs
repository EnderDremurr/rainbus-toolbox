using CommunityToolkit.Mvvm.ComponentModel;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;
using Serilog;
using Path = System.IO.Path;

namespace RainbusToolbox.ViewModels;

public partial class BattleAnnouncerTranslationEditorViewModel
    : TranslationEditorViewModel<AnnouncerVoiceLocalizationFile, AnnouncerVoice>
{
    [ObservableProperty]
    private string _localizedAnnouncerName = "";

    [ObservableProperty]
    private string _phraseType = "";

    [ObservableProperty]
    private string _referenceAnnouncerName = "";

    private RepositoryManager _repositoryManager =>
        (RepositoryManager)App.Current.ServiceProvider.GetService(typeof(RepositoryManager));

    private AnnouncerLocalizationFile _announcerLocalizationFile => _repositoryManager.AnnouncerNames;
    private AnnouncerLocalizationFile _referenceAnnouncerLocalizationFile => _repositoryManager.AnnouncerNamesReference;

    private AnnouncerVoiceTypeLocalizationFile _announcerVoiceTypeLocalizationFile =>
        _repositoryManager.AnnouncerVoiceTypes;


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

    protected override void UpdateCurrentItem()
    {
        if (EditableFile != null && EditableFile.DataList.Count > 0)
            CurrentItem = EditableFile.DataList[CurrentIndex];

        var currentAnnouncerVoiceTypeId = CurrentItem!.Id;

        var parts = currentAnnouncerVoiceTypeId.Split('_');
        var middle = string.Join("", parts[1..^2]).ToLower()
            .Replace("advatk", "adv")
            .Replace("disadvatk", "disadv")
            .Replace("specialbuff", "buff")
            .Replace("specialdebuff", "debuff")
            .Replace("takebigdmg", "bigdamage")
            .Replace("givebigdmg", "bigdamage")
            .Replace("round", "")
            .Replace("advphysical", "physicaladv")
            .Replace("disadvphysical", "physicaldisadv")
            .Replace("advattr", "attradv")
            .Replace("disadvattr", "attrdisadv");

        if (middle.Contains("special"))
        {
            PhraseType = "Особая реплика";
            return;
        }

        PhraseType = _announcerVoiceTypeLocalizationFile.DataList
            .FirstOrDefault(a =>
            {
                var normalized = a.Id.ToLower().Replace("_", "");
                return normalized.Contains(middle) || middle.Contains(normalized);
            })
            ?.Content ?? "Неизвестно";

        if (PhraseType == "Неизвестно")
            Log.Debug(
                "No match for: {CurrentAnnouncerVoiceTypeId} (middle: {Middle})", currentAnnouncerVoiceTypeId, middle);
    }
}