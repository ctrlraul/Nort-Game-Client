using Newtonsoft.Json;

namespace Nort.Entities;

public class CraftSetup : EntitySetup
{
    [JsonProperty] public string blueprintId;
    [JsonProperty] public string factionId;
    [JsonProperty] public Craft.ComponentSet componentSet;

    [JsonIgnore] public Blueprint Blueprint
    {
        get => Assets.Instance.GetBlueprint(blueprintId);
        set => blueprintId = value.id;
    }

    [JsonIgnore]
    public Faction Faction
    {
        get => Assets.Instance.GetFaction(factionId);
        set => factionId = value.id;
    }
}