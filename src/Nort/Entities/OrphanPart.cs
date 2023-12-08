using CtrlRaul.Godot;
using Godot;

namespace Nort.Entities;

public partial class OrphanPart : Entity
{
    [Ready] public Sprite2D sprite2d;
    [Ready] public Sprite2D skillSprite;
    [Ready] public AnimationPlayer animationPlayer;

    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();
        
        sprite2d.SelfModulate = Config.FactionlessColor;
        skillSprite.GlobalRotation = 0;
        animationPlayer.Play("float");
    }

    public void SetSetup(OrphanPartSetup setup)
    {
        Position = setup.Place;
        Rotation = setup.angle;
        sprite2d.Texture = Assets.Instance.GetPartTexture(setup.Part);
        sprite2d.FlipH = setup.flipped;
        sprite2d.Material = setup.shiny ? Assets.ShinyMaterial : null;
        skillSprite.Texture = string.IsNullOrEmpty(setup.skillId) ? null : Assets.Instance.GetSkillTexture(setup.skillId);
    }
}