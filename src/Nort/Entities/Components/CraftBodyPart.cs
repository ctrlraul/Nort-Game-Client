using System;
using System.Collections.Generic;
using Godot;
using CtrlRaul.Godot.Linq;

namespace Nort.Entities.Components;

public partial class CraftBodyPart : Node2D
{
    private static readonly Color SemiTransparent = new(1, 1, 1, 0.2f);
    public event Action Destroyed;

    private CollisionShape2D hitboxCollisionShape;
    private Sprite2D sprite;

    public bool IsDestroyed { get; private set; }
    
    private BlueprintPart _blueprint;
    public CraftBodyComponent body;
    public readonly List<CraftBodyPartSkill> skills = new();
    public float hullMax;
    public float hull;
    
    public override void _Ready()
    {
        hitboxCollisionShape = GetNode<CollisionShape2D>("%CollisionShape2D");
        sprite = GetNode<Sprite2D>("%Sprite2D");
    }

    public void SetBlueprint(BlueprintPart blueprint)
    {
        _blueprint = blueprint;

        Position = blueprint.Place;
        Rotation = blueprint.angle;

        hullMax = blueprint.Part.hull;
        hull = hullMax;

        sprite.Texture = Assets.Instance.GetPartTexture(blueprint);
        sprite.FlipH = blueprint.flipped;
        sprite.Material = blueprint.shiny ? Assets.SHINY_MATERIAL : null;

        RectangleShape2D rectangleShape2D = new();
        rectangleShape2D.Size = sprite.Texture.GetSize();
        hitboxCollisionShape.Shape = rectangleShape2D;

        // List<Skill> skillzz = new();
        
        if (Assets.IsCore(blueprint.Part))
        {
            // skillzz.Add(Assets.Instance.DefaultCoreSkill);
            Sprite2D coreLight = new();
            coreLight.Texture = Assets.CORE_LIGHT_TEXTURE;
            AddChild(coreLight);
        }

        // if (!string.IsNullOrEmpty(blueprint.skillId))
        //     skillzz.Add(blueprint.Skill);
        //
        // foreach (Skill skill in skillzz)
        // {
        //     CraftBodyPartSkill cbpSkill = skill.Scene.Instantiate<CraftBodyPartSkill>();
        //     cbpSkill.Position = Position;
        //     cbpSkill.part = this;
        //     skills.Add(cbpSkill);
        // }
        
        UpdateColor();
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

        foreach (CraftBodyPartSkill skill in skills)
            skill.Remove();
    }

    private void Drop()
    {
        OrphanPartSetup setup = new()
        {
            Place = GlobalPosition,
            angle = GlobalRotation,
            partId = _blueprint.partId,
            flipped = _blueprint.flipped,
            skillId = _blueprint.skillId,
            shiny = _blueprint.shiny || ShinyDropRoll()
        };

        Stage.Instance.Spawn(setup);
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
            color = color.Lerp(body.Color, weight);
        }
        
        sprite.SelfModulate = color;
        hitboxCollisionShape.DebugColor = color * SemiTransparent;
    }
    
    private float GetDropRate()
    {
        return Part.IsCore(_blueprint.Part) ? Config.DropRateCore : Config.DropRateHull;
    }

    private bool ShinyDropRoll()
    {
        return GD.Randf() <= Config.DropRateShiny;
    }
}