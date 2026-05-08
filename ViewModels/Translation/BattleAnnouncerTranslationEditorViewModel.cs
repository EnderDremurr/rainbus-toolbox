using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;
using Path = System.IO.Path;

namespace RainbusToolbox.ViewModels;

public partial class BattleAnnouncerTranslationEditorViewModel
    : TranslationEditorViewModel<AnnouncerVoiceLocalizationFile, AnnouncerVoice>
{
    private readonly Dictionary<string, string> _announcerVoiceTypeLookupMap = new()
    {
        { "announcer_danger", "Danger" },
        { "announcer_enemy_specialskill", "SpecialSkill" },
        { "announcer_enemy_specialgimmick", "SpecialGimmick" },

        { "announcer_ally_specialdebuff", "AllyDebuff" },
        { "announcer_ally_specialbuff", "AllyBuff" },

        { "announcer_specialcheer", "SpecialCheer" },
        { "announcer_cheer", "Cheer" },

        { "announcer_enemy_adv", "EnemyAdv" },
        { "announcer_ally_adv", "AllyAdv" },
        { "announcer_ally_advex", "AllyAdvEx" },

        { "announcer_enemy_break", "EnemyBreak" },
        { "announcer_ally_break", "AllyBreak" },

        { "announcer_ally_dead", "AllyDead" },
        { "announcer_killenemy", "KillEnemy" },
        { "announcer_multikillenemy", "MultiKillEnemy" },
        { "announcer_multikillally", "MultiKillAlly" },

        { "announcer_enemy_destroy", "EnemyDestroy_Battle" },

        { "announcer_round_takebigdmg", "AllyBigDamage_Round" },
        { "announcer_round_givebigdmg", "EnemyBigDamage_Round" },

        { "announcer_advatk_physical", "PhysicalAdv" },
        { "announcer_advatk_attr", "AttrAdv" },
        { "announcer_disadvatk_physical", "PhysicalDisadv" },
        { "announcer_disadvatk_attr", "AttrDisadv" },

        { "announcer_enemy_specialbuff", "EnemyBuff" },
        { "announcer_enemy_specialdebuff", "EnemyDebuff" },

        { "announcer_equip", "Equip" },

        { "announcer_neglect", "Neglect" },
        { "announcer_wait", "Neglect" },

        { "announcer_battle_win", "Win" },
        { "announcer_battle_defeat", "Defeat" }
    };

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

        var typeFromMap = _announcerVoiceTypeLookupMap
            .OrderByDescending(t => t.Key.Length)
            .FirstOrDefault(t =>
                currentAnnouncerVoiceTypeId.StartsWith(t.Key + "_", StringComparison.Ordinal))
            .Value;

        PhraseType = typeFromMap != null
            ? _announcerVoiceTypeLocalizationFile.DataList
                .FirstOrDefault(a => a.Id == typeFromMap)
                ?.Content ?? "Специальная реплика"
            : "Специальная реплика";

        if (PhraseType == "Специальная реплика")
            Log.Debug(
                "No match for: {CurrentAnnouncerVoiceTypeId} (type: {Middle})", currentAnnouncerVoiceTypeId,
                typeFromMap);
    }
}