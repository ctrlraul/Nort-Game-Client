﻿using System;
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
    [Ready] public CollisionShape2D rangeAreaCollisionShape2D;
    [Ready] public RayCast2D rayCast2D;
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

            target = value;

            if (IsInstanceValid(target))
            {
                SetPhysicsProcess(true);
                LookAt(Target.GlobalPosition);
                rayCast2D.ForceRaycastUpdate();
                Fire();
            }
            else
            {
                SetPhysicsProcess(false);
                Rotation = -Mathf.Pi / 2; // Point up
            }
        }
    }


    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();
        
        if (cooldownTimer.WaitTime < particles.Lifetime)
            Logger.Warn(GetType().Name, "Firing cooldown should be greater than the particles' lifespan");
        
        range.CollisionMask = Assets.Instance.GetFactionCollisionMask(Part.Faction);
        rayCast2D.CollisionMask = range.CollisionMask;

        UpdateCollisionMasks();

        Part.Craft.FactionChanged += OnCraftFactionChanged;

        SetPhysicsProcess(false);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        LookAt(Target.GlobalPosition);
    }
    
    
    private void LookForATarget()
    {
        Target = range.GetOverlappingAreas().FindNearest(GlobalPosition, true) as CraftPart;
    }

    private void UpdateCollisionMasks()
    {
        range.CollisionMask = Assets.Instance.GetFactionCollisionMask(Part.Faction);
        rayCast2D.CollisionMask = range.CollisionMask;
        if (Target != null)
        {
            LookForATarget();
        }
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
        
        cooldownTimer.Start();
        particles.Emitting = true;
        
        if (rayCast2D.GetCollider() is CraftPart partHit)
        {
            partHit.Craft.TakeHit(partHit, this, Damage);
            Fired?.Invoke();
        }
    }


    private void OnCraftFactionChanged()
    {
        UpdateCollisionMasks();
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