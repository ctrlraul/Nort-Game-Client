using Godot;
using System;
using Shouldly;

namespace Nort.Entities.Components;

public partial class TractorComponent : EntityComponent
{
    private const float Distance = 200;
    private const float Torque = 10;
    
    private Line2D line;
    
    public TractorTargetComponent target;
    private Action<double> tickMethod;

    private Craft craft;
    
    public override void _Ready()
    {
        base._Ready();
        Craft.ShouldNotBeNull($"Expected {nameof(Craft)} as parent");
        line = GetNode<Line2D>("%Line2D");
        Clear();
    }

    public override void _PhysicsProcess(double delta)
    {
        delta *= Engine.PhysicsTicksPerSecond; // Normalize delta
        tickMethod?.Invoke(delta);
        UpdateGfx();
    }

    public void SetTarget(TractorTargetComponent value)
    {
        if (value == null)
        {
            Clear();
            return;
        }

        if (value.Entity is OrphanPart)
        {
            tickMethod = TickForDroppedPart;
        }
        else
        {
            tickMethod = TickForAnything;
        }

        target = value;
        SetPhysicsProcess(true);
    }

    private void Clear()
    {
        Visible = false;
        tickMethod = TickForAnything;
        SetPhysicsProcess(false);
    }

    private void TickForAnything(double delta)
    {
        float targetDistance = GlobalTransform.Origin.DistanceTo(target.Entity.GlobalTransform.Origin);
        float force = Math.Abs(Distance - targetDistance) / Distance * Torque;
        int forceDelta = targetDistance < Distance ? 1 : -1;
        Vector2 direction = GlobalTransform.Origin.DirectionTo(target.Entity.GlobalTransform.Origin);
        target.Entity.GlobalTransform = target.Entity.GlobalTransform with
        {
            Origin = target.Entity.GlobalTransform.Origin + direction * force * forceDelta * (float)delta
        };
    }

    private void TickForDroppedPart(double delta)
    {
        Vector2 direction = GlobalTransform.Origin.DirectionTo(target.Entity.GlobalTransform.Origin);
        target.Entity.GlobalTransform = target.Entity.GlobalTransform with
        {
            Origin = target.Entity.GlobalTransform.Origin + direction * Torque * (float)delta
        };
    }

    private void UpdateGfx()
    {
        line.Points[1] = target.Entity.GlobalTransform.Origin - GlobalTransform.Origin;
    }
}
