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
