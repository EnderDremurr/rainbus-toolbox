using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;


public enum BattleHintTypes
{
    Loading,
    Battle,
    Abnormality,
}


// BattleHint*
[FilePattern("BattleHint*")]
public class BattleHintsFile : LocalizationFileBase
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; } = new List<GenericIdContent>();
}


// AbDlg* (Character dialogue files - DonQuixote, Faust, Gregor, etc.) +
[FilePattern("AbDlg*")]
public class DialogueFile : LocalizationFileBase
{
    [JsonProperty("dataList")]
    public List<DialogueEntry> DataList { get; set; }
}

public class DialogueEntry : LocalizationFileBase
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("personalityid")]
    public int PersonalityId { get; set; }

    [JsonProperty("voicefile")]
    public int VoiceFile { get; set; }

    [JsonProperty("teller")]
    public string Teller { get; set; }

    [JsonProperty("dialog")]
    public string Dialog { get; set; }

    // Keeping as string since format is non-standard, can parse later if needed
    [JsonProperty("usage")]
    public string Usage { get; set; }
}

// BattleAnnouncerDlg/Announcer* (Battle announcer dialogue) +
public class BattleAnnouncerFile : LocalizationFileBase
{
    [JsonProperty("dataList")]
    public List<BattleAnnouncerEntry> DataList { get; set; }
}

public class BattleAnnouncerEntry
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("dlg")]
    public string Dialogue { get; set; }
}

//PersonalityVoiceDlg/Voice_* (Character personality voice lines)+

public class PersonalityVoiceFile : LocalizationFileBase
{
    [JsonProperty("dataList")]
    public List<PersonalityVoiceEntry> DataList { get; set; }
}

public class PersonalityVoiceEntry
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("desc")]
    public string Description { get; set; }

    [JsonProperty("dlg")]
    public string Dialogue { get; set; }
}

//EGOVoiceDig/VoiceEGO* (EGO ability voice lines) +
public class EGOVoiceFile : LocalizationFileBase
{
    [JsonProperty("dataList")]
    public List<EGOVoiceEntry> DataList { get; set; }
}

public class EGOVoiceEntry
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("desc")]
    public string Description { get; set; }

    [JsonProperty("dlg")]
    public string Dialogue { get; set; }
}

//StoryData/* (Individual story scene files)+
public class StoryDataFile : LocalizationFileBase
{
    [JsonProperty("dataList")]
    public List<StoryDataItem> DataList { get; set; }
}

public class StoryDataItem
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("model")]
    public string? Model { get; set; } // Optional

    [JsonProperty("teller")]
    public string? Teller { get; set; } // Optional

    [JsonProperty("title")]
    public string? Title { get; set; } // Optional

    [JsonProperty("place")]
    public string? Place { get; set; } // Optional

    [JsonProperty("content")]
    public string Content { get; set; } = string.Empty; // Always present
}

//StoryText.json +
public class StoryTextFile : LocalizationFileBase
{
    [JsonProperty("dataList")]
    public List<StoryTextItem> DataList { get; set; }
}

public class StoryTextItem
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    [JsonProperty("desc")]
    public string Desc { get; set; } = string.Empty;
}
//StoryTheater* (Story theater UI and notes)+
public class StoryTheaterFile : LocalizationFileBase
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; }
}

//StoryTheater*Detail (Story theater UI and notes with details)+
public class StoryTheaterDetailFile : LocalizationFileBase
{
    [JsonProperty("dataList")]
    public List<StoryTheaterDetailItem> DataList { get; set; }
}

public class StoryTheaterDetailItem
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    [JsonProperty("desc")]
    public string Description { get; set; } = string.Empty;
}

// StageNode* (Stage narrative content)+
public class StageNodeFile : LocalizationFileBase
{
    [JsonProperty("dataList")]
    public List<StageNodeItem> DataList { get; set; }
}

public class StageNodeItem
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;
}

//DungeonNode* (Dungeon narrative content)
public class DungeonNodeFile : LocalizationFileBase
{
    [JsonProperty("dataList")]
    public List<DungeonNodeItem> DataList { get; set; }
}

public class DungeonNodeItem
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("stageList")]
    public List<DungeonStage> StageList { get; set; } = new List<DungeonStage>();
}

public class DungeonStage
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    [JsonProperty("desc")]
    public string Desc { get; set; } = string.Empty;
}


//Passives* (Passive abilities)
public class PassivesFile : LocalizationFileBase
{
    [JsonProperty("dataList")]
    public List<GenericIdNameDesc> DataList { get; set; }
}


//Buffs* (Buff descriptions)
public class BuffsFile
{
    [JsonProperty("dataList")]
    public List<Buff> DataList { get; set; }
}

public class Buff
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("desc")]
    public string Desc { get; set; } = string.Empty;

    [JsonProperty("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonProperty("undefined")]
    public string Undefined { get; set; } = string.Empty;
}
//EGOGift* (EGO gifts)
public class EGOGiftFile
{
    [JsonProperty("dataList")]
    public List<GenericIdNameDesc> DataList { get; set; }
}

//AbnormalityGuides* (Abnormality descriptions)
public class AbnormalityGuideFile
{
    [JsonProperty("dataList")]
    public List<AbnormalityGuide> DataList { get; set; }
}

public class AbnormalityGuide
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("codeName")]
    public string CodeName { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("clue")]
    public string Clue { get; set; } = string.Empty;

    [JsonProperty("storyList")]
    public List<AbnormalityStory> StoryList { get; set; }
}

public class AbnormalityStory
{
    [JsonProperty("level")]
    public int Level { get; set; }

    [JsonProperty("story")]
    public string Story { get; set; } = string.Empty;
}

//Enemies* (Enemy data)
public class EnemyFile
{
    [JsonProperty("dataList")]
    public List<GenericIdNameDesc> DataList { get; set; }
}

//BattleKeywords* (Battle terminology)
public class BattleKeywordFile
{
    [JsonProperty("dataList")]
    public List<BattleKeyword> DataList { get; set; }
}

public class BattleKeyword
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("desc")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonProperty("undefined")]
    public string Undefined { get; set; } = "-";
}

// PanicInfo* (Panic system information)
public class PanicInfoFile
{
    [JsonProperty("dataList")]
    public List<PanicInfo> DataList { get; set; }
}

public class PanicInfo
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("panicName")]
    public string PanicName { get; set; } = string.Empty;

    [JsonProperty("lowMoraleDescription")]
    public string LowMoraleDescription { get; set; } = string.Empty;

    [JsonProperty("panicDescription")]
    public string PanicDescription { get; set; } = string.Empty;
}

//*UIText* (UI elements)
public class UITextFile
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; }
}



//
//
/*** TODO:
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   ShopUI.json
   
   MissionUIText.json
   
   Tutorial* (Tutorial text)
   
   FormationUI* (Formation screen text)
   
   Gacha* (Gacha system text)
   
   LoginUIText.json
   
   AbEvents* (Abnormality events)
   
   ActionEvents* (Action events)
   
   ChoiceEvent* (Choice events)
   
   Event* (Various event types - MOWE, TKT, YCGD, etc.)
   
   *EventText
 
    *Event
   
   
   RailwayDungeon* (Railway dungeon events)
   
   Characters.json
   
   Personalities.json
   
   IntroduceCharacter.json
   
   UnitKeyword* (Character unit keywords)
   
   KeywordDictionary.json
   
   ResistText.json
   
   SuccessRate.json
   
   AttributeText.json
   
   AssociationName.json
   
   BattlePass* (Battle pass content)
   
   IAP* (In-app purchase content)
   
   UserAgreements.json
   
   FAQ.json
   
   ReturnPolicy.json
   
   ShotcutKeyManual.json
   
   MirrorDungeon* (Mirror dungeon content)
   
   RailwayDungeon* (Railway dungeon content)
   
   
   ProjectGS* (Project GS content)
   
   BgmLyrics/* (Background music lyrics)
***/