using System.Collections.Generic;
using Newtonsoft.Json;
using Nort.Entities;
using Nort.Interface;

namespace Nort;

public class Mission : ISavable
{
    [JsonProperty] public string id;
    [JsonProperty] public string displayName;
    [JsonProperty] public string description;
    [JsonProperty] public List<EntitySetup> entitySetups = new();
}