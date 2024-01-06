using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot;
using Nort.Entities.Components;
using Nort.Pages;

namespace Nort.Entities;

public partial class TurretCraft : Craft
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

	public IEnumerable<string> BlueprintIdOptions => Config.TurretBlueprints;

	#endregion


	public TurretCraft() : base()
	{
		blueprint = Assets.Instance.GetBlueprint(Config.TurretBlueprints.First());
	}


	public override void _Ready()
	{
		base._Ready();
		SetPhysicsProcess(false);
	}
}