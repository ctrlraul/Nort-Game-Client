using Godot;

namespace Nort.Entities;

public partial class OrphanPart : Entity
{
    private Sprite2D _sprite;
    private Sprite2D _skillSprite;
    private AnimationPlayer _animationPlayer;

    public override void _Ready()
    {
        _sprite = GetNode<Sprite2D>("%Sprite2D");
        _skillSprite = GetNode<Sprite2D>("%SkillSprite");
        _animationPlayer = GetNode<AnimationPlayer>("%AnimationPlayer");

        _sprite.SelfModulate = Config.FactionlessColor;
        _skillSprite.GlobalRotation = 0;
        _animationPlayer.Play("float");
    }

    public void SetSetup(OrphanPartSetup setup)
    {
        Position = setup.Place;
        Rotation = setup.angle;
		
        _sprite.Texture = Assets.Instance.GetPartTexture(setup.Part);
        _sprite.FlipH = setup.flipped;
        _sprite.Material = setup.shiny ? Assets.ShinyMaterial : null;

        if (!string.IsNullOrEmpty(setup.skillId))
            _skillSprite.Texture = Assets.Instance.GetSkillTexture(setup.skillId);
        else
            _skillSprite.QueueFree();
    }
}