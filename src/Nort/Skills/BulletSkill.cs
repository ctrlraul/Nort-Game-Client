using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using CtrlRaul;
using CtrlRaul.Godot;
using Nort.Entities;

namespace Nort.Skills;

public partial class BulletSkill : Node2D, ISkillNode
{
    #region ISkillNode Implementation
    
    public event Action Fired;

    public CraftPart Part { get; set; }
    
    public float CooldownMax => (float)cooldownTimer.WaitTime;
    public float Cooldown => (float)cooldownTimer.TimeLeft;
    public Texture2D Texture => GetNode<Sprite2D>("Sprite2D").Texture;

    #endregion
    
    
    private const float Damage = 3;

    [Ready] public Area2D range;
    [Ready] public ShapeCast2D aimShapeCast2D;
    [Ready] public GpuParticles2D particles;
    [Ready] public Timer cooldownTimer;
    
    private CraftPart target;
    public CraftPart Target
    {
        get => target;
        set
        {
            if (value == target)
                return;

            if (IsInstanceValid(target))
                target.Craft.FactionChanged -= LookForATarget;
            
            target = value;

            if (IsInstanceValid(target))
            {
                target.Craft.FactionChanged += LookForATarget;
                GlobalRotation = GlobalPosition.AngleToPoint(target.GlobalPosition);
                Fire();
                SetProcess(true);
            }
            else
            {
                ResetRotation();
                SetProcess(false);
            }
        }
    }


    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();
        
        if (cooldownTimer.WaitTime < particles.Lifetime)
            Logger.Warn(GetType().Name, "Firing cooldown should be greater than the particles' lifespan");

        Part.Craft.FactionChanged += UpdateCollisionMasks;

        ResetRotation();
        UpdateCollisionMasks();
        SetProcess(false);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        LookAt(Target.GlobalPosition);
    }
    
    
    private void LookForATarget()
    {
        Target = range.GetOverlappingAreas()
            .Cast<CraftPart>()
            .FirstOrDefault(part => !part.IsDestroyed);
    }
    
    private void ResetRotation()
    {
        Rotation = -Part.Rotation - Mathf.Pi / 2;
    }

    private void UpdateCollisionMasks()
    {
        range.CollisionMask = Assets.Instance.GetFactionCollisionMask(Part.Faction);
        aimShapeCast2D.CollisionMask = range.CollisionMask;
        LookForATarget();
    }

    private void Fire()
    {
        if (cooldownTimer.TimeLeft > 0)
            return;

        if (!IsInstanceValid(Target))
            return;

        if (Game.Instance.InMissionEditor)
            return;

        CraftPart aimedPart = GetAimedPart();

        if (aimedPart == null)
            return;

        // ShowFiredVisualFeedback();
        
        cooldownTimer.Start();
        particles.Emitting = true;
        
        aimedPart.Craft.TakeHit(aimedPart, this, Damage);

        AudioManager.Instance.PlayBulletFired(GlobalPosition);

        Fired?.Invoke();
    }

    private CraftPart GetAimedPart()
    {
        LookAt(Target.GlobalPosition);
        aimShapeCast2D.ForceShapecastUpdate();

        int collisionCount = aimShapeCast2D.GetCollisionCount();
        List<CraftPart> collidingParts = new();

        for (int i = 0; i < collisionCount; i++)
            collidingParts.Add((CraftPart)aimShapeCast2D.GetCollider(i));

        return collidingParts.FirstOrDefault(part => !part.IsDestroyed);
    }

    private void ShowFiredVisualFeedback()
    {
        Modulate = new Color(1, 0, 1);
        CreateTween().TweenProperty(this, "modulate:g", 1, cooldownTimer.WaitTime * 0.90);
    }
    
    
    private void OnRangeAreaEntered(Area2D area)
    {
        Target ??= (CraftPart)area;
    }

    private void OnRangeAreaExited(Area2D area)
    {
        if (area == Target)
            LookForATarget();
    }

    private void OnCooldownTimerTimeout()
    {
        Fire();
    }
}