using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot;
using Godot;
using Nort.Entities;

namespace Nort.Pages.MissionEditor;

public partial class EditorPlayerCraft : EditorCraft, IEditorEntity<PlayerCraftSetup>
{
    [Ready] public Label label;
    
    public new PlayerCraftSetup Setup
    {
        get => new() { testBlueprint = Blueprint };
        set => Blueprint = value.testBlueprint;
    }
    
    public new Blueprint Blueprint
    {
        get => base.Blueprint;
        protected set
        {
            label.Position = label.Position with { Y = Assets.Instance.GetBlueprintVisualSize(value).Y * 0.5f + 20 };
            base.Blueprint = value;
        }
    }

    public override void _Ready()
    {
        base._Ready();
        Initialize();
    }

    private async void Initialize()
    {
        await Game.Instance.Initialize();
        displayCraft.Color = Assets.Instance.PlayerFaction.Color;
    }

    protected override List<ExplorerField> InitExplorerFields()
    {
        List<Blueprint> blueprints = Assets.Instance.GetBlueprints().ToList();
        IEnumerable<string> blueprintNames = blueprints.Select(b => b.id.Capitalize());
        
        return new List<ExplorerField>
        {
            new ExplorerOptionsField(this, nameof(Blueprint), blueprints, blueprintNames)
        };
    }
}