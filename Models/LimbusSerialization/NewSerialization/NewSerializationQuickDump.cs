using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class UiLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdContent>
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; }
}

public class CharacterLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdName>
{
    [JsonProperty("dataList")]
    public List<GenericIdName> DataList { get; set; }
}

public class PersonalityLocalizationFile : LocalizationFileBase, ILocalizationContainer<PersonalityDataEntry>
{
    [JsonProperty("dataList")]
    public List<PersonalityDataEntry> DataList { get; set; }
}
public class PersonalityDataEntry
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("title")]
    public string? Title { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("nameWithTitle")]
    public string? NameWithTitle { get; set; }

    [JsonProperty("desc")]
    public string? Description { get; set; }

}

public class EnemyLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdNameDesc>
{
    [JsonProperty("dataList")]
    public List<GenericIdNameDesc> DataList { get; set; }
}

public class EgoLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdNameDesc>
{
    [JsonProperty("dataList")]
    public List<GenericIdNameDesc> DataList { get; set; }
}

public class SkillLocalizationFile : LocalizationFileBase, ILocalizationContainer<Skill>
{
    [JsonProperty("dataList")]
    public List<Skill> DataList { get; set; }
}

public class Skill
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("levelList")]
    public List<SkillLevel> LevelList { get; set; }
}

public class SkillLevel
{
    [JsonProperty("abName")]
    public string? AbnormalityName { get; set; }
    
    [JsonProperty("level")]
    public string? Level { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("desc")]
    public string? Desc { get; set; }

    [JsonProperty("coinlist")]
    public List<CoinListItem> CoinList { get; set; } = new List<CoinListItem>();
}

public class CoinListItem
{
    [JsonProperty("coindescs")]
    public List<CoinDesc> CoinDescs { get; set; } = new List<CoinDesc>();
}

public class CoinDesc
{
    [JsonProperty("desc")]
    public string? Desc { get; set; }
}

public class PassiveLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdNameDesc>
{
    [JsonProperty("dataList")]
    public List<GenericIdNameDesc> DataList { get; set; }
}

public class BufLocalizationFile : LocalizationFileBase, ILocalizationContainer<BuffKeyword>
{
    [JsonProperty("dataList")]
    public List<BuffKeyword> DataList { get; set; }
}

public class BuffKeyword
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("desc")]
    public string? Desc { get; set; }

    [JsonProperty("summary")]
    public string? Summary { get; set; }

    [JsonProperty("undefined")]
    public string? Undefined { get; set; }
    
    [JsonProperty("flavor")]
    public string? Flavor { get; set; }
}

public class BuffAbilitiesLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdDesc>
{
    [JsonProperty("dataList")]
    public List<GenericIdDesc> DataList { get; set; }
}

public class ItemLocalizationFile : LocalizationFileBase, ILocalizationContainer<ItemDesc>
{
    [JsonProperty("dataList")]
    public List<ItemDesc> DataList { get; set; }
}

public class ItemDesc
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("desc")]
    public string? Desc { get; set; }
    
    [JsonProperty("flavor")]
    public string? Flavor { get; set; }
}

public class KeywordLocalizationFile : LocalizationFileBase, ILocalizationContainer<BuffKeyword>
{
    [JsonProperty("dataList")]
    public List<BuffKeyword> DataList { get; set; }
}

public class SkillTagLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdName>
{
    [JsonProperty("dataList")]
    public List<GenericIdName> DataList { get; set; }
}

public class AbnormalityEventsLocalizationFile : LocalizationFileBase, ILocalizationContainer<AbnormalityEventChoice>
{
    [JsonProperty("dataList")]
    public List<AbnormalityEventChoice> DataList { get; set; }
}

public class AbnormalityEventChoice
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("title")]
    public string? Title { get; set; }

    [JsonProperty("eventDesc")]
    public string? EventDesc { get; set; }

    [JsonProperty("prevDesc")]
    public string? PrevDesc { get; set; }

    [JsonProperty("behaveDesc")]
    public string? BehaveDesc { get; set; }

    [JsonProperty("successDesc")]
    public List<string>? SuccessDesc { get; set; }

    [JsonProperty("failureDesc")]
    public List<string>? FailureDesc { get; set; }
}


public class AbnormalityCharDlgsLocalizationFile : LocalizationFileBase, ILocalizationContainer<AbnormalityCharDlg>
{
    [JsonProperty("dataList")]
    public List<AbnormalityCharDlg> DataList { get; set; }
}
public class AbnormalityCharDlg
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("personalityid")]
    public string? PersonalityId { get; set; }

    [JsonProperty("voicefile")]
    public string? VoiceFile { get; set; }

    [JsonProperty("teller")]
    public string? Teller { get; set; }

    [JsonProperty("dialog")]
    public string? Dialog { get; set; }

    [JsonProperty("usage")]
    public string? Usage { get; set; }
}


public class AttributeTextLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdName>
{
    [JsonProperty("dataList")]
    public List<GenericIdName> DataList { get; set; }
}

public class AbnormalityGuideContentLocalizationFile : LocalizationFileBase, ILocalizationContainer<AbnormalityGuide>
{
    [JsonProperty("dataList")]
    public List<AbnormalityGuide> DataList { get; set; }
}
public class AbnormalityGuide
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("codeName")]
    public string? CodeName { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("clue")]
    public string? Clue { get; set; }

    [JsonProperty("storyList")]
    public List<AbnormalityStory?> StoryList { get; set; }
}

public class AbnormalityStory
{
    [JsonProperty("level")]
    public string? Level { get; set; }

    [JsonProperty("story")]
    public string? Story { get; set; } = string.Empty;
}

public class KeywordDictionaryLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdNameDesc>
{
    [JsonProperty("dataList")]
    public List<GenericIdNameDesc> DataList { get; set; }
}

public class ActionEventsLocalizationFile : LocalizationFileBase, ILocalizationContainer<ActionEvent>
{
    [JsonProperty("dataList")]
    public List<ActionEvent> DataList { get; set; }
}
public class ActionEvent
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("desc")]
    public string? Desc { get; set; }

    [JsonProperty("options")]
    public List<EventOption>? Options { get; set; }
}
public class EventOption
{
    [JsonProperty("message")]
    public string? Message { get; set; }

    [JsonProperty("messageDesc")]
    public string? MessageDesc { get; set; }

    [JsonProperty("result")]
    public List<string>? Result { get; set; }
}


public class EgoGiftsLocalizationFile : LocalizationFileBase, ILocalizationContainer<EgoGift>
{
    [JsonProperty("dataList")]
    public List<EgoGift> DataList { get; set; }
}
public class EgoGift
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("desc")]
    public string? Desc { get; set; }

    [JsonProperty("simpleDesc")]
    public List<EgoGiftSimpleDesc>? SimpleDesc { get; set; }
}
public class EgoGiftSimpleDesc
{
    [JsonProperty("abilityID")]
    public string? AbilityId { get; set; }

    [JsonProperty("simpleDesc")]
    public string? SimpleDesc { get; set; }
}


public class StageChapterLocalizationFile : LocalizationFileBase, ILocalizationContainer<StageChapter>
{
    [JsonProperty("dataList")]
    public List<StageChapter> DataList { get; set; }
}
public class StageChapter
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("company")]
    public string? Company { get; set; }

    [JsonProperty("area")]
    public string? Area { get; set; }

    [JsonProperty("chapter")]
    public string? Chapter { get; set; }

    [JsonProperty("chapterNumber")]
    public string? ChapterNumber { get; set; }

    [JsonProperty("chaptertitle")]
    public string? ChapterTitle { get; set; }

    [JsonProperty("timeline")]
    public string? Timeline { get; set; }
}


public class StagePartLocalizationFile : LocalizationFileBase, ILocalizationContainer<StagePart>
{
    [JsonProperty("dataList")]
    public List<StagePart> DataList { get; set; }
}

public class StagePart
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("parttitle")]
    public string? PartTitle { get; set; }
}

public class StageNodeInfoLocalizationFile : LocalizationFileBase, ILocalizationContainer<StageNode>
{
    [JsonProperty("dataList")]
    public List<StageNode> DataList { get; set; }
}

public class StageNode
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("title")]
    public string? Title { get; set; }

    [JsonProperty("place")]
    public string? Place { get; set; }

    [JsonProperty("desc")]
    public string? Desc { get; set; }
}


public class DungeonNodeInfoLocalizationFile : LocalizationFileBase, ILocalizationContainer<StageNode>
{
    [JsonProperty("dataList")]
    public List<StageNode> DataList { get; set; }
}

public class StoryDungeonNodeInfoLocalizationFile : LocalizationFileBase, ILocalizationContainer<StoryDungeonNodeList>
{
    [JsonProperty("dataList")]
    public List<StoryDungeonNodeList> DataList { get; set; }
}

public class StoryDungeonNodeList
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("stageList")]
    public List<StageNode>? StageList { get; set; }
}

public class RailwayDungeonNodeInfoLocalizationFile : LocalizationFileBase, ILocalizationContainer<StageNode>
{
    [JsonProperty("dataList")]
    public List<StageNode> DataList { get; set; }
}

public class RailwayDungeonLocalizationFile : LocalizationFileBase, ILocalizationContainer<RailwayDungeon>
{
    [JsonProperty("dataList")]
    public List<RailwayDungeon> DataList { get; set; }
}
public class RailwayDungeon
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("longName")]
    public string? LongName { get; set; }

}

public class DungeonAreaLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdContent>
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; }
}

public class QuestLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdContent>
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; }
}

public class StoryTheaterLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdTitleDesc>
{
    [JsonProperty("dataList")]
    public List<GenericIdTitleDesc> DataList { get; set; }
}

public class AnnouncerLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdName>
{
    [JsonProperty("dataList")]
    public List<GenericIdName> DataList { get; set; }
}

public class NormalBattleHintLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdContent>
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; }
}

public class AbBattleHintLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdContent>
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; }
}

public class BattleResultHintLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdContent>
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; }
}

public class TutorialDescLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdContent>
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; }
}

public class PersonalityVoiceLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdDescDlg>
{
    [JsonProperty("dataList")]
    public List<GenericIdDescDlg> DataList { get; set; }
}


public class AnnouncerVoiceLocalizationFile : LocalizationFileBase, ILocalizationContainer<AnnouncerVoice>
{
    [JsonProperty("dataList")]
    public List<AnnouncerVoice> DataList { get; set; }
}

public class AnnouncerVoice
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("dlg")]
    public string? Dialogue { get; set; }
}

public class EgoVoiceLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdDescDlg>
{
    [JsonProperty("dataList")]
    public List<GenericIdDescDlg> DataList { get; set; }
}

public class BattleSpeechBubbleLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdDescDlg>
{
    [JsonProperty("dataList")]
    public List<GenericIdDescDlg> DataList { get; set; }
}

public class IapProductLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdNameDesc>
{
    [JsonProperty("dataList")]
    public List<GenericIdNameDesc> DataList { get; set; }
}

public class IapStickerLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdContent>
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; }
}

public class GetConditionTextLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdContent>
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; }
}

public class ChoiceEventResultLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdContent>
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; }
}

public class BattlePassMissionLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdContent>
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; }
}

public class GachaTitleLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdContent>
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; }
}

public class GachaNoticeLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdContent>
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; }
}

public class IntroduceCharacterLocalizationFile : LocalizationFileBase, ILocalizationContainer<IntroductuceCharacter>
{
    [JsonProperty("dataList")]
    public List<IntroductuceCharacter> DataList { get; set; }
}
public class IntroductuceCharacter
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string? Name { get; set; } = string.Empty;

    [JsonProperty("desc")]
    public string? Desc { get; set; } = string.Empty;
    
    [JsonProperty("sentence")]
    public string? Sentence { get; set; } = string.Empty;
}
public class UserBannerLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdNameDesc>
{
    [JsonProperty("dataList")]
    public List<GenericIdNameDesc> DataList { get; set; }
}

public class UserTicketLLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdNameDesc>
{
    [JsonProperty("dataList")]
    public List<GenericIdNameDesc> DataList { get; set; }
}

public class UserTicketRLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdNameDesc>
{
    [JsonProperty("dataList")]
    public List<GenericIdNameDesc> DataList { get; set; }
}

public class UserTicketEGOBgLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdNameDesc>
{
    [JsonProperty("dataList")]
    public List<GenericIdNameDesc> DataList { get; set; }
}

public class BgmLyricsLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdContent>
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; }
}

public class ThreadDungeonLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdName>
{
    [JsonProperty("dataList")]
    public List<GenericIdName> DataList { get; set; }
}

public class RailwayDungeonStationNameLocalizationFile : LocalizationFileBase, ILocalizationContainer<RailwayDungeonStationNameList>
{
    [JsonProperty("dataList")]
    public List<RailwayDungeonStationNameList> DataList { get; set; }
}
public class RailwayDungeonStationNameList
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
    [JsonProperty("nameList")]
    public  List<RailwayDungeonStationNameList>? NameList { get; set; }
}

public class RailwayDungeonStationName
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("content")]
    public string? Content { get; set; } = string.Empty;

    [JsonProperty("shortName")]
    public string? ShortName { get; set; } = string.Empty;
}

public class RailwayDungeonBuffLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdContent>
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; }
}
public class DungeonNameLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdName>
{
    [JsonProperty("dataList")]
    public List<GenericIdName> DataList { get; set; }
}

public class MentalConditionLocalizationFile : LocalizationFileBase, ILocalizationContainer<MentalCondition>
{
    [JsonProperty("dataList")]
    public List<MentalCondition> DataList { get; set; }
}

public class MentalCondition
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("add")]
    public string? Add { get; set; } = string.Empty;

    [JsonProperty("min")]
    public string? Min { get; set; } = string.Empty;
}

public class DanteNoteLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdTitleDesc>
{
    [JsonProperty("dataList")]
    public List<GenericIdTitleDesc> DataList { get; set; }
}

public class DanteNoteCategoryKeywordLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdContent>
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; }
}

public class PanicInfoLocalizationFile : LocalizationFileBase, ILocalizationContainer<PanicInfo>
{
    [JsonProperty("dataList")]
    public List<PanicInfo> DataList { get; set; }
}

public class PanicInfo
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("panicName")]
    public string PanicName { get; set; } = string.Empty;

    [JsonProperty("lowMoraleDescription")]
    public string LowMoraleDescription { get; set; } = string.Empty;

    [JsonProperty("panicDescription")]
    public string PanicDescription { get; set; } = string.Empty;
}

public class DungeonStartBuffsLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdDescription>
{
    [JsonProperty("dataList")]
    public List<GenericIdDescription> DataList { get; set; }
}

public class EgoGiftCategoryLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdName>
{
    [JsonProperty("dataList")]
    public List<GenericIdName> DataList { get; set; }
}

public class MirrorDungeonEgoGiftLockedDescLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdContent>
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; }
}

public class DanteAbilityLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdDescRawDesc>
{
    [JsonProperty("dataList")]
    public List<GenericIdDescRawDesc> DataList { get; set; }
}

public class MirrorDungeonThemeLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdName>
{
    [JsonProperty("dataList")]
    public List<GenericIdName> DataList { get; set; }
}

public class UnlockCodeLocalizationFile : LocalizationFileBase, ILocalizationContainer<UnlockCode>
{
    [JsonProperty("dataList")]
    public List<UnlockCode> DataList { get; set; }
}

public class UnlockCode
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("openCondition")]
    public string? OpenCondition { get; set; } = string.Empty;
}

public class ScenarioModelCodesLocalizationFile : LocalizationFileBase, ILocalizationContainer<ScenarioModelCode>
{
    [JsonProperty("dataList")]
    public List<ScenarioModelCode> DataList { get; set; }
}

public class ScenarioModelCode
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string? Name { get; set; } = string.Empty;
    [JsonProperty("nickName")]
    public string? NickName { get; set; } = string.Empty;
}

public class StoryLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdContent>
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; }
}

public class AnnouncerVoiceTypeLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdContent>
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; }
}

public class MirrorDungeonRentalNameLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdContent>
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; }
}

public class ProjectGSLessonNameLocalizationFile : LocalizationFileBase, ILocalizationContainer<ProjectGSLessonName>
{
    [JsonProperty("dataList")]
    public List<ProjectGSLessonName> DataList { get; set; }
}

public class ProjectGSLessonName
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
    [JsonProperty("content")]
    public string? Content { get; set; }
    [JsonProperty("teacher")]
    public string? Teacher { get; set; }
}


public class ProjectGSComboNameLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdContent>
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; }
}
