using Godot;
using Newtonsoft.Json;

namespace Nort;

public partial class BlueprintPart : RefCounted
{
    [JsonProperty] public bool flipped;
    [JsonProperty] public float x;
    [JsonProperty] public float y;
    [JsonProperty] public float angle;
    [JsonProperty] public bool shiny;
    [JsonProperty] public string partId;
    [JsonProperty] public string skillId;
    
    [JsonIgnore] public Vector2 Place
    {
        get => new(x, y);
        set
        {
            x = value.X;
            y = value.Y;
        }
    }

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

    [JsonIgnore] public int Mass => Part.mass + (string.IsNullOrEmpty(skillId) ? 0 : Skill.mass);
    
    public static BlueprintPart From(Part part)
    {
        return new BlueprintPart { partId = part.id };
    }
    
    public static BlueprintPart From(PartData partData)
    {
        return new BlueprintPart
        {
            partId = partData.partId,
            skillId = partData.skillId,
            shiny = partData.shiny,
        };
    }
}