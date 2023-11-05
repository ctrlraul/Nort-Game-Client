using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot.Linq;
using Nort;
using Nort.Entities.Components;

public partial class CraftBodyPartBulletSkill : CraftBodyPartSkill
{
    public readonly float damage = 5;
    private const float RangeRadius = 700;

    private Area2D rangeArea;
    private CollisionShape2D rangeAreaCollisionShape2D;
    private Area2D ray;
    private CollisionShape2D rayCollisionShape2D;
    private GpuParticles2D particles;
    private Timer cooldownTimer;

    private CraftBodyPart _target;
    private CraftBodyPart Target
    {
        get => _target;
        set => SetTarget(value);
    }

    public override void _Ready()
    {
        rangeArea = GetNode<Area2D>("%RangeArea");
        rangeAreaCollisionShape2D = GetNode<CollisionShape2D>("%RangeAreaCollisionShape2D");
        ray = GetNode<Area2D>("%Ray");
        rayCollisionShape2D = GetNode<CollisionShape2D>("%RayCollisionShape2D");
        particles = GetNode<GpuParticles2D>("%GpuParticles2D");
        cooldownTimer = GetNode<Timer>("%CooldownTimer");
        
        if (cooldownTimer.WaitTime > particles.Lifetime)
            throw new Exception("ayo bro this aint worky");

        // rangeAreaCollisionShape2D.Shape.Radius = RangeRadius;
        // rayCollisionShape2D.Shape.B = new Vector2(0, -RangeRadius);

        rangeAreaCollisionShape2D.Shape = new CircleShape2D { Radius = RangeRadius };
        rayCollisionShape2D.Shape = new SegmentShape2D { B = new Vector2(0, -RangeRadius) };

        GetNode<Sprite2D>("Sprite2D").Texture = Assets.Instance.GetSkillTexture("bullet");
        SetPhysicsProcess(false);
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalRotation = GlobalPosition.AngleToPoint(Target.GlobalPosition) + Mathf.Pi * 0.5f;
    }

    private void Fire()
    {
        if (Target == null || cooldownTimer.TimeLeft > 0)
            return; 

        if (Target.IsDestroyed)
            TryToFindTarget();

        if (Target == null)
            return;

        cooldownTimer.Start();
        particles.Emitting = true;

        IEnumerable<CraftBodyPart> foePartsInRay = GetFoePartsInArea(ray);
        CraftBodyPart partHit = foePartsInRay.FindNearest(part.GlobalPosition, true);

        if (partHit == null)
            return;

        Target.body.Craft.TakeHit(this, partHit);
    }

    private void TryToFindTarget()
    {
        List<CraftBodyPart> foePartsInRange = GetFoePartsInArea(rangeArea).ToList();
        Target = foePartsInRange.Any() ? foePartsInRange[(int)GD.Randi() % foePartsInRange.Count] : null;
    }

    private bool IsFoePartArea(Area2D area)
    {
        return (area.Owner as CraftBodyPart)!.body.Craft.Faction != part.body.Craft.Faction;
    }

    private IEnumerable<CraftBodyPart> GetFoePartsInArea(Area2D area)
    {
        return area.GetOverlappingAreas()
            .Where(area2 => IsFoePartArea(area2) && !(area2.Owner as CraftBodyPart)!.IsDestroyed)
            .Select(partArea => partArea.Owner as CraftBodyPart);
    }

    private void SetTarget(CraftBodyPart value)
    {
        if (value == _target)
            return;

        if (_target is { IsDestroyed: false })
            _target.Destroyed -= OnTargetDestroyed;

        _target = value;

        if (_target != null)
        {
            _target.Destroyed += OnTargetDestroyed;
            Fire();
            SetPhysicsProcess(true);
        }
        else
        {
            Rotation = 0;
            SetPhysicsProcess(false);
        }
    }

    private void OnCooldownTimerTimeout()
    {
        Fire();
    }

    private void OnRangeAreaAreaEntered(Area2D area)
    {
        if (Target == null && IsFoePartArea(area))
            Target = (CraftBodyPart)area.Owner;
    }

    private void OnRangeAreaAreaExited(Area2D area)
    {
        if (Target != null && area.Owner == Target)
            TryToFindTarget();
    }

    private void OnTargetDestroyed()
    {
        TryToFindTarget();
    }
}