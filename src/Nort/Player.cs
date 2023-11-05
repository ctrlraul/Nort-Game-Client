using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Nort.Interface;

namespace Nort;

public class Player : ISavable
{
    [JsonProperty] public int currentBlueprintIndex;
    [JsonProperty] public string id = "";
    [JsonProperty] public string nick = "";
    [JsonProperty] public List<Blueprint> blueprints = new();
    [JsonProperty] public List<PartData> parts = new();
    
    [JsonIgnore] public IEnumerable<PartData> Cores => parts.Where(partData => partData.Part.type == Part.Type.Core);
    [JsonIgnore] public IEnumerable<PartData> Hulls => parts.Where(partData => partData.Part.type == Part.Type.Hull);

    [JsonIgnore] public Blueprint CurrentBlueprint
    {
        get => blueprints.Count > currentBlueprintIndex ? blueprints[currentBlueprintIndex] : null;
        set => currentBlueprintIndex = blueprints.IndexOf(value);
    }
}