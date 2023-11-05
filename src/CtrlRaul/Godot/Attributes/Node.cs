/*using Godot;
using System;
using System.Reflection;

namespace CtrlRaul.Godot.Attributes;


[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ChildNodeAttribute : Attribute
{
    public NodePath Path { get; }

    public ChildNodeAttribute(NodePath path)
    {
        Path = path;
    }
}


public partial class Node
{
    public Node()
    {
        foreach (PropertyInfo propertyInfo in GetType().GetProperties(BindingFlags.DeclaredOnly))
        {
            ChildNodeAttribute attribute = propertyInfo.GetCustomAttribute<ChildNodeAttribute>();
            
            if (attribute == null)
                continue;
            
            propertyInfo.SetValue(this, GetNode(attribute.Path));
        }
    }
}*/