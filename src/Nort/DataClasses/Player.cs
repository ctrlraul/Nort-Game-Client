using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Nort.Interface;

namespace Nort;

public class MissionRecord
{
    [JsonProperty] public float bestTime;
    [JsonProperty] public float bestScore;
}

public class Player : ISavable
{
    [JsonProperty] public string id = string.Empty;
    [JsonProperty] public string nick = string.Empty;
    [JsonProperty] public Blueprint blueprint;
    [JsonProperty] public List<PartData> parts = new();
    [JsonProperty] public Dictionary<string, MissionRecord> missionRecords = new();
    
    [JsonIgnore] public IEnumerable<PartData> Cores => parts.Where(partData => partData.Part.type == Part.Type.Core);
    [JsonIgnore] public IEnumerable<PartData> Hulls => parts.Where(partData => partData.Part.type == Part.Type.Hull);
}