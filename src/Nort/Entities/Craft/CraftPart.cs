using System;
using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot;
using Godot;
using CtrlRaul.Godot.Linq;

namespace Nort.Entities;

// Context:
// A 2D spaceship fight game where you can build your own ship with multiple parts, parts can individually break off in battle. Ships have an Area2D node, and each part is a CollisionShape2D node.
//
//     Weapons keep track of all enemy parts within range using `CollisionObject`'s `area_shape_entered` and `area_shape_exited` signals, using the `area_shape_index` argument to keep track of which part is entering or leaving the range.
//
// The issue:
// Because `area_shape_exited` also triggers when shapes are being removed from the tree (reasonably so), the `area_shape_index` argument turns out to be completely unreliable as when parts are destroyed (removed from the tree), the indexes shift.


public partial class CraftPart : CollisionShape2D
{
    //public event Action<SkillNode, float> HitTaken;
    public event Action<CraftPart> Destroyed;

    [Ready] public Sprite2D sprite2D;
    
    public readonly List<ISkillNode> skillNodes = new();
    public float hullMax;
    public float hull;

    public bool IsDestroyed { get; private set; }
    public Craft Craft { get; set; }
    
    
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


    private void SetCraft(Craft craft)
    {
        Craft = craft;
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
            Node2D node = skill.Scene.Instantiate<Node2D>();
            
            node.Position = Position;

            if (node is ISkillNode skillNode)
            {
                skillNode.Part = this;
                skillNodes.Add(skillNode);
            }
            
        }

        if (faction != null) // Actually not sure why this check is needed
            UpdateColor();
    }


    // public void TakeHit(SkillNode from, float damage)
    // {
    //     HitTaken?.Invoke(from, damage);
    // }
    
    public void TakeDamage(float damage)
    {
        hull -= damage;
        
        UpdateColor();

        if (hull > 0)
            return;
        
        if (GD.Randf() <= GetDropRate())
            Drop();

        Destroy();


        foreach (Node skill in skillNodes.Cast<Node>())
            skill.Remove();
    }

    private void Drop()
    {
        OrphanPart orphanPart = Stage.Instance.Spawn<OrphanPart>();

        orphanPart.Position = GlobalPosition;
        orphanPart.Rotation = GlobalRotation;
        orphanPart.PartId = blueprint.partId;
        orphanPart.SkillId = blueprint.skillId;
        orphanPart.Flipped = blueprint.flipped;
        orphanPart.Shiny = blueprint.shiny || ShinyDropRoll();
    }

    public void Destroy()
    {
        IsDestroyed = true;
        Disabled = true;
        Modulate *= new Color(1, 1, 1, 0.5f);
        Destroyed?.Invoke(this);
    }

    private void UpdateColor()
    {
        if (hullMax > 0)
        {
            float weight = Mathf.Max(0, hull) / hullMax;
            sprite2D.SelfModulate = Config.FactionlessColor.Lerp(Faction.Color, weight);
        }
        else
        {
            sprite2D.SelfModulate = Config.FactionlessColor;
        }
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