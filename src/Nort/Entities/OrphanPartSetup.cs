using Newtonsoft.Json;

namespace Nort.Entities;

public class OrphanPartSetup : EntitySetup
{
    [JsonProperty] public bool flipped;
    [JsonProperty] public bool shiny;
    [JsonProperty] public float angle;
    [JsonProperty] public string partId;
    [JsonProperty] public string skillId;
    
    [JsonIgnore] public Part Part
    {
        get => Assets.Instance.GetPart(partId);
        set => partId = value.id;
    }

    [JsonIgnore] public Skill Skill
    {
        get => Assets.Instance.GetSkill(skillId);
        set => partId = value.id;
    }
}