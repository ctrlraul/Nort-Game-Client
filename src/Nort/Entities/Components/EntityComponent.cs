using System;
using System.Collections.Generic;
using Godot;

namespace Nort.Entities.Components;

public partial class EntityComponent : Node2D
{
    //public static readonly PackedScene CRAFT_BODY = GD.Load<PackedScene>("res://Scenes/Entities/Components/CraftBodyComponent.tscn");
    //public static readonly PackedScene FLIGHT = GD.Load<PackedScene>("res://Scenes/Entities/Components/FlightComponent.tscn");
    // public static readonly PackedScene STATS_DISPLAY = GD.Load<PackedScene>("res://Scenes/Entities/Components/StatsDisplayComponent.tscn");

    private static readonly Dictionary<Type, PackedScene> Scenes = new()
    {
        { typeof(CraftBodyComponent), GD.Load<PackedScene>("res://Scenes/Entities/Components/CraftBodyComponent.tscn")},
        { typeof(TractorComponent), GD.Load<PackedScene>("res://Scenes/Entities/Components/TractorComponent.tscn")},
        { typeof(TractorTargetComponent), GD.Load<PackedScene>("res://Scenes/Entities/Components/TractorTargetComponent.tscn")},
        { typeof(PlayerControlsComponent), GD.Load<PackedScene>("res://Scenes/Entities/Components/PlayerControlsComponent.tscn")},
    };

    public Entity Entity { get; private set; }
    public Craft Craft { get; private set; }
    public OrphanPart OrphanPart { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        Entity = GetParent<Entity>();
        Craft = Entity as Craft;
        OrphanPart = Entity as OrphanPart;
    }
    
    public virtual void Init() {}

    public static T Create<T>() where T : EntityComponent, new()
    {
        PackedScene scene = GetSceneForComponentType<T>();
        return scene != null ? scene.Instantiate<T>() : new T();
    }

    private static PackedScene GetSceneForComponentType<T>() where T : EntityComponent
    {
        return Scenes.TryGetValue(typeof(T), out PackedScene scene) ? scene : null;
    }
}