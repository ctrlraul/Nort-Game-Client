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

    private string blueprintId;

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
        get
        {
            Blueprint blueprint = Blueprint.From(displayCraft.Blueprint);
            blueprint.id = blueprintId;
            return blueprint;
        }
        protected set
        {
            blueprintId = value.id;
            displayCraft.Blueprint = value;
            Rect2 rect = Assets.Instance.GetBlueprintVisualRect(value);
            hitBox.Size = rect.Size;
            hitBox.Position = rect.Position;
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
            new ExplorerEnumField(this, nameof(Behavior), typeof(Craft.ComponentSet)),
        };
    }
}