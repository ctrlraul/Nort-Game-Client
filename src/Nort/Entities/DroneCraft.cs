using System;
using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot;
using Godot;
using Nort.Entities.Components;
using Nort.Hud;
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
    
    
    private const int ProcessIntervalFrames = 10;
    private const float PathPointDistanceTolerance = 200;

    private readonly int processIntervalFramesOffset = (int)GD.Randi() % ProcessIntervalFrames;
    private readonly List<Vector2> path = new();
    private readonly List<Craft> foesInRange = new();
    
    [Ready] public FlightComponent flightComponent;
    [Ready] public Area2D range;
    [Ready] public Line2D pathLine2D;

    private bool IsProcessFrame => Engine.GetFramesDrawn() % ProcessIntervalFrames != processIntervalFramesOffset;
    private Vector2 targetPosition;
    private Craft target;
    

    public DroneCraft() : base()
    {
        blueprint = Assets.Instance.GetBlueprint(Config.DroneBlueprints.First());
    }


    public override void _Ready()
    {
        base._Ready();

        if (Game.Instance.InMissionEditor)
        {
            SetPhysicsProcess(false);
            range.Monitoring = false;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (path.Any())
        {
            flightComponent.Direction = Position.DirectionTo(path[0]);

            if (IsProcessFrame)
            {
                if (Position.DistanceTo(path[0]) < PathPointDistanceTolerance)
                {
                    path.RemoveAt(0);
                }
            }
        }
        
        UpdateDebugVisuals();
    }
    

    private void OnRangeAreaEntered(Area2D area)
    {
        if (area.Owner is Craft craft && Faction.Hostile(craft.Faction, Faction))
        {
            foesInRange.Add(craft);
        }
    }

    private void OnRangeAreaExited(Area2D area)
    {
        if (area.Owner is Craft craft && Faction.Hostile(craft.Faction, Faction))
        {
            foesInRange.Remove(craft);

            if (!foesInRange.Any())
            {
                float distance = Position.DistanceTo(craft.Position);
                path.Add(craft.Position.Lerp(Position, (craft.ArtificialRadius) / distance));
                UpdateDebugVisuals();
            }
        }
    }

    private void UpdateDebugVisuals()
    {
        if (Config.DebugAi)
            pathLine2D.Points = new[]{ Position }.Concat(path).ToArray();
    }
}