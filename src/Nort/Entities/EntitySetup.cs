using Godot;
using Newtonsoft.Json;
using Nort.Interface;

namespace Nort.Entities;

public class EntitySetup : ISavable
{
    public enum EntityType
    {
        Craft,
        PlayerCraft,
        OrphanPart
    }

    [JsonProperty] public EntityType Type { get; protected set; }
    [JsonProperty] public float x;
    [JsonProperty] public float y;
    
    [JsonIgnore] public Vector2 Place
    {
        get => new(x, y);
        set
        {
            x = value.X;
            y = value.Y;
        }
    }
}

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