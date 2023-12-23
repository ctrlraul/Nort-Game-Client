using System.Collections.Generic;
using System.Linq;
using Godot;
using Newtonsoft.Json;
using Nort.Interface;
using Shouldly;

namespace Nort;

public class BlueprintStats
{
    public int core;
    public int hull;
    public int mass;
    public float torque;
    public float acceleration;
}

public class Blueprint : ISavable
{
    [JsonProperty] public string id;
    [JsonProperty] public BlueprintPart core;
    [JsonProperty] public List<BlueprintPart> hulls = new();

    public static Blueprint From(Blueprint blueprint)
    {
        Blueprint newBlueprint = new();
        newBlueprint.id = Assets.GenerateUuid();
        newBlueprint.core = blueprint.core;
        newBlueprint.hulls.AddRange(blueprint.hulls);
        return newBlueprint;
    }

    public static int GetCoreStat(Blueprint blueprint)
    {
        int result = 0;
        result += blueprint.core.Part.core;
        result += blueprint.hulls.Sum(hull => hull.Part.core);
        return result;
    }

    public static int GetHullStat(Blueprint blueprint)
    {
        int result = 0;
        result += blueprint.core.Part.hull;
        result += blueprint.hulls.Sum(hull => hull.Part.hull);
        return result;
    }

    public static int GetMassStat(Blueprint blueprint)
    {
        int result = 0;
        result += blueprint.core.Mass;
        result += blueprint.hulls.Sum(hull => hull.Mass);
        return result;
    }

    public static BlueprintStats GetStats(Blueprint blueprint)
    {
        blueprint.ShouldNotBeNull();
        
        BlueprintStats stats = new()
        {
            torque = 1,
            acceleration = 1,
            core = blueprint.core.Part.core,
            hull = blueprint.core.Part.hull,
            mass = blueprint.core.Mass
        };
        
        foreach (BlueprintPart hull in blueprint.hulls)
        {
            stats.core += hull.Part.core;
            stats.hull += hull.Part.hull;
            stats.mass += hull.Mass;
        }
        
        return stats;
    }
}