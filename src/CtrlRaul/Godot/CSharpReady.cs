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
            if (!field.IsPublic)
                throw new Exception("Can only initialize public fields with Ready");
            ReadyAttribute attribute = field.GetCustomAttribute<ReadyAttribute>();
            if (attribute != null)
                field.SetValue(node, node.GetNode(attribute.Path));
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
            if (attribute != null)
                property.SetValue(node, node.GetNode(attribute.Path));
        }
    }
}