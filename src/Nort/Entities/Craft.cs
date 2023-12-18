using System;
using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot;
using CtrlRaul.Godot.Linq;
using Godot;
using Nort.Entities.Components;
using Nort.Skills;

namespace Nort.Entities;

public partial class Craft : Entity
{
    public event Action StatsChanged;
    public event Action FactionChanged;
    public event Action Destroyed;

    [Export] private PackedScene craftBodyPartScene;

    [Ready] public Area2D partsContainer;
    [Ready] public Node2D skillsContainer;

    private CraftBodyPart corePart;

    protected Faction faction = Assets.Instance.DefaultEnemyFaction;

    public Faction Faction
    {
        get => faction;
        set => SetFaction(value);
    }

    protected Blueprint blueprint = Assets.Instance.InitialBlueprint;

    public Blueprint Blueprint
    {
        get => blueprint;
        set => SetBlueprint(value);
    }

    protected Rect2 blueprintVisualRect;

    public float ArtificialRadius => blueprintVisualRect.Size.Length();


    public float CoreMax { get; private set; }
    public float Core { get; private set; }
    public float HullMax { get; private set; }
    public float Hull { get; private set; }


    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();

        SetBlueprint(Blueprint);
        SetFaction(Faction);
    }


    protected virtual void SetBlueprint(Blueprint value)
    {
        blueprint = value;

        if (!IsInsideTree())
            return;
        
        blueprintVisualRect = Assets.Instance.GetBlueprintVisualRect(Blueprint);

        
        
        skillsContainer.QueueFreeChildren();
        partsContainer.QueueFreeChildren();
        
        foreach (BlueprintPart blueprintPart in blueprint.hulls)
            AddPart(blueprintPart);

        corePart = AddPart(blueprint.core);

        BlueprintStats stats = Blueprint.GetStats(blueprint);
        CoreMax = stats.core;
        HullMax = stats.hull;
        Core = CoreMax;
        Hull = HullMax;
        
        StatsChanged?.Invoke();
    }
    
    protected virtual void SetFaction(Faction value)
    {
        faction = value;

        if (!IsInsideTree())
            return;
        
        foreach (CraftBodyPart part in GetParts())
            part.Faction = Faction;
        
        partsContainer.CollisionLayer = Assets.Instance.GetFactionCollisionLayer(faction);
        partsContainer.CollisionLayer |= PhysicsLayer.Get("craft");
        
        FactionChanged?.Invoke();
    }


    public void Destroy()
    {
        Hull = 0;
        Core = 0;

        foreach (CraftBodyPart part in GetParts())
            part.Destroy();

        QueueFree();
        Destroyed?.Invoke();
    }

    
    private CraftBodyPart AddPart(BlueprintPart blueprintPart)
    {
        CraftBodyPart part = craftBodyPartScene.Instantiate<CraftBodyPart>();

        part.Faction = Faction;
        part.Blueprint = blueprintPart;

        if (!Game.Instance.InMissionEditor)
        {
            part.Destroyed += () => OnPartDestroyed(part);
            part.HitTaken += (from, damage) => OnPartTookHit(part, from, damage);
        }
        
        partsContainer.AddChild(part);

        foreach (SkillNode skill in part.skillNodes)
            skillsContainer.AddChild(skill);

        return part;
    }
    
    private IEnumerable<CraftBodyPart> GetParts()
    {
        return partsContainer.GetChildren().Cast<CraftBodyPart>();
    }
    

    private void OnPartTookHit(CraftBodyPart part, SkillNode from, float damage)
    {
        if (from is BulletSkillNode)
        {
            Hull -= damage;

            if (Hull >= 0)
                return;

            if (part == corePart)
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
            
            StatsChanged?.Invoke();
        }
    }

    private void OnPartDestroyed(CraftBodyPart part)
    {
        GD.Print("Move part drop code in here!");
    }
}