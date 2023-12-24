using System;
using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot;
using Godot;

namespace Nort.Entities;

public partial class CraftPart : Area2D
{
    public event Action<CraftPart> Destroyed;

    [Ready] public Sprite2D sprite2D;
    [Ready] public CollisionShape2D collisionShape2D;
    
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

        if (!IsInsideTree())
            return;
        
        CollisionLayer = Assets.Instance.GetFactionCollisionLayer(faction);
        CollisionLayer |= PhysicsLayer.Get("craft_part");
        
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
        
        collisionShape2D.Shape = new RectangleShape2D { Size = sprite2D.Texture.GetSize() };

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

    
    public void TakeDamage(float damage)
    {
        hull -= damage;
        
        UpdateColor();

        if (hull > 0)
            return;

        Destroy();
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
        orphanPart.Velocity = Craft.Velocity + Position.Normalized() * (3 + GD.Randf() * 3);
        orphanPart.SetColor(sprite2D.SelfModulate);
        orphanPart.BrokenOff(GD.Randf() < GetDropRate());
    }

    public void Destroy()
    {
        IsDestroyed = true;
        
        CallDeferred(nameof(Drop));
        
        foreach (Node skillNode in skillNodes.Cast<Node>())
            skillNode.QueueFree();
        
        QueueFree();
        
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