using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

namespace Nort.Entities;

[AttributeUsage(AttributeTargets.Property)]
public class SavableAttribute : Attribute { }

public class EntitySetup : Dictionary<string, object> { }
public class EntitySetup
{
    public string typeName;
    public Vector2 position;
    public float angle;
    public Dictionary<string, object> subTypeData = new();
}

public partial class Entity : Node2D
{
    #region Static

    private static readonly Dictionary<Type, List<PropertyInfo>> PropertiesCache = new();
    
    
    public static bool IsEntitySetupOfType<T>(EntitySetup setup) where T : Entity
    {
        return setup.typeName ==  typeof(T).Name;
    }
    
    public static EntitySetup GetSetup(Entity entity)
    {
        return new()
        {
            typeName = entity.GetType().Name,
            position = entity.Position,
            angle = Mathf.Round(entity.RotationDegrees),
            subTypeData = GetSavableProperties(entity).ToDictionary(info => info.Name, info => info.GetValue(entity))
        };
    }

    public static void SetSetup(Entity entity, EntitySetup setup)
    {
        entity.Position = setup.position;
        entity.RotationDegrees = setup.angle;

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

    #endregion
    
    
    private const float Damp = 0.95f;
    
    
    public Vector2 Velocity { get; set; } = Vector2.Zero;

    
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        
        delta *= Engine.PhysicsTicksPerSecond; // Normalize delta

        Position += Velocity * (float)delta;
        Velocity *= Damp;
    }
}