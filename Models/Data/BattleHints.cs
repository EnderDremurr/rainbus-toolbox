using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public abstract class LocalizationFileBase
{
    [JsonIgnore]
    public string PathTo { get; private set; }

    [JsonIgnore]
    public string FileName { get; private set; }

    [JsonIgnore]
    public string FullPath { get; private set; }

    // Protected constructor for inheritance
    protected LocalizationFileBase(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new System.ArgumentException("File path cannot be null or empty", nameof(filePath));

        FullPath = filePath;
        PathTo = Path.GetDirectoryName(filePath) ?? string.Empty;
        FileName = Path.GetFileNameWithoutExtension(filePath) ?? string.Empty;
    }

    // Parameterless constructor for JSON deserialization
    protected LocalizationFileBase()
    {
        // Will be populated later by deserializer
    }

    // Method to set path info after JSON deserialization
    internal void SetPathInfo(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new System.ArgumentException("File path cannot be null or empty", nameof(filePath));

        FullPath = filePath;
        PathTo = Path.GetDirectoryName(filePath) ?? string.Empty;
        FileName = Path.GetFileNameWithoutExtension(filePath) ?? string.Empty;
    }
}

public enum BattleHintTypes
{
    Loading,
    Battle,
    Abnormality,
}
public class BattleHint
{
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("content")]
    public string Content { get; set; }
}

public class BattleHintsFile
{
    [JsonProperty("dataList")]
    public List<BattleHint> DataList { get; set; } = new List<BattleHint>();
}


// AbDlg* (Character dialogue files - DonQuixote, Faust, Gregor, etc.) +
public class DialogueFile
{
    [JsonProperty("dataList")]
    public List<DialogueEntry> DataList { get; set; }
}

public class DialogueEntry
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
public class BattleAnnouncerFile
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

public class PersonalityVoiceFile
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
public class EGOVoiceFile
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
public class StoryDataFile
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
public class StoryTextFile
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
public class StoryTheaterFile
{
    [JsonProperty("dataList")]
    public List<StoryTheaterItem> DataList { get; set; }
}

public class StoryTheaterItem
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("content")]
    public string Content { get; set; } = string.Empty;
}

//StoryTheater*Detail (Story theater UI and notes with details)+
public class StoryTheaterDetailFile
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
public class StageNodeFile
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
public class DungeonNodeFile
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

//Skills* (Character and enemy skills)
public class SkillsFile
{
    [JsonProperty("dataList")]
    public List<Skill> DataList { get; set; }
}

public class Skill
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("levelList")]
    public List<SkillLevel> LevelList { get; set; }
}

public class SkillLevel
{
    [JsonProperty("level")]
    public int Level { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("desc")]
    public string Desc { get; set; } = string.Empty;

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
    public string Desc { get; set; } = string.Empty;
}

//Passives* (Passive abilities)
public class PassivesFile
{
    [JsonProperty("dataList")]
    public List<PassiveAbility> DataList { get; set; }
}

public class PassiveAbility
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("desc")]
    public string Desc { get; set; } = string.Empty;
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
    public List<EGOGift> DataList { get; set; }
}

public class EGOGift
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("desc")]
    public string Desc { get; set; } = string.Empty;
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
    public List<Enemy> DataList { get; set; }
}

public class Enemy
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("desc")]
    public string Description { get; set; } = string.Empty;  // "Ядро" or "Часть"
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

//MainUIText* (Main UI elements)
public class MainUITextFile
{
    [JsonProperty("dataList")]
    public List<MainUIText> DataList { get; set; }
}

public class MainUIText
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("content")]
    public string Content { get; set; } = string.Empty;
}
//BattleUIText.json

public class BattleUITextFile
{
    [JsonProperty("dataList")]
    public List<BattleUIText> DataList { get; set; }
}

public class BattleUIText
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("content")]
    public string Content { get; set; } = string.Empty;
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
   
   CultivationEvent.json
   
   NightCleanUpEvent.json
   
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
   
   HellsChicken* (Hell's Chicken game mode)
   
   ProjectGS* (Project GS content)
   
   BgmLyrics/* (Background music lyrics)
***/