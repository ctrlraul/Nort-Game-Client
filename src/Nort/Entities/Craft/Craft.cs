using System;
using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot;
using CtrlRaul.Godot.Linq;
using Godot;
using Nort.Skills;

namespace Nort.Entities;

public abstract partial class Craft : Entity
{
    public event Action StatsChanged;
    public event Action FactionChanged;
    
    [Connectable] public event Action<Craft> Destroyed;

    [Export] private PackedScene craftBodyPartScene;

    [Ready] public Node2D partsContainer;
    [Ready] public Node2D skillsContainer;

    public bool IsDestroyed { get; private set; }
    private CraftPart corePart;

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
        
        foreach (CraftPart part in GetParts())
            part.Faction = Faction;
        
        FactionChanged?.Invoke();
    }
    
    
    [Connectable]
    public void Destroy()
    {
        Hull = 0;
        Core = 0;

        foreach (CraftPart part in GetParts())
            part.Destroy();

        QueueFree();
        IsDestroyed = true;
        Destroyed?.Invoke(this);
    }

    
    private CraftPart AddPart(BlueprintPart blueprintPart)
    {
        CraftPart part = craftBodyPartScene.Instantiate<CraftPart>();

        part.Craft = this; // I don't like this
        part.Faction = Faction;
        part.Blueprint = blueprintPart;

        if (!Game.Instance.InMissionEditor)
        {
            part.Destroyed += OnPartDestroyed;
            //part.HitTaken += (from, damage) => TakeHit(part, from, damage);
        }
        
        partsContainer.AddChild(part);

        foreach (Node skill in part.skillNodes.Cast<Node>())
            skillsContainer.AddChild(skill);

        return part;
    }
    
    private IEnumerable<CraftPart> GetParts()
    {
        return partsContainer.GetChildren().Cast<CraftPart>();
    }

    public IEnumerable<ISkillNode> GetSkillNodes()
    {
        return skillsContainer.GetChildren().Cast<ISkillNode>();
    }

    public void TakeHit(CraftPart part, ISkillNode from, float damage)
    {
        switch (from)
        {
            case BulletSkillNode:
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
                break;
        }
    }

    private void OnPartDestroyed(CraftPart part)
    {
        
    }
}