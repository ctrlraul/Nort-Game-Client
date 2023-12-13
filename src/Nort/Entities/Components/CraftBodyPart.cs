using System;
using System.Collections.Generic;
using CtrlRaul.Godot;
using Godot;
using CtrlRaul.Godot.Linq;

namespace Nort.Entities.Components;

public partial class CraftBodyPart : CollisionShape2D
{
    public event Action<SkillNode, float> HitTaken;
    
    private static readonly Color SemiTransparent = new(1, 1, 1, 0.2f);
    public event Action Destroyed;

    [Ready] public Sprite2D sprite2D;
    
    public List<SkillNode> skillNodes = new();
    public float hullMax;
    public float hull;

    public bool IsDestroyed { get; private set; }

    
    private Faction faction = Assets.Instance.DefaultEnemyFaction;
    public Faction Faction
    {
        get => faction;
        set => SetFaction(value);
    }

    private BlueprintPart blueprint = Assets.Instance.InitialBlueprint.core;
    public BlueprintPart Blueprint
    {
        get => blueprint;
        set => SetBlueprint(value);
    }
    
    
    public override void _Ready()
    {
        this.InitializeReady();
        
        //SetFaction(Faction);
        SetBlueprint(Blueprint);
    }


    private void SetFaction(Faction value)
    {
        faction = value;
        
        if (IsInsideTree())
            UpdateColor();
    }

    private void SetBlueprint(BlueprintPart value)
    {
        blueprint = value;

        if (!IsInsideTree())
            return;

        Position = blueprint.Place;
        RotationDegrees = blueprint.angle;

        hullMax = blueprint.Part.hull;
        hull = hullMax;

        sprite2D.Texture = Assets.Instance.GetPartTexture(blueprint);
        sprite2D.FlipH = blueprint.flipped;
        sprite2D.Material = blueprint.shiny ? Assets.ShinyMaterial : null;
        
        Shape = new RectangleShape2D { Size = sprite2D.Texture.GetSize() };

        List<Skill> skills = new();
        
        if (Assets.IsCore(blueprint.Part))
        {
            skills.Add(Assets.Instance.DefaultCoreSkill);
            Sprite2D coreLight = new();
            coreLight.Texture = Assets.CoreLightTexture;
            AddChild(coreLight);
        }

        if (!string.IsNullOrEmpty(blueprint.skillId))
            skills.Add(blueprint.Skill);
        
        foreach (Skill skill in skills)
        {
            SkillNode cbpSkill = skill.Scene.Instantiate<SkillNode>();
            cbpSkill.Position = Position;
            cbpSkill.part = this;
            skillNodes.Add(cbpSkill);
        }

        if (faction != null) // Actually not sure why this check is needed
            UpdateColor();
    }


    public void TakeHit(SkillNode from, float damage)
    {
        HitTaken?.Invoke(from, damage);
    }
    
    public void TakeDamage(float damage)
    {
        hull -= damage;
        
        UpdateColor();

        if (hull > 0)
            return;
        
        if (GD.Randf() <= GetDropRate())
            Drop();

        Destroy();
            
        this.Remove();

        foreach (SkillNode skill in skillNodes)
            skill.Remove();
    }

    private void Drop()
    {
        OrphanPart orphanPart = Stage.Instance.SpawnOrphanPart();

        orphanPart.Position = GlobalPosition;
        orphanPart.Rotation = GlobalRotation;
        // orphanPart.PartId = blueprint.partId;
        // orphanPart.Flipped = blueprint.flipped;
        // orphanPart.Shiny = blueprint.shiny || ShinyDropRoll();
    }

    public void Destroy()
    {
        IsDestroyed = true;
        Destroyed?.Invoke();
    }

    private void UpdateColor()
    {
        Color color = Config.FactionlessColor;
        
        if (hullMax > 0)
        {
            float weight = Mathf.Max(0, hull) / hullMax;
            color = color.Lerp(Faction.Color, weight);
        }
        
        sprite2D.SelfModulate = color;
        DebugColor = color * SemiTransparent;
    }
    
    private float GetDropRate()
    {
        return Part.IsCore(blueprint.Part) ? Config.DropRateCore : Config.DropRateHull;
    }

    private bool ShinyDropRoll()
    {
        return GD.Randf() <= Config.DropRateShiny;
    }
}