using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Nort.Interface;

namespace Nort;

public class Player : ISavable
{
    [JsonProperty] public string id = string.Empty;
    [JsonProperty] public string nick = string.Empty;
    [JsonProperty] public Blueprint blueprint;
    [JsonProperty] public List<PartData> parts = new();
    
    [JsonIgnore] public IEnumerable<PartData> Cores => parts.Where(partData => partData.Part.type == Part.Type.Core);
    [JsonIgnore] public IEnumerable<PartData> Hulls => parts.Where(partData => partData.Part.type == Part.Type.Hull);
}