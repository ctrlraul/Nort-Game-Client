using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

namespace Nort.Entities;


[AttributeUsage(AttributeTargets.Property)]
public class SavableAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Event)]
public class ConnectableAttribute : Attribute
{
    private static readonly Dictionary<Type, List<EventInfo>> EventsCache = new();
    private static readonly Dictionary<Type, List<MethodInfo>> MethodsCache = new();

    public static IEnumerable<EventInfo> GetConnectableEvents(Type type)
    {
        if (EventsCache.TryGetValue(type, out List<EventInfo> cachedEvents))
            return cachedEvents;

        List<EventInfo> events = type.GetEvents()
            .Where(info => info.GetCustomAttribute<ConnectableAttribute>() != null)
            .ToList();

        EventsCache.Add(type, events);

        return events;
    }

    public static IEnumerable<MethodInfo> GetConnectableMethods(Type type)
    {
        if (MethodsCache.TryGetValue(type, out List<MethodInfo> cachedMethods))
            return cachedMethods;

        List<MethodInfo> methods = type.GetMethods()
            .Where(info => info.GetCustomAttribute<ConnectableAttribute>() != null)
            .ToList();

        MethodsCache.Add(type, methods);

        return methods;
    }
}


public class EntitySetup
{
    private static readonly Dictionary<Type, List<PropertyInfo>> EntityPropertiesCache = new();

    public bool autoSpawn;
    public float angle;
    public string uuid;
    public string typeName;
    public Vector2 position;
    public List<EntityConnection> connections;
    public Dictionary<string, object> subTypeData;

    public EntitySetup()
    {
    }

    public EntitySetup(
        bool autoSpawn,
        float angle,
        string uuid,
        string typeName,
        Vector2 position,
        List<EntityConnection> connections,
        Dictionary<string, object> subTypeData)
    {
        this.autoSpawn = autoSpawn;
        this.angle = angle;
        this.uuid = uuid;
        this.typeName = typeName;
        this.position = position;
        this.connections = connections;
        this.subTypeData = subTypeData;
    }


    public bool IsForType<T>() where T : Entity
    {
        return typeName == typeof(T).Name;
    }

    public void Inject(Entity entity)
    {
        entity.AutoSpawned = autoSpawn;
        entity.RotationDegrees = angle;
        entity.Uuid = uuid;
        entity.Position = position;
        entity.Connections.AddRange(connections);

        foreach (PropertyInfo property in GetSavableProperties(entity))
        {
            if (subTypeData.TryGetValue(property.Name, out object value))
                property.SetValue(entity, value);
        }
    }

    public static EntitySetup From(Entity entity)
    {
        return new EntitySetup
        (
            entity.AutoSpawned,
            Mathf.Round(entity.RotationDegrees),
            entity.Uuid,
            entity.GetType().Name,
            entity.Position,
            entity.Connections,
            GetSavableProperties(entity).ToDictionary(info => info.Name, info => info.GetValue(entity))
        );
    }

    private static List<PropertyInfo> GetSavableProperties(Entity entity)
    {
        Type type = entity.GetType();

        if (EntityPropertiesCache.TryGetValue(type, out List<PropertyInfo> cachedProperties))
            return cachedProperties;

        List<PropertyInfo> properties = type.GetProperties()
            .Where(info => info.GetCustomAttribute<SavableAttribute>() != null)
            .ToList();

        EntityPropertiesCache.Add(type, properties);

        return properties;
    }
}


public class EntityConnection
{
    public string targetUuid;
    public string eventName;
    public string methodName;
}

public abstract partial class Entity : Node2D
{
    protected virtual float Damp { get; } = 0.95f;

    public bool AutoSpawned { get; set; } = true;
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