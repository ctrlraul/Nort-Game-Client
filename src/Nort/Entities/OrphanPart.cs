using CtrlRaul.Godot;
using Godot;
using Nort.Hud;
using Nort.Pages;

namespace Nort.Entities;

public partial class OrphanPart : Entity
{
    [Savable]
    public string PartId
    {
        get => Part.id;
        set => SetPartId(value);
    }
    
    [Savable]
    public string SkillId
    {
        get => Skill?.id;
        set => SetPartId(value);
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
    
    [Ready] public Sprite2D sprite2d;
    [Ready] public Sprite2D skillSprite;
    [Ready] public AnimationPlayer animationPlayer;

    private bool flipped;
    private bool shiny;
    public Part Part { get; private set; }
    public Skill Skill { get; private set; }
    
    
    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();

        SetPartId(PartId);
        SetSkillId(SkillId);
        SetFlipped(flipped);
        SetShiny(shiny);
        
        sprite2d.SelfModulate = Config.FactionlessColor;
        skillSprite.GlobalRotation = 0;
        animationPlayer.Play("float");
    }


    private void SetPartId(string value)
    {
        Part = Assets.Instance.GetPart(value);

        if (IsInsideTree())
            sprite2d.Texture = Assets.Instance.GetPartTexture(Part);
    }

    private void SetSkillId(string value)
    {
        if (value == null)
        {
            Skill = null;
        }
        
        Skill = Assets.Instance.GetSkill(value);

        if (IsInsideTree())
            skillSprite.Texture = Skill == null ? null : Assets.Instance.GetSkillTexture(Skill.id);
    }

    private void SetFlipped(bool value)
    {
        flipped = value;

        if (IsInsideTree())
            sprite2d.FlipH = value;
    }

    private void SetShiny(bool value)
    {
        shiny = value;

        if (IsInsideTree())
            sprite2d.Material = value ? Assets.ShinyMaterial : null;
    }
}