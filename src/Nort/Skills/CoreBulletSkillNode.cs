using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot;
using CtrlRaul.Godot.Linq;
using Nort;
using Nort.Entities.Components;

namespace Nort.Skills;

public partial class CoreBulletSkillNode : SkillNode
{
    [Ready] public Area2D rangeArea;
    [Ready] public CollisionShape2D rangeAreaCollisionShape2D;
    [Ready] public Timer cooldownTimer;

    private CraftBodyPart target;
    private CraftBodyPart Target
    {
        get => target;
        set => SetTarget(value);
    }

    public override void _Ready()
    {
        this.InitializeReady();
        
        // rangeAreaCollisionShape2D.Shape = new CircleShape2D { Radius = RangeRadius };
        // rayCollisionShape2D.Shape = new SegmentShape2D
        // {
        //     A = rayCollisionShape2D.Shape.A,
        //     B = new Vector2(0, -RangeRadius),
        // };

        SetPhysicsProcess(false);
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalRotation = GlobalPosition.AngleToPoint(Target.GlobalPosition) + Mathf.Pi * 0.5f;
    }

    public override void Fire()
    {
        if (Target == null || cooldownTimer.TimeLeft > 0)
            return; 

        if (Target.IsDestroyed)
            TryToFindNewTarget();

        if (Target == null)
            return;

        cooldownTimer.Start();

        GD.Print("Core bullet fire!");
    }

    private void TryToFindNewTarget()
    {
        List<CraftBodyPart> foePartsInRange = GetFoePartsInArea(rangeArea).ToList();
        Target = foePartsInRange.Any() ? foePartsInRange[(int)GD.Randi() % foePartsInRange.Count] : null;
    }

    private bool IsFoePartArea(Area2D area)
    {
        return (area.Owner as CraftBodyPart)!.Faction != part.Faction;
    }

    private IEnumerable<CraftBodyPart> GetFoePartsInArea(Area2D area)
    {
        return area.GetOverlappingAreas()
            .Where(area2 => IsFoePartArea(area2) && !(area2.Owner as CraftBodyPart)!.IsDestroyed)
            .Select(partArea => partArea.Owner as CraftBodyPart);
    }

    private void SetTarget(CraftBodyPart value)
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
            Target = (CraftBodyPart)area.Owner;
    }

    private void OnRangeAreaAreaExited(Area2D area)
    {
        if (Target != null && area.Owner == Target)
            TryToFindNewTarget();
    }
}