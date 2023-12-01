using Godot;
using Newtonsoft.Json;

namespace Nort;

public class Skill
{
    [JsonProperty] public string id;
    [JsonProperty] public string displayName;
    [JsonProperty] public string scenePath;
    [JsonProperty] public int mass;

    private PackedScene _scene;
    public PackedScene Scene => _scene ??= GD.Load<PackedScene>(Config.SkillsPath.PathJoin(scenePath));
}