using Newtonsoft.Json;

namespace Nort.Entities;

public class PlayerCraftSetup : EntitySetup
{
    [JsonProperty] public Blueprint testBlueprint;
}