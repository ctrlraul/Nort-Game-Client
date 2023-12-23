#nullable enable
using Newtonsoft.Json;
using Nort.Interface;

namespace Nort;

public class PartData : ISavable
{
    [JsonProperty] public bool shiny;
    [JsonProperty] public string? partId;
    [JsonProperty] public string? skillId;

    
    [JsonIgnore] public Part Part
    {
        get => Assets.Instance.GetPart(partId);
        set => partId = value.id;
    }

    [JsonIgnore] public Skill? Skill
    {
        get => string.IsNullOrEmpty(skillId) ? null : Assets.Instance.GetSkill(skillId);
        set => skillId = value != null ? value.id: string.Empty;
    }


    public static PartData From(Part part)
    {
        return new PartData { partId = part.id };
    }

    public static PartData From(BlueprintPart blueprintPart)
    {
        return new PartData
        {
            partId = blueprintPart.partId,
            skillId = blueprintPart.skillId,
            shiny = blueprintPart.shiny
        };
    }
}