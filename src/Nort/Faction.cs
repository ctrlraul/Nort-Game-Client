using Godot;
using Newtonsoft.Json;

namespace Nort;

public class Faction
{
    [JsonProperty] public string id;
    [JsonProperty] public string displayName;
    [JsonProperty] public string colorHex;
    [JsonProperty] public int collisionLayer;

    [JsonIgnore] private Color? _color;
    [JsonIgnore] public Color Color => _color ??= new Color(colorHex);
}