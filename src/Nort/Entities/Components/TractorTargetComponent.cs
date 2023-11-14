using Godot;

namespace Nort.Entities.Components;

public partial class TractorTargetComponent : EntityComponent
{
    private Area2D area;
    private AnimationPlayer animationPlayer;

    private bool _inRange;
    public bool InRange
    {
        get => _inRange;
        set
        {
            animationPlayer.Play(value ? "show" : "hide");
            _inRange = value;
        }
    }

    public override void _Ready()
    {
        base._Ready();
        area = GetNode<Area2D>("%Area2D");
        animationPlayer = GetNode<AnimationPlayer>("%AnimationPlayer");
        animationPlayer.Play("hide");
    }

    public void Targeted(Craft by)
    {
        
    }

    public void Released(Craft from)
    {
        
    }

    private void OnAnimationPlayerAnimationFinished(StringName animationName)
    {
        if (animationName == "show")
        {
            animationPlayer.Play("rotate");
        }
    }
}