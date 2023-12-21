using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

namespace Nort.Entities;


[AttributeUsage(AttributeTargets.Property)]
public class SavableAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Event)]
public class ConnectableAttribute : Attribute { }


public class EntitySetup
{
    public string uuid;
    public string typeName;
    public Vector2 position;
    public float angle;
    public List<EntityConnection> connections = new();
    public Dictionary<string, object> subTypeData = new();
}


public class EntityConnection
{
    public string targetUuid;
    public string eventName;
    public string methodName;
}

public partial class Entity : Node2D
{
    #region Static

    private static readonly Dictionary<Type, List<PropertyInfo>> PropertiesCache = new();
    private static readonly Dictionary<Type, List<EventInfo>> EventsCache = new();
    private static readonly Dictionary<Type, List<MethodInfo>> MethodsCache = new();
    
    
    public static bool IsEntitySetupOfType<T>(EntitySetup setup) where T : Entity
    {
        return setup.typeName ==  typeof(T).Name;
    }
    
    public static EntitySetup GetSetup(Entity entity)
    {
        return new()
        {
            uuid = entity.Uuid,
            typeName = entity.GetType().Name,
            position = entity.Position,
            angle = Mathf.Round(entity.RotationDegrees),
            connections = entity.Connections,
            subTypeData = GetSavableProperties(entity).ToDictionary(info => info.Name, info => info.GetValue(entity))
        };
    }

    public static void SetSetup(Entity entity, EntitySetup setup)
    {
        entity.Uuid = setup.uuid;
        entity.Position = setup.position;
        entity.RotationDegrees = setup.angle;
        entity.Connections.AddRange(setup.connections);

        foreach (PropertyInfo property in GetSavableProperties(entity))
        {
            if (setup.subTypeData.TryGetValue(property.Name, out object value))
                property.SetValue(entity, value);
        }
    }

    private static List<PropertyInfo> GetSavableProperties(Entity entity)
    {
        Type type = entity.GetType();
        
        if (PropertiesCache.TryGetValue(type, out List<PropertyInfo> cachedProperties))
        {
            return cachedProperties;
        }
        
        List<PropertyInfo> properties = type.GetProperties()
            .Where(info => info.GetCustomAttribute<SavableAttribute>() != null)
            .ToList();
        
        PropertiesCache.Add(type, properties);

        return properties;
    }

    public static List<EventInfo> GetConnectableEvents(Entity entity)
    {
        Type type = entity.GetType();
        
        if (EventsCache.TryGetValue(type, out List<EventInfo> cachedEvents))
        {
            return cachedEvents;
        }
        
        List<EventInfo> events = type.GetEvents()
            .Where(info => info.GetCustomAttribute<ConnectableAttribute>() != null)
            .ToList();
        
        EventsCache.Add(type, events);

        return events;
    }

    public static List<MethodInfo> GetConnectableMethods(Node node)
    {
        Type type = node.GetType();
        
        if (MethodsCache.TryGetValue(type, out List<MethodInfo> cachedMethods))
        {
            return cachedMethods;
        }
        
        List<MethodInfo> methods = type.GetMethods()
            .Where(info => info.GetCustomAttribute<ConnectableAttribute>() != null)
            .ToList();
        
        MethodsCache.Add(type, methods);

        return methods;
    }
    
    #endregion
    
    
    private const float Damp = 0.95f;

    public string Uuid { get; set; } = Assets.GenerateUuid();
    
    public List<EntityConnection> Connections { get; set; } = new();
    public Vector2 Velocity { get; set; } = Vector2.Zero;

    
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        
        delta *= Engine.PhysicsTicksPerSecond; // Normalize delta

        Position += Velocity * (float)delta;
        Velocity *= Damp;
    }
}