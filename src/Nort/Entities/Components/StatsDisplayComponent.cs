using CtrlRaul.Godot;
using Godot;
using Shouldly;

namespace Nort.Entities.Components;

public partial class StatsDisplayComponent : EntityComponent
{
    [Ready] public Node2D hull;
    [Ready] public Node2D core;

    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();
        
        Craft.ShouldNotBeNull($"Expected {nameof(Craft)} as parent");

        Vector2 halfSize = Assets.Instance.GetBlueprintVisualSize(Craft.Blueprint) * 0.5f;

        Scale = Scale with { X = halfSize.X };
        Position = Position with { Y = halfSize.Y + 10 };

        hull.Modulate = Craft.Faction.Color;
    }

    public override void _Process(double delta)
    {
        hull.Scale = hull.Scale with { X = Mathf.Clamp(Craft.Hull / Craft.HullMax, 0, 1) };
        core.Scale = core.Scale with { Y = Mathf.Clamp(Craft.Core / Craft.CoreMax, 0, 1) };
    }
}
