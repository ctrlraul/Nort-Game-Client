using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Nort.Entities;
using Nort.Interface;

namespace Nort;

public static class DictionaryExtensions
{
    public static T GetTyped<T>(this IDictionary dictionary, string key)
    {
        if (key is null)
            throw new Exception("key can't be null");

        dynamic value = dictionary[key];

        if (value is not T)
            throw new Exception($"expected value of type ${typeof(T).Name} for key '{key}', got {value.GetType().Name} instead");
        
        return value;
    }
}

public class Mission : ISavable
{
    [JsonProperty] public string id;
    [JsonProperty] public string displayName;
    [JsonProperty] public List<Dictionary<string, object>> entitySetups = new();

    /*public IDictionary Serialize()
    {
        return new Dictionary<string, object>
        {
            { "id", id },
            { "displayName", displayName },
            { "entitySetups", entitySetups }
        };
    }

    public static Mission Deserialize(IDictionary dictionary)
    {
        Mission mission = new();

        var rawEntitySetups = dictionary.GetTyped<IEnumerable<IDictionary>>("entitySetups");

        mission.id = dictionary.GetTyped<string>("id");
        mission.displayName = dictionary.GetTyped<string>("displayName");
        mission.entitySetups = ParseEntitySetups(rawEntitySetups);

        return mission;
    }

    private static List<EntitySetup> ParseEntitySetups(IEnumerable<IDictionary> rawEntitySetups)
    {
        List<EntitySetup> entitySetups = new();
        
        foreach (IDictionary dictionary in rawEntitySetups)
        {
            string className = dictionary.GetTyped<string>("className");

            Type type = Type.GetType(className);

            if (type is null)
                throw new Exception($"No entity setup type found with name '{className}'");
            
            if (!type.IsSubclassOf(typeof(EntitySetup)))
                throw new Exception($"{type.Name} is not an entity setup type");
                
            
        }

        return entitySetups;
    }*/
}