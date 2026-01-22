using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RainbusToolbox.Utilities.Data;
using System.IO;
using RainbusToolbox.Models.Managers;

namespace RainbusToolbox.Models.Data;

public static class FileToObjectCaster
{
    public static readonly Dictionary<string, Type> Map = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
    {
        { "ui", typeof(UiLocalizationFile) },
        { "character", typeof(CharacterLocalizationFile) },
        { "personality", typeof(PersonalityLocalizationFile) },
        { "enemy", typeof(EnemyLocalizationFile) },
        { "ego", typeof(EgoLocalizationFile) },
        { "skill", typeof(SkillLocalizationFile) },
        { "passive", typeof(PassiveLocalizationFile) },
        { "buf", typeof(BufLocalizationFile) },
        { "buffAbilities", typeof(BuffAbilitiesLocalizationFile) },
        { "item", typeof(ItemLocalizationFile) },
        { "keyword", typeof(KeywordLocalizationFile) },
        { "skillTag", typeof(SkillTagLocalizationFile) },
        { "abnormalityEvents", typeof(AbnormalityEventsLocalizationFile) },
        { "abnormalityCharDlgs", typeof(AbnormalityCharDlgsLocalizationFile) },
        { "attributeText", typeof(AttributeTextLocalizationFile) },
        { "abnormalityGuideContent", typeof(AbnormalityGuideContentLocalizationFile) },
        { "keywordDictionary", typeof(KeywordDictionaryLocalizationFile) },
        { "actionEvents", typeof(ActionEventsLocalizationFile) },
        { "egoGifts", typeof(EgoGiftsLocalizationFile) },
        { "stageChapter", typeof(StageChapterLocalizationFile) },
        { "stagePart", typeof(StagePartLocalizationFile) },
        { "stageNodeInfo", typeof(StageNodeInfoLocalizationFile) },
        { "dungeonNodeInfo", typeof(DungeonNodeInfoLocalizationFile) },
        { "storyDungeonNodeInfo", typeof(StoryDungeonNodeInfoLocalizationFile) },
        { "railwayDungeonNodeInfo", typeof(RailwayDungeonNodeInfoLocalizationFile) },
        { "railwayDungeon", typeof(RailwayDungeonLocalizationFile) },
        { "dungeonArea", typeof(DungeonAreaLocalizationFile) },
        { "quest", typeof(QuestLocalizationFile) },
        { "storyTheater", typeof(StoryTheaterLocalizationFile) },
        { "announcer", typeof(AnnouncerLocalizationFile) },
        { "normalBattleHint", typeof(NormalBattleHintLocalizationFile) },
        { "abBattleHint", typeof(AbBattleHintLocalizationFile) },
        { "battleResultHint", typeof(BattleResultHintLocalizationFile) },
        { "tutorialDesc", typeof(TutorialDescLocalizationFile) },
        { "personalityVoice", typeof(PersonalityVoiceLocalizationFile) },
        { "announcerVoice", typeof(AnnouncerVoiceLocalizationFile) },
        { "egoVoice", typeof(EgoVoiceLocalizationFile) },
        { "battleSpeechBubble", typeof(BattleSpeechBubbleLocalizationFile) },
        { "iapProduct", typeof(IapProductLocalizationFile) },
        { "iapSticker", typeof(IapStickerLocalizationFile) },
        { "getConditionText", typeof(GetConditionTextLocalizationFile) },
        { "choiceEventResult", typeof(ChoiceEventResultLocalizationFile) },
        { "battlePassMission", typeof(BattlePassMissionLocalizationFile) },
        { "gachaTitle", typeof(GachaTitleLocalizationFile) },
        { "gachaNotice", typeof(GachaNoticeLocalizationFile) },
        { "introduceCharacter", typeof(IntroduceCharacterLocalizationFile) },
        { "userBanner", typeof(UserBannerLocalizationFile) },
        { "userTicketL", typeof(UserTicketLLocalizationFile) },
        { "userTicketR", typeof(UserTicketRLocalizationFile) },
        { "userTicketEGOBg", typeof(UserTicketEGOBgLocalizationFile) },
        { "bgmLyrics", typeof(BgmLyricsLocalizationFile) },
        { "threadDungeon", typeof(ThreadDungeonLocalizationFile) },
        { "railwayDungeonStationName", typeof(RailwayDungeonStationNameLocalizationFile) },
        { "railwayDungeonBuff", typeof(RailwayDungeonBuffLocalizationFile) },
        { "dungeonName", typeof(DungeonNameLocalizationFile) },
        { "mentalCondition", typeof(MentalConditionLocalizationFile) },
        { "danteNote", typeof(DanteNoteLocalizationFile) },
        { "danteNoteCategoryKeyword", typeof(DanteNoteCategoryKeywordLocalizationFile) },
        { "panicInfo", typeof(PanicInfoLocalizationFile) },
        { "dungeonStartBuffs", typeof(DungeonStartBuffsLocalizationFile) },
        { "egoGiftCategory", typeof(EgoGiftCategoryLocalizationFile) },
        { "mirrorDungeonEgoGiftLockedDesc", typeof(MirrorDungeonEgoGiftLockedDescLocalizationFile) },
        { "danteAbility", typeof(DanteAbilityLocalizationFile) },
        { "mirrorDungeonTheme", typeof(MirrorDungeonThemeLocalizationFile) },
        { "unlockCode", typeof(UnlockCodeLocalizationFile) },
        { "scenarioModelCodes", typeof(ScenarioModelCodesLocalizationFile) },
        { "story", typeof(StoryLocalizationFile) },
        { "announcerVoiceType", typeof(AnnouncerVoiceTypeLocalizationFile) },
        { "mirrorDungeonRentalName", typeof(MirrorDungeonRentalNameLocalizationFile) },
        { "projectGSLessonName", typeof(ProjectGSLessonNameLocalizationFile) },
        { "projectGSComboName", typeof(ProjectGSComboNameLocalizationFile) },
    };
    
    public static Type? GetType(string pathToFile, RepositoryManager repositoryManager)
    {
        Console.WriteLine($"Recevied a cast request for file <{pathToFile}>");
        var fileName = Path.GetFileNameWithoutExtension(pathToFile);
        var isKnownFile = repositoryManager.DeveloperFileTypeMap.TryGetValue(fileName, out var knownFileType);
        Console.WriteLine($"Supposed file type is <{knownFileType}>");
        if (!isKnownFile || knownFileType is null)
        {
            if(IsStoryFile(pathToFile))
                return typeof(StoryDataFile);
            if (fileName == "BattleHint")
                return typeof(NormalBattleHintLocalizationFile);
            
            return null;
        };
        var isKnown = Map.TryGetValue(knownFileType, out var type);
        return isKnown ? type : null;
    }

    private static bool IsStoryFile(string pathToFile)
    {
        var directoryPath = Path.GetDirectoryName(pathToFile);
        if (string.IsNullOrEmpty(directoryPath))
            return false;
    
        var pathParts = directoryPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return pathParts.Any(part => part.Equals("StoryData", StringComparison.OrdinalIgnoreCase));
    }
}