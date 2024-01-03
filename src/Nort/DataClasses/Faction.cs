using Godot;
using Newtonsoft.Json;

namespace Nort;

public class Faction
{
    [JsonProperty] public string id;
    [JsonProperty] public string displayName;
    [JsonProperty] public string colorHex;
    [JsonProperty] public string colorShinyHex;
    [JsonProperty] public int collisionLayer;

    [JsonIgnore] private Color? color;
    [JsonIgnore] public Color Color => color ??= new Color(colorHex);

    [JsonIgnore] private Color? colorShiny;
    [JsonIgnore] public Color ColorShiny => colorShiny ??= new Color(colorShinyHex);
}