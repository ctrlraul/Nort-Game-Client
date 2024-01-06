using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot;
using Godot;
using Nort.Entities.Components;
using Nort.Pages;

namespace Nort.Entities;

public partial class DroneCraft : Craft
{
    #region EntityInspector compatibility
    
    public IEnumerable<string> FactionIdOptions => Assets.Instance.GetFactions().Select(f => f.id);
    
    [Savable]
    [Inspect(nameof(FactionIdOptions))]
    public string FactionId
    {
        get => Faction.id;
        set => Faction = Assets.Instance.GetFaction(value);
    }

    [Savable]
    [Inspect(nameof(BlueprintIdOptions))]
    public string BlueprintId
    {
        get => Blueprint.id;
        set => Blueprint = Assets.Instance.GetBlueprint(value);
    }

    public IEnumerable<string> BlueprintIdOptions => Config.DroneBlueprints;

    #endregion


    private const int DistanceMaintainedFromTarget = 400;
    private const int DistanceToleratedFromTargetPosition = 32;
    
    [Ready] public FlightComponent flightComponent;
    [Ready] public Area2D range;
    [Ready] public Timer rememberTargetTimer;

    private CraftPart target;

    private CraftPart Target
    {
        get => target;
        set
        {
            target = value;
            SetPhysicsProcess(IsInstanceValid(target));
        }
    }

    private Vector2 targetPosition;


    public DroneCraft()
    {
        blueprint = Assets.Instance.GetBlueprint(Config.DroneBlueprints.First());
    }


    public override void _Ready()
    {
        base._Ready();
        SetPhysicsProcess(false);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (Game.Instance.InMissionEditor)
            return;

        targetPosition = Target.GlobalPosition +
                         Target.GlobalPosition.DirectionTo(Position) * DistanceMaintainedFromTarget;

        if (Position.DistanceTo(targetPosition) > ArtificialRadius + DistanceToleratedFromTargetPosition)
            flightComponent.Direction = Position.DirectionTo(targetPosition);
    }


    protected override void SetFaction(Faction value)
    {
        base.SetFaction(value);
        
        if (IsInsideTree())
            range.CollisionMask = Assets.Instance.GetFactionCollisionMask(faction);
    }


    private void LookForNewTarget()
    {
        Target = range.GetOverlappingAreas()
            .Cast<CraftPart>()
            .FirstOrDefault(part => !part.IsDestroyed);
    }
    

    private void OnRangeAreaEntered(Area2D area)
    {
        if (Target != null && rememberTargetTimer.IsStopped())
            return;

        if (!rememberTargetTimer.IsStopped())
            rememberTargetTimer.Stop();

        Target = (CraftPart)area;
    }

    private void OnRangeAreaExited(Area2D area)
    {
        if (Target == null || area != Target)
            return;

        if (Target.IsDestroyed)
        {
            LookForNewTarget();

            return;
        }

        // Idk how the fuck this is supposed to be outside of the tree while the drone itself
        // still is in it and not even queued for deletion and I sincerely cannot be fucked
        if (rememberTargetTimer.IsInsideTree())
            rememberTargetTimer.Start();
    }

    private void OnRememberTargetTimerTimeout()
    {
        LookForNewTarget();
    }
}