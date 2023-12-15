﻿using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot;
using Nort.Entities.Components;
using Nort.Hud;
using Nort.Pages;

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
}