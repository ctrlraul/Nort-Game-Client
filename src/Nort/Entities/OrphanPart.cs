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
            Texture2D texture = Assets.Instance.GetPartTexture(value);
            partId = value;
            partSprite.Texture = texture;
            collisionShape2D.Shape = new RectangleShape2D { Size = texture.GetSize() };
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
        get => partSprite.FlipH;
        set => partSprite.FlipH = value;
    }
    
    [Savable, Inspect]
    public bool Shiny
    {
        get => ((ShaderMaterial)partSprite.Material).GetShaderParameter("shiny").AsBool();
        set => ((ShaderMaterial)partSprite.Material).SetShaderParameter("shiny", value);
    }

    public IEnumerable<string> PartIdOptions => Assets.Instance.GetParts().Select(p => p.id);
    public IEnumerable<string> SkillIdOptions => Assets.Instance.GetSkills().Select(s => s.id);

    [Ready] public Sprite2D partSprite;
    [Ready] public Sprite2D skillSprite;
    [Ready] public CollisionShape2D collisionShape2D;
    [Ready] public AnimationPlayer animationPlayer;

    private ShaderMaterial material;

    private string partId;
    private string skillId;

    public bool AppearOnRadar { get; private set; }

    private bool collectable = true;

    public bool Collectable
    {
        get => collectable;
        set
        {
            collectable = value;

            if (Game.Instance.InMissionEditor)
                return;

            SceneTreeTimer timer = GetTree().CreateTimer(0.5);

            if (collectable)
            {
                timer.Timeout += () => AppearOnRadar = true;
            }
            else
            {
                timer.Timeout += () => animationPlayer.Play("dissolve");
            }
        }
    }

    
    public override async void _Ready()
    {
        base._Ready();
        this.InitializeReady();

        material = partSprite.Material as ShaderMaterial;

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

    
    public void SetColor(Color value)
    {
        partSprite.SelfModulate = value;
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