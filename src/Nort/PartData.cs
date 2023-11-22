#nullable enable
using Newtonsoft.Json;
using Nort.Interface;

namespace Nort;

public class PartData : ISavable
{
    [JsonProperty] public bool shiny;
    [JsonProperty] public string? partId;
    [JsonProperty] public string? skillId;

    [JsonIgnore] public string Discriminator => $"{(partId, skillId, shiny)}";

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

    #region Override == and !=
    
    public override bool Equals(object? obj) => obj is PartData partData && Discriminator == partData.Discriminator;
    public override int GetHashCode() => Discriminator.GetHashCode();
    public static bool operator ==(PartData? obj1, PartData? obj2) => ReferenceEquals(obj1, obj2) && obj1 is not null && obj1.Equals(obj2);
    public static bool operator !=(PartData obj1, PartData obj2) => !(obj1 == obj2);

    #endregion
}