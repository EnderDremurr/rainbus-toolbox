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
public class BattleHintsFile : LocalizationFileBase, ILocalizationContainer<GenericIdContent>
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; } = new List<GenericIdContent>();
}




// BattleAnnouncerDlg/Announcer* (Battle announcer dialogue) +
[FilePattern("Announcer*")]
public class BattleAnnouncerFile : LocalizationFileBase, ILocalizationContainer<BattleAnnouncerEntry>
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

public class PersonalityVoiceFile : LocalizationFileBase, ILocalizationContainer<PersonalityVoiceEntry>
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
public class EGOVoiceFile : LocalizationFileBase, ILocalizationContainer<EGOVoiceEntry>
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


//StoryTheater* (Story theater UI and notes)+
public class StoryTheaterFile : LocalizationFileBase, ILocalizationContainer<GenericIdContent>
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; }
}

//StoryTheater*Detail (Story theater UI and notes with details)+
public class StoryTheaterDetailFile : LocalizationFileBase, ILocalizationContainer<GenericIdTitleDesc>
{
    [JsonProperty("dataList")]
    public List<GenericIdTitleDesc> DataList { get; set; }
}


// StageNode* (Stage narrative content)+
public class StageNodeFile : LocalizationFileBase, ILocalizationContainer<GenericIdTitle>
{
    [JsonProperty("dataList")]
    public List<GenericIdTitle> DataList { get; set; }
}
//DungeonNode* (Dungeon narrative content)
public class DungeonNodeFile : LocalizationFileBase, ILocalizationContainer<DungeonNodeItem>
{
    [JsonProperty("dataList")]
    public List<DungeonNodeItem> DataList { get; set; }
}

public class DungeonNodeItem
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("stageList")]
    public List<GenericIdTitleDesc> StageList { get; set; } = new List<GenericIdTitleDesc>();
}


//Passives* (Passive abilities)
public class PassivesFile : LocalizationFileBase, ILocalizationContainer<GenericIdNameDesc>
{
    [JsonProperty("dataList")]
    public List<GenericIdNameDesc> DataList { get; set; }
}

[FilePattern("Bufs*")] // bufs with one f!!! (its like that in game)
public class BuffsFile : LocalizationFileBase, ILocalizationContainer<Buff>
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
[FilePattern("EGOGift*")]
public class EGOGiftFile : LocalizationFileBase, ILocalizationContainer<GenericIdNameDesc>
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



//*UIText* (UI elements)
public class UITextFile
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; }
}



//
//
/*** TODO:
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   Tutorial* (Tutorial text)
   
   
   Gacha* (Gacha system text)
   
   
 * *UI*
   
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