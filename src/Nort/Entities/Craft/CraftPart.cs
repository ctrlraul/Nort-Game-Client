﻿using System;
using CtrlRaul.Godot;
using Godot;
using Nort.Skills;

namespace Nort.Entities;

public partial class CraftPart : Area2D
{
    public event Action<CraftPart> Destroyed;
    
    [Ready] public Sprite2D sprite2D;
    [Ready] public CollisionShape2D collisionShape2D;
    [Ready] public Node2D skillNodeContainer;
    [Ready] public AnimationPlayer animationPlayer;

    public ISkillNode skillNode;
    public float hullMax;
    public float hull;

    public bool IsDestroyed { get; private set; }
    public Craft Craft { get; set; }
    
    private Faction faction = Assets.Instance.DefaultEnemyFaction;
    public Faction Faction
    {
        get => faction;
        set
        {
            faction = value;

            if (!IsInsideTree())
                return;

            CollisionLayer = Assets.Instance.GetFactionCollisionLayer(faction);
            CollisionLayer |= PhysicsLayer.Get("craft_part");

            UpdateColor();
        }
    }

    private BlueprintPart blueprint = Assets.Instance.InitialBlueprint.core;

    public BlueprintPart Blueprint
    {
        get => blueprint;
        set
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
            ((ShaderMaterial)sprite2D.Material).SetShaderParameter("shiny", blueprint.shiny);

            collisionShape2D.Shape = new RectangleShape2D { Size = sprite2D.Texture.GetSize() };

            if (Assets.IsCore(blueprint.Part))
                skillNodeContainer.AddChild(new Sprite2D { Texture = Assets.CoreLightTexture });

            if (!string.IsNullOrEmpty(blueprint.skillId))
            {
                Node2D node = blueprint.Skill.Scene.Instantiate<Node2D>();
                skillNode = (ISkillNode)node;
                skillNode.Part = this;
                skillNodeContainer.AddChild(node);
            }

            UpdateColor();
        }
    }

    public bool Flipped
    {
        get => sprite2D.FlipH;
        set => sprite2D.FlipH = value;
    }


    public override void _Ready()
    {
        this.InitializeReady();

        Faction = Faction;
        Blueprint = blueprint;
    }


    private void SetCraft(Craft craft)
    {
        Craft = craft;
    }
    
    public void TakeDamage(float damage)
    {
        hull -= damage;
        
        UpdateColor();

        if (hull > 0)
            return;

        Destroy();
    }

    public BlueprintPart GetCurrentBlueprint()
    {
        return new()
        {
            angle = RotationDegrees,
            flipped = blueprint.flipped,
            shiny = blueprint.shiny,
            partId = blueprint.partId,
            skillId = blueprint.skillId,
            Place = Position,
        };
    }

    private void Drop()
    {
        OrphanPart orphanPart = Stage.Instance.AddEntity<OrphanPart>();

        orphanPart.Position = GlobalPosition;
        orphanPart.Rotation = GlobalRotation;
        orphanPart.PartId = blueprint.partId;
        orphanPart.SkillId = blueprint.skillId;
        orphanPart.Flipped = blueprint.flipped;
        orphanPart.Shiny = blueprint.shiny || ShinyDropRoll();
        orphanPart.Velocity = Craft.Velocity + Position.Normalized() * (3 + GD.Randf() * 3);
        orphanPart.Collectable = Craft is not PlayerCraft && GD.Randf() < GetDropRate();
        orphanPart.SetColor(sprite2D.SelfModulate);
    }

    public void Destroy()
    {
        if (IsDestroyed)
            throw new Exception("Already destroyed");
        
        IsDestroyed = true;
        
        CallDeferred(nameof(Drop));
        
        QueueFree();

        //AudioManager.Instance.PlayPartDetached(GlobalPosition);
        
        Destroyed?.Invoke(this);
    }

    public void SetColorScale(float scale)
    {
        Color color = Assets.Instance.GetFactionColor(Faction, blueprint.shiny);
        sprite2D.SelfModulate = Config.FactionlessColor.Lerp(color, scale);
    }

    public void AnimateSpawn()
    {
        animationPlayer.Play("pre_spawn");
        IntervalTweener tween = CreateTween().TweenInterval(Position.Length() * 0.005f);
        tween.Finished += () => animationPlayer.Play("spawn");
    }
    

    private void UpdateColor()
    {
        if (hullMax > 0)
        {
            SetColorScale(Mathf.Max(0, hull) / hullMax);
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