using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;




// BattleHint*
[FilePattern("BattleHint*")]
public class BattleHintsFile : LocalizationFileBase, ILocalizationContainer<GenericIdContent>
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; } = new List<GenericIdContent>();
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
//TODO: Implement editor

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
//TODO: Implement editor









//Enemies* (Enemy data)
public class EnemyFile
{
    [JsonProperty("dataList")]
    public List<GenericIdNameDesc> DataList { get; set; }
}
//TODO: Implement editor




//*UIText* (UI elements)
public class UITextFile
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; }
}
//TODO: Implement editor


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