using System;
using System.Linq;
using Godot;
using CtrlRaul;
using CtrlRaul.Godot;
using CtrlRaul.Godot.Linq;
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
    [Ready] public Area2D pseudoRay;
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
                SetProcess(true);
                Fire();
            }
            else
            {
                SetProcess(false);
                PointUp();
            }
        }
    }


    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();
        
        if (cooldownTimer.WaitTime < particles.Lifetime)
            Logger.Warn(GetType().Name, "Firing cooldown should be greater than the particles' lifespan");

        PointUp();
        UpdateCollisionMasks();

        Part.Craft.FactionChanged += UpdateCollisionMasks;

        SetProcess(false);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        LookAt(Target.GlobalPosition);
    }
    
    
    private void LookForATarget()
    {
        Target = range.GetOverlappingAreas().FindNearest(GlobalPosition, true) as CraftPart;
    }
    
    private void PointUp()
    {
        GlobalRotation = -Mathf.Pi / 2;
    }

    private void UpdateCollisionMasks()
    {
        range.CollisionMask = Assets.Instance.GetFactionCollisionMask(Part.Faction);
        pseudoRay.CollisionMask = range.CollisionMask;
        LookForATarget();
    }

    public void Fire()
    {
        if (Game.Instance.InMissionEditor)
            return;
        
        if (Part.IsDestroyed)
            return;

        if (Target == null)
            return;
        
        if (cooldownTimer.TimeLeft > 0)
            return;

        LookAt(Target.GlobalPosition);

        CraftPart aimedPart = GetAimedPart();

        if (aimedPart == null)
            return;

        cooldownTimer.Start();
        particles.Emitting = true;

        aimedPart.Craft.TakeHit(aimedPart, this, Damage);

        AudioManager.Instance.PlayBulletFired(GlobalPosition);

        Fired?.Invoke();
    }

    private CraftPart GetAimedPart()
    {
        return pseudoRay
            .GetOverlappingAreas()
            .Cast<CraftPart>()
            .OrderBy(part => part.GlobalPosition.DistanceTo(GlobalPosition))
            .FirstOrDefault(part => !part.IsDestroyed);
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