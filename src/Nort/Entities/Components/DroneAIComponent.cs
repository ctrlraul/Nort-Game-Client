using Godot;
using System;
using System.Collections.Generic;

namespace Nort.Entities.Components;

public partial class DroneAiComponent : EntityComponent
{
    private const int FramesBetweenChecks = 5;
    private const float PathPointDistanceTolerance = 200;

    private FlightComponent flightComponent;
    private readonly int checkTimeOffset = new Random().Next() % FramesBetweenChecks;
    private readonly List<Vector2> path = new();

    public override void Init()
    {
        //flightComponent = Craft.GetComponent<FlightComponent>();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (path.Count == 0)
            return;

        flightComponent.Direction = Craft.Position.DirectionTo(path[0]);

        if (Engine.GetFramesDrawn() % FramesBetweenChecks != checkTimeOffset)
            return;
        
        if (Craft.Position.DistanceTo(path[0]) < PathPointDistanceTolerance)
            path.RemoveAt(0);
    }

    private void OnArea2DAreaEntered(Area2D area)
    {
        if (area.Owner is CraftBodyPart part && part.Faction != Craft.Faction)
        {
            path.Insert(0, Craft.Position.Lerp(part.Position, 0.5f));
        }
    }

    private void OnArea2DAreaExited(Area2D area)
    {
        if (area.Owner is CraftBodyPart part && part.Faction != Craft.Faction)
        {
            path.Insert(0, part.Position);
        }
    }
}
