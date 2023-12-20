using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;

namespace CtrlRaul.Godot;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ReadyAttribute : Attribute
{
    public string Path { get; }

    public ReadyAttribute(string path)
    {
        Path = path;
    }

    public ReadyAttribute()
    {
        Path = null;
    }
}

public static class NodeExtensions
{
    private static readonly Dictionary<Type, FieldInfo[]> FieldsCache = new();
    private static readonly Dictionary<Type, PropertyInfo[]> PropertiesCache = new();

    public static void InitializeReady(this Node node)
    {
        Type type = node.GetType();
        
        FieldInfo[] fields;
        if (FieldsCache.TryGetValue(type, out FieldInfo[] cachedFields))
        {
            fields = cachedFields;
        }
        else
        {
            FieldsCache.Add(type, type.GetFields());
            fields = FieldsCache[type];
        }

        foreach (FieldInfo field in fields)
        {
            ReadyAttribute attribute = field.GetCustomAttribute<ReadyAttribute>();

            if (attribute == null)
                continue;
            
            if (!field.IsPublic)
            {
                Logger.Error($"CSharpReady in {type.Name}", $"Can only set nodes on public fields");
                continue;
            }
            
            string path = attribute.Path ?? "%" + char.ToUpper(field.Name[0]) + field.Name[1..];

            Node child = node.GetNodeOrNull(path);

            if (child == null)
            {
                Logger.Error($"CSharpReady in {type.Name}", $"Node '{path}' not found relative to '{node.GetPath()}'");
            }
            else
            {
                field.SetValue(node, child);
            }
        }
        
        PropertyInfo[] properties;
        if (PropertiesCache.TryGetValue(type, out PropertyInfo[] cachedProperties))
        {
            properties = cachedProperties;
        }
        else
        {
            PropertiesCache.Add(type, type.GetProperties());
            properties = PropertiesCache[type];
        }
        
        foreach (PropertyInfo property in properties)
        {
            ReadyAttribute attribute = property.GetCustomAttribute<ReadyAttribute>();
            
            if (attribute == null)
                continue;
            
            if (!property.CanWrite)
            {
                Logger.Error($"CSharpReady in {type.Name}", $"Can only set nodes on public properties");
                continue;
            }
            
            string path = attribute.Path ?? "%" + char.ToUpper(property.Name[0]) + property.Name[1..];

            Node child = node.GetNodeOrNull(path);

            if (child == null)
            {
                Logger.Error($"CSharpReady in {type.Name}", $"Node '{path}' not found relative to '{node.GetPath()}'");
            }
            else
            {
                property.SetValue(node, child);
            }
        }
    }
}