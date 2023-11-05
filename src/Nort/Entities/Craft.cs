using System;
using System.Collections.Generic;
using Godot;
using Nort.Entities.Components;
using Nort.Interface;

namespace Nort.Entities;

public partial class Craft : Entity, IFactionMember
{
    public event Action Destroyed;
    
    [Export] public PackedScene craftBodyComponentScene;
    [Export] private PackedScene flightComponentScene;
    [Export] private PackedScene playerControlsComponentScene;
    [Export] private PackedScene tractorComponentScene;
    [Export] private PackedScene tractorTargetComponentScene;
    [Export] private PackedScene droneAIComponentScene;
    [Export] private PackedScene statsDisplayComponentScene;
    
    public enum ComponentSet {
        None,
        Player,
        Fighter,
        Drone,
        Turret,
        Carrier,
        Outpost
    }

    private PackedScene[] GetComponentsForSet(ComponentSet set)
    {
        return set switch
        {
            ComponentSet.None => Array.Empty<PackedScene>(),
            ComponentSet.Player => new[]
            {
                craftBodyComponentScene,
                flightComponentScene,
                tractorComponentScene,
                playerControlsComponentScene,
            },
            ComponentSet.Fighter => new[]
            {
                craftBodyComponentScene,
                flightComponentScene,
                statsDisplayComponentScene,
            },
            ComponentSet.Drone => new[]
            {
                craftBodyComponentScene,
                flightComponentScene,
                tractorTargetComponentScene,
            },
            ComponentSet.Turret => new[]
            {
                craftBodyComponentScene,
                tractorTargetComponentScene,
            },
            ComponentSet.Carrier => new[]
            {
                craftBodyComponentScene,
                statsDisplayComponentScene,
            },
            ComponentSet.Outpost => new[]
            {
                craftBodyComponentScene,
                statsDisplayComponentScene,
            },
            _ => throw new ArgumentOutOfRangeException(nameof(set), set, null)
        };
    }


    public Faction Faction { get; private set; }
    public CraftBodyComponent Body { get; private set; }

    private Blueprint _blueprint;
    public Blueprint Blueprint { get; private set; }


    public float coreMax;
    public float core;
    public float hullMax;
    public float hull;


    public override void _Ready()
    {
        base._Ready();
        
        BlueprintStats stats = Blueprint.GetStats(_blueprint);
        
        coreMax = stats.core;
        hullMax = stats.hull;

        Body = GetComponentOrThrow<CraftBodyComponent>();
        Body.SetBlueprint(_blueprint);
    }

    public void TakeHit(CraftBodyPartSkill sourceSkill, CraftBodyPart part)
    {
        if (sourceSkill is CraftBodyPartBulletSkill bulletSkill)
        {
            hull -= bulletSkill.damage;

            if (hull >= 0)
                return;
            
            if (part == Body.Core)
            {
                core += hull;

                if (core <= 0)
                    Destroy();
            }
            else
            {
                part.TakeDamage(hull * -1);
            }

            hull = 0;
        }
    }


    private void Destroy()
    {
        hull = 0;
        core = 0;

        foreach (CraftBodyPart part in Body.GetParts())
            part.Destroy();
        
        QueueFree();
        Destroyed?.Invoke();
    }


    public Craft FromSetup(CraftSetup setup)
    {
        Craft craft = new();

        craft.Position = setup.Place;
        craft.Faction = setup.Faction;
        craft.Blueprint = setup.Blueprint;

        foreach (PackedScene componentScene in GetComponentsForSet(setup.componentSet))
        {
            EntityComponent component = componentScene.Instantiate<EntityComponent>();
            craft.AddComponent(component);
        }

        return craft;
    }
}