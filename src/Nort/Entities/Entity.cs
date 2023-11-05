using System;
using System.Collections.Generic;
using Godot;
using Nort.Entities.Components;

namespace Nort.Entities;

public partial class Entity : Node2D
{
    private readonly Dictionary<Type, EntityComponent> components = new();

    protected void AddComponent<T>(T component) where T : EntityComponent
    {
        components.Add(typeof(T), component);
        AddChild(component);
    }

    public T GetComponent<T>() where T : EntityComponent
    {
        return components.TryGetValue(typeof(T), out EntityComponent component)
            ? component as T
            : null;
    }

    public T GetComponentOrThrow<T>() where T : EntityComponent
    {
        return components.TryGetValue(typeof(T), out EntityComponent component)
            ? component as T
            : throw new Exception($"{GetType().Name} was expected to have a {typeof(T).Name} component");
    }
}
