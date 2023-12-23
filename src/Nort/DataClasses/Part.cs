using Newtonsoft.Json;

namespace Nort;

public class Part
{
    public enum Type
    {
        Core,
        Hull,
    }

    [JsonProperty] public string id;
    [JsonProperty] public string displayName;
    [JsonProperty] public string textureName;
    [JsonProperty] public Type type;
    [JsonProperty] public int core;
    [JsonProperty] public int hull;
    [JsonProperty] public int mass;

    public static bool IsCore(Part part)
    {
        return part.type == Type.Core;
    }
}