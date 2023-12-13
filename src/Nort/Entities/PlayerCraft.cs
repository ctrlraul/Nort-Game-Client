using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot;
using Godot;
using Nort.Entities.Components;
using Nort.Hud;
using Nort.Pages;

namespace Nort.Entities;

public partial class PlayerCraft : Craft
{
    #region EntityInspector compatibility

    [Savable]
    [Inspect(nameof(TestBlueprintIdOptions))]
    public string TestBlueprintId
    {
        get => Blueprint.id;
        set => Blueprint = Assets.Instance.GetBlueprint(value);
    }

    public IEnumerable<string> TestBlueprintIdOptions => Assets.Instance.GetBlueprints().Select(b => b.id);

    #endregion


    [Ready] public FlightComponent flightComponent;
    [Ready] public Label label;


    public PlayerCraft() : base()
    {
        faction = Assets.Instance.PlayerFaction;
    }


    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();
    }


    protected override void UpdateEditorStuff()
    {
        base.UpdateEditorStuff();

        if (!IsInsideTree())
            return;

        label.Position = label.Position with { Y = blueprintVisualRect.Position.Y + blueprintVisualRect.Size.Y + 20 };
    }
}