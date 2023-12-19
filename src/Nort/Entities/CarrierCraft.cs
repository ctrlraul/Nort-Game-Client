using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot;
using Godot;
using Nort.Entities.Components;
using Nort.Hud;
using Nort.Skills;

namespace Nort.Entities;

public partial class CarrierCraft : Craft
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

    public IEnumerable<string> BlueprintIdOptions => Config.CarrierBlueprints;

    #endregion


    [Ready] public StatsDisplayComponent statsDisplayComponent;
    

    public CarrierCraft() : base()
    {
        blueprint = Assets.Instance.GetBlueprint(Config.CarrierBlueprints.First());
    }


    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (Game.Instance.InMissionEditor)
            return;

        if (Engine.GetFramesDrawn() % 60 != 0)
            return;

        foreach (ISkillNode skillNode in GetSkillNodes())
        {
            if (skillNode is DroneSkillNode droneSkill)
                droneSkill.Fire();
        }
    }
}