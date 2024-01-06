using System;
using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot;
using CtrlRaul.Godot.Linq;
using Godot;
using Nort.Skills;

namespace Nort.Entities;

public partial class Craft : Entity
{
    public event Action StatsChanged;
    public event Action FactionChanged;
    
    [Connectable] public event Action Destroyed;

    [Export] private PackedScene craftPartScene;

    [Ready] public Node2D partsContainer;

    public bool IsDestroyed { get; private set; }
    public CraftPart CorePart { get; private set; }

    public readonly List<ISkillNode> skillNodes = new();

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


    public BlueprintStats Stats { get; private set; }
    public float CoreMax { get; private set; }
    public float Core { get; private set; }
    public float HullMax { get; private set; }
    public float Hull { get; private set; }
    

    protected virtual void SetBlueprint(Blueprint value)
    {
        blueprint = value;

        if (!DidSpawn)
            return;
        
        blueprintVisualRect = Assets.Instance.GetBlueprintVisualRect(Blueprint);

        skillNodes.Clear();
        ClearParts();
        
        foreach (BlueprintPart blueprintPart in blueprint.hulls)
            AddPart(blueprintPart);

        CorePart = AddPart(blueprint.core);

        RecalculateStats();
    }

    public CraftPart SetCoreBlueprint(BlueprintPart blueprintPart)
    {
        if (CorePart == null)
            CorePart = AddPart(blueprintPart);
        else
            CorePart.Blueprint = blueprintPart;

        RecalculateStats();

        return CorePart;
    }
    
    protected virtual void SetFaction(Faction value)
    {
        faction = value;

        if (!DidSpawn)
            return;
        
        foreach (CraftPart part in GetParts())
            part.Faction = Faction;

        // Hack to force the physics engine to re-calculate collisions
        // so ex-friendly crafts will detect each other as enemies.
        Position += new Vector2(0.1f, 0);
        
        FactionChanged?.Invoke();
    }

    public void ClearParts()
    {
        CorePart = null;
        partsContainer.QueueFreeChildren();
        RecalculateStats();
    }

    public Blueprint GetCurrentBlueprint()
    {
        return new()
        {
            id = Assets.GenerateUuid(),
            core = CorePart?.Blueprint,
            hulls = GetParts()
                .Where(part => part != CorePart)
                .Select(part => part.GetCurrentBlueprint())
                .ToList()
        };
    }
    
    
    [Connectable]
    public void Destroy()
    {
        Hull = 0;
        Core = 0;

        if (Assets.IsCore(CorePart.Blueprint.Part))
            Stage.Instance.AddCoreExplosionEffect(GlobalPosition);
        
        foreach (CraftPart part in GetParts())
            part.Destroy();

        QueueFree();
        IsDestroyed = true;
        Destroyed?.Invoke();
    }


    public CraftPart AddPart(BlueprintPart blueprintPart)
    {
        CraftPart part = craftPartScene.Instantiate<CraftPart>();

        part.Craft = this; // I don't like this
        part.Faction = Faction;
        part.Blueprint = blueprintPart;

        if (!Game.Instance.InMissionEditor)
        {
            part.Destroyed += OnPartDestroyed;
            //part.HitTaken += (from, damage) => TakeHit(part, from, damage);
        }
        
        partsContainer.AddChild(part);

        if (part.skillNode != null)
            skillNodes.Add(part.skillNode);

        RecalculateStats();
        
        return part;
    }
    
    private IEnumerable<CraftPart> GetParts()
    {
        return partsContainer.GetChildren().Cast<CraftPart>();
    }

    public void TakeHit(CraftPart part, ISkillNode from, float damage)
    {
        Hull -= damage;

        if (Hull >= 0)
            return;

        if (part == CorePart)
        {
            Core += Hull;

            CorePart.SetColorScale(Core / CoreMax);

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

    private void RecalculateStats()
    {
        Stats = Blueprint.GetStats(GetCurrentBlueprint());

        CoreMax = Stats.core;
        HullMax = Stats.hull;
        Core = CoreMax;
        Hull = HullMax;

        StatsChanged?.Invoke();
    }

    public void MovePartToTop(CraftPart part)
    {
        if (partsContainer.GetChildCount() > 1)
            partsContainer.MoveChild(part, -1);
    }


    protected override void OnSpawning()
    {
        SetFaction(faction);
        SetBlueprint(blueprint);

        foreach (CraftPart part in GetParts())
            part.AnimateSpawn();
    }

    private void OnPartDestroyed(CraftPart part)
    {
        
    }
}