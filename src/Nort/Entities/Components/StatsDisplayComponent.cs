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
        
        Rect2 rect = Assets.Instance.GetBlueprintVisualRect(Craft.Blueprint);

        Scale = Scale with { X = rect.Size.X };
        Position = Position with { Y = rect.Position.Y + rect.Size.Y + 10 };

        hull.Modulate = Craft.Faction.Color;
    }

    public override void _Process(double delta)
    {
        hull.Scale = hull.Scale with { X = Mathf.Clamp(Craft.Hull / Craft.HullMax, 0, 1) };
        core.Scale = core.Scale with { Y = Mathf.Clamp(Craft.Core / Craft.CoreMax, 0, 1) };
    }
}
