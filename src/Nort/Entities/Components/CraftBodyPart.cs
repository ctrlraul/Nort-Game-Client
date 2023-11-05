using System;
using System.Collections.Generic;
using Godot;
using CtrlRaul.Godot.Linq;

namespace Nort.Entities.Components;

public partial class CraftBodyPart : Node2D
{
    public event Action Destroyed;

    private CollisionShape2D hitboxCollisionShape;
    private Sprite2D sprite;

    private Color _color;
    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            UpdateColor();
        }
    }
    
    public bool IsDestroyed { get; private set; }
    
    private BlueprintPart _blueprint;
    public List<CraftBodyPartSkill> skills = new();
    public CraftBodyComponent body;
    public float hullMax;
    public float hull;
    
    public override void _Ready()
    {
        hitboxCollisionShape = GetNode<CollisionShape2D>("%CollisionShape2D");
        sprite = GetNode<Sprite2D>("%Sprite2D");
        if (body == null)
            throw new Exception("Set body before adding to tree, for good measure.");
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

        List<Skill> skillzz = new();
        
        if (Assets.Instance.IsCore(blueprint))
        {
            skillzz.Add(Assets.Instance.DefaultCoreSkill);
            Sprite2D coreLight = new();
            coreLight.Texture = Assets.CORE_LIGHT_TEXTURE;
            AddChild(coreLight);
        }

        if (!string.IsNullOrEmpty(blueprint.skillId))
            skillzz.Add(blueprint.Skill);

        foreach (Skill skill in skillzz)
        {
            CraftBodyPartSkill cbpSkill = skill.Scene.Instantiate<CraftBodyPartSkill>();
            cbpSkill.Position = Position;
            cbpSkill.part = this;
            skills.Add(cbpSkill);
        }
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
        float weight = Mathf.Max(0, hull) / hullMax;
        sprite.SelfModulate = GameConfig.FactionlessColor.Lerp(_color, weight);
        hitboxCollisionShape.DebugColor = _color * new Color(1, 1, 1, 0.2f);
    }
    
    private float GetDropRate()
    {
        return Part.IsCore(_blueprint.Part) ? GameConfig.DropRateCore : GameConfig.DropRateHull;
    }

    private bool ShinyDropRoll()
    {
        return GD.Randf() <= GameConfig.DropRateShiny;
    }
}