using System;
using Godot;
using Nort.Entities.Components;
using Nort.Interface;

namespace Nort.Entities;

public partial class Craft : Entity, IFactionMember
{
    public event Action Destroyed;
    
    public enum ComponentSet {
        None,
        Player,
        Fighter,
        Drone,
        Turret,
        Carrier,
        Outpost
    }

    public Faction Faction { get; private set; }
    public CraftBodyComponent Body { get; private set; }

    private Blueprint blueprint;
    public Blueprint Blueprint
    {
        get => blueprint;
        private set
        {
            BlueprintStats stats = Blueprint.GetStats(value);
            CoreMax = stats.core;
            HullMax = stats.hull;
            blueprint = value;
        }
    }

    public float CoreMax { get; private set; }
    public float Core { get; private set; }
    public float HullMax { get; private set; }
    public float Hull { get; private set; }


    public void SetSetup(CraftSetup setup)
    {
        Position = setup.Place;
        Faction = setup.Faction;
        Blueprint = setup.Blueprint;

        AddComponents(setup.componentSet);
        InitComponents();

        Body = GetComponentOrThrow<CraftBodyComponent>();
        Body.SetBlueprint(Blueprint);
    }

    public void SetSetup(PlayerCraftSetup setup)
    {
        Position = setup.Place;
        Faction = Assets.Instance.PlayerFaction;
        Blueprint = Game.Instance.CurrentPlayer?.blueprint ?? setup.testBlueprint;
        
        AddComponents(ComponentSet.Player);
        InitComponents();

        Body = GetComponentOrThrow<CraftBodyComponent>();
        Body.SetBlueprint(Blueprint);
    }
    

    private void AddComponents(ComponentSet set)
    {
        AddComponent(EntityComponent.Create<CraftBodyComponent>());
        
        switch (set)
        {
            case ComponentSet.None:
                return;
            
            case ComponentSet.Player:
                AddComponent(EntityComponent.Create<PlayerControlsComponent>());
                AddComponent(EntityComponent.Create<FlightComponent>());
                AddComponent(EntityComponent.Create<TractorComponent>());
                break;
            
            case ComponentSet.Fighter:
                AddComponent(EntityComponent.Create<FlightComponent>());
                AddComponent(EntityComponent.Create<TractorComponent>());
                AddComponent(EntityComponent.Create<StatsDisplayComponent>());
                break;
            
            case ComponentSet.Drone:
                AddComponent(EntityComponent.Create<FlightComponent>());
                AddComponent(EntityComponent.Create<TractorTargetComponent>());
                break;
            
            case ComponentSet.Turret:
                AddComponent(EntityComponent.Create<TractorTargetComponent>());
                break;
            
            case ComponentSet.Carrier:
                AddComponent(EntityComponent.Create<StatsDisplayComponent>());
                break;
            
            case ComponentSet.Outpost:
                AddComponent(EntityComponent.Create<StatsDisplayComponent>());
                break;
            
            default:
                throw new NotImplementedException($"Component set not implemented for set '{set}'");
        }
    }

    public void TakeHit(CraftBodyPartSkill sourceSkill, CraftBodyPart part)
    {
        if (sourceSkill is CraftBodyPartBulletSkill bulletSkill)
        {
            Hull -= bulletSkill.damage;

            if (Hull >= 0)
                return;
            
            if (part == Body.Core)
            {
                Core += Hull;

                if (Core <= 0)
                    Destroy();
            }
            else
            {
                part.TakeDamage(Hull * -1);
            }

            Hull = 0;
        }
    }


    public void Destroy()
    {
        Hull = 0;
        Core = 0;

        foreach (CraftBodyPart part in Body.GetParts())
            part.Destroy();
        
        QueueFree();
        Destroyed?.Invoke();
    }
}