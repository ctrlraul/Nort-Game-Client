using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot;
using Nort.Entities;

namespace Nort.Skills;

public partial class CoreBulletSkill : Node2D, ISkillNode
{
    #region ISkillNode Implementation
    
    public event Action Fired;

    public CraftPart Part { get; set; }
    
    public float CooldownMax => (float)cooldownTimer.WaitTime;
    public float Cooldown => (float)cooldownTimer.TimeLeft;
    public Texture2D Texture => GetNode<Sprite2D>("Sprite2D").Texture;

    #endregion
    
    
    [Ready] public Area2D rangeArea;
    [Ready] public CollisionShape2D rangeAreaCollisionShape2D;
    [Ready] public Timer cooldownTimer;

    private CraftPart target;
    private CraftPart Target
    {
        get => target;
        set => SetTarget(value);
    }

    public override void _Ready()
    {
        this.InitializeReady();
        SetPhysicsProcess(false);
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalRotation = GlobalPosition.AngleToPoint(Target.GlobalPosition) + Mathf.Pi * 0.5f;
    }

    public void Fire()
    {
        if (Target == null || cooldownTimer.TimeLeft > 0)
            return; 

        if (Target.IsDestroyed)
            TryToFindNewTarget(null);

        if (Target == null)
            return;

        cooldownTimer.Start();

        GD.Print("Core bullet fire!");
        
        Fired?.Invoke();
    }

    private void TryToFindNewTarget(CraftPart foePart)
    {
        List<CraftPart> foePartsInRange = GetFoePartsInArea(rangeArea).ToList();
        Target = foePartsInRange.Any() ? foePartsInRange[(int)GD.Randi() % foePartsInRange.Count] : null;
    }

    private bool IsFoePartArea(Area2D area)
    {
        return (area.Owner as CraftPart)!.Faction != Part.Faction;
    }

    private IEnumerable<CraftPart> GetFoePartsInArea(Area2D area)
    {
        return area.GetOverlappingAreas()
            .Where(area2 => IsFoePartArea(area2) && !(area2.Owner as CraftPart)!.IsDestroyed)
            .Select(partArea => partArea.Owner as CraftPart);
    }

    private void SetTarget(CraftPart value)
    {
        if (value == target)
            return;

        if (target is { IsDestroyed: false })
            target.Destroyed -= TryToFindNewTarget;

        target = value;

        if (target != null)
        {
            target.Destroyed += TryToFindNewTarget;
            Fire();
            SetPhysicsProcess(true);
        }
        else
        {
            Rotation = 0;
            SetPhysicsProcess(false);
        }
    }

    private void OnRangeAreaAreaEntered(Area2D area)
    {
        if (Target == null && IsFoePartArea(area))
            Target = (CraftPart)area.Owner;
    }

    private void OnRangeAreaAreaExited(Area2D area)
    {
        if (Target != null && area.Owner == Target)
            TryToFindNewTarget(null);
    }
}