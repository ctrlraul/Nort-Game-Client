using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

namespace Nort.Entities;

[AttributeUsage(AttributeTargets.Property)]
public class SavableAttribute : Attribute { }

public class EntitySetup : Dictionary<string, object> { }

public partial class Entity : Node2D
{
    #region Static

    private static readonly Dictionary<Type, List<PropertyInfo>> PropertiesCache = new();
    
    
    public static Dictionary<string, object> GetSetup(Entity entity)
    {
        List<PropertyInfo> properties = GetSavableProperties(entity);
        
        EntitySetup entitySetup = new();

        foreach (PropertyInfo property in properties)
            entitySetup.Add(property.Name, property.GetValue(entity));
        
        return entitySetup;
    }

    public static void SetSetup(Entity entity, EntitySetup setup)
    {
        foreach (PropertyInfo property in GetSavableProperties(entity))
        {
            if (property.Name == "Type")
                continue;

            if (!setup.TryGetValue(property.Name, out object savedValue))
                continue;
            
            property.SetValue(entity, savedValue is double dbl ? (float)dbl : savedValue);
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
    
    [Savable] public string Type => GetType().Name;
    
    [Savable]
    public float X
    {
        get => Position.X;
        set => Position = Position with { X = value };
    }
    
    [Savable]
    public float Y
    {
        get => Position.Y;
        set => Position = Position with { Y = value };
    }
    
    [Savable]
    public float Angle
    {
        get => RotationDegrees;
        set => RotationDegrees = value;
    }
    
    
    public Vector2 Velocity { get; set; } = Vector2.Zero;

    
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        
        delta *= Engine.PhysicsTicksPerSecond; // Normalize delta

        Position += Velocity * (float)delta;
        Velocity *= Damp;
    }
}