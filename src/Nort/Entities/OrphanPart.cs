using System;
using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot;
using Godot;
using Nort.Hud;

namespace Nort.Entities;

public partial class OrphanPart : Entity
{
    [Connectable]
    public event Action Collected;
    
    [Connectable]
    public event Action Destroyed;
    
    
    [Savable]
    [Inspect(nameof(PartIdOptions))]
    public string PartId
    {
        get => partId;
        set
        {
            partId = value;
            sprite2D.Texture = Assets.Instance.GetPartTexture(value);
        }
    }
    
    [Savable]
    [Inspect(nameof(SkillIdOptions))]
    public string SkillId
    {
        get => skillId;
        set
        {
            skillId = value;
            skillSprite.Texture = string.IsNullOrEmpty(skillId) ? null : Assets.Instance.GetSkillTexture(skillId);
        }
    }

    
    [Savable, Inspect]
    public bool Flipped
    {
        get => sprite2D.FlipH;
        set => sprite2D.FlipH = value;
    }
    
    [Savable, Inspect]
    public bool Shiny
    {
        get => material.GetShaderParameter("shiny").AsBool();
        set => material.SetShaderParameter("shiny", value);
    }

    public IEnumerable<string> PartIdOptions => Assets.Instance.GetParts().Select(p => p.id);
    public IEnumerable<string> SkillIdOptions => Assets.Instance.GetSkills().Select(s => s.id);
    
    [Ready] public Sprite2D sprite2D;
    [Ready] public Sprite2D skillSprite;
    [Ready] public AnimationPlayer animationPlayer;

    private ShaderMaterial material;

    private string partId;
    private string skillId;

    public bool Collectable { get; private set; } = true;
    public bool AppearOnRadar { get; private set; }


    public override async void _Ready()
    {
        base._Ready();
        this.InitializeReady();

        material = sprite2D.Material as ShaderMaterial;

        if (material == null)
            throw new Exception($"Expected a {nameof(ShaderMaterial)} material on sprite");
        
        material.SetShaderParameter("dissolve_noise_offset", new Vector2(GD.Randi() % 256, GD.Randi() % 256));
        
        await Game.Instance.Initialize();
        
        PartId = Config.InitialPart;
        SkillId = null;
        Flipped = false;
        Shiny = false;
        
        skillSprite.GlobalRotation = 0;
        
        SetColor(Config.FactionlessColor);
    }


    public void BrokenOff(bool collectable)
    {
        Collectable = collectable;
        
        GetTree().CreateTimer(0.5).Timeout += () =>
        {
            if (collectable)
            {
                AppearOnRadar = true;
            }
            else
            {
                animationPlayer.Play("dissolve");
            }
        };
    }
    
    public void SetColor(Color value)
    {
        sprite2D.SelfModulate = value;
    }
    
    public PartData GetPartData()
    {
        return new()
        {
            partId = PartId,
            skillId = SkillId,
            shiny = Shiny
        };
    }
    
    
    [Connectable]
    public void Collect()
    {
        QueueFree();
        Collected?.Invoke();
    }

    [Connectable]
    public void Destroy()
    {
        QueueFree();
        Destroyed?.Invoke();
    }
}