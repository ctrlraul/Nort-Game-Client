using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot;
using Godot;
using Nort.Hud;
using Nort.Pages;

namespace Nort.Entities;

public partial class OrphanPart : Entity
{
    [Savable]
    [Inspect(nameof(PartIdOptions))]
    public string PartId
    {
        get => Part.id;
        set => SetPartId(value);
    }
    
    [Savable]
    [Inspect(nameof(SkillIdOptions))]
    public string SkillId
    {
        get => Skill?.id;
        set => SetSkillId(value);
    }
    
    [Savable, Inspect]
    public bool Flipped
    {
        get => flipped;
        set => SetFlipped(value);
    }
    
    [Savable, Inspect]
    public bool Shiny
    {
        get => shiny;
        set => SetShiny(value);
    }

    public IEnumerable<string> PartIdOptions => Assets.Instance.GetParts().Select(p => p.id);
    public IEnumerable<string> SkillIdOptions => Assets.Instance.GetSkills().Select(s => s.id);
    
    [Ready] public Sprite2D sprite2D;
    [Ready] public Sprite2D skillSprite;
    [Ready] public CollisionShape2D editorHitBoxShape;
    [Ready] public Node2D editorStuff;
    [Ready] public AnimationPlayer animationPlayer;

    private bool flipped;
    private bool shiny;
    public Part Part { get; private set; }
    public Skill Skill { get; private set; }


    public OrphanPart()
    {
        PartId = Config.InitialPart;
    }
    
    
    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();

        Initialize();

        if (Game.Instance.InMissionEditor)
        {
            editorHitBoxShape.Disabled = false;
        }
        else
        {
            editorStuff.QueueFree();
        }
    }


    private async void Initialize()
    {
        await Game.Instance.Initialize();
        
        SetPartId(PartId);
        SetSkillId(SkillId);
        SetFlipped(flipped);
        SetShiny(shiny);
        
        sprite2D.SelfModulate = Config.FactionlessColor;
        skillSprite.GlobalRotation = 0;

        animationPlayer.SpeedScale = (0.05f + 0.1f * GD.Randf()) * (GD.Randf() > 0.5f ? 1 : -1);
        animationPlayer.Play("rotate");
        animationPlayer.Seek(GD.Randf());
    }

    private void SetPartId(string value)
    {
        GD.Print($"SetPartId: {value}");
        
        Part = Assets.Instance.GetPart(value);

        if (IsInsideTree())
            sprite2D.Texture = Assets.Instance.GetPartTexture(Part);
    }

    private void SetSkillId(string value)
    {
        Skill = string.IsNullOrEmpty(value) ? null : Assets.Instance.GetSkill(value);

        if (IsInsideTree())
            skillSprite.Texture = Skill == null ? null : Assets.Instance.GetSkillTexture(Skill.id);
    }

    private void SetFlipped(bool value)
    {
        flipped = value;

        if (IsInsideTree())
            sprite2D.FlipH = value;
    }

    private void SetShiny(bool value)
    {
        shiny = value;

        if (IsInsideTree())
            sprite2D.Material = value ? Assets.ShinyMaterial : null;
    }
}