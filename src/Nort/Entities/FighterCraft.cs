using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot;
using Godot;
using Nort.Entities.Components;
using Nort.Pages;
using Nort.Skills;

namespace Nort.Entities;

public partial class FighterCraft : Craft
{
	#region EntityInspector compatibility

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

	public IEnumerable<string> FactionIdOptions => Assets.Instance.GetFactions().Select(f => f.id);
	public IEnumerable<string> BlueprintIdOptions => Assets.Instance.GetBlueprints().Select(b => b.id);

	#endregion


	[Ready] public StatsDisplayComponent statsDisplayComponent;


	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (Game.Instance.InMissionEditor)
			return;

		if (Engine.GetFramesDrawn() % 60 != 0)
			return;

		foreach (ISkillNode skillNode in skillNodes)
		{
			if (skillNode is DroneSkill droneSkill)
				droneSkill.Fire();
		}
	}
}