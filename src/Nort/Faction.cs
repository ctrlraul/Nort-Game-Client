using Godot;
using Newtonsoft.Json;

namespace Nort;

public class Faction
{
    [JsonProperty] public string id;
    [JsonProperty] public string displayName;
    [JsonProperty] public string colorHex;

    [JsonIgnore] private Color? _color;
    [JsonIgnore] public Color Color => _color ??= new Color(colorHex);

    
    public static bool Allied(Faction a, Faction b)
    {
        return a.id == b.id; // Simple for now
    }
    
    public static bool Hostile(Faction a, Faction b)
    {
        return !Allied(a, b);
    }
}