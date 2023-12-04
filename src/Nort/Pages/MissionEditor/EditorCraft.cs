using System;
using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot;
using Godot;
using Nort.Entities;
using Nort.UI;

namespace Nort.Pages.MissionEditor;

public partial class EditorCraft : EditorEntity, IEditorEntity<CraftSetup>
{
    [Ready] public DisplayCraft displayCraft;

    public CraftSetup Setup
    {
        get => new()
        {
            Place = Position,
            Blueprint = Blueprint,
            Faction = Faction,
            componentSet = Behavior,
        };
        set
        {
            Position = value.Place;
            Blueprint = value.Blueprint;
            Faction = value.Faction;
            Behavior = value.componentSet;
        }
    }

    public Blueprint Blueprint
    {
        get => displayCraft.Blueprint;
        protected set
        {
            displayCraft.Blueprint = value;
            hitBox.Size = Assets.Instance.GetBlueprintVisualSize(value);
            hitBox.Position = -hitBox.Size * 0.5f;
        }
    }

    private Faction faction;
    public Faction Faction
    {
        get => faction;
        protected set
        {
            faction = value;
            displayCraft.Color = faction.Color;
        }
    }

    public Craft.ComponentSet Behavior { get; protected set; }

    protected override List<ExplorerField> InitExplorerFields()
    {
        List<Blueprint> blueprints = Assets.Instance.GetBlueprints().ToList();
        IEnumerable<string> blueprintNames = blueprints.Select(b => b.id.Capitalize());
        
        List<Faction> factions = Assets.Instance.GetFactions().ToList();
        IEnumerable<string> factionNames = factions.Select(f => f.displayName);
        
        return new List<ExplorerField>
        {
            new ExplorerOptionsField(this, nameof(Blueprint), blueprints, blueprintNames),
            new ExplorerOptionsField(this, nameof(Faction), factions, factionNames),
            //new ExplorerEnumField(this, nameof(Behavior), Enum.GetValues<>(), ""),
        };
    }

    private static IEnumerable<string> GetComponentSetNames()
    {
        return Enum.GetNames<Craft.ComponentSet>();
    }
}