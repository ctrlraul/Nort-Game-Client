using Godot;
using System;
using CtrlRaul.Godot;

namespace Nort.Entities;

public partial class ConductorCraft : Craft
{
	public event Action Conduct;


	[Ready] public AnimationPlayer animationPlayer;


	public ConductorCraft()
	{
		blueprint = Assets.Instance.GetBlueprint("conductor");
		faction = Assets.Instance.PlayerFaction;
	}


	protected override void OnSpawning()
	{
		base.OnSpawning();
		animationPlayer.Play("summon_portal");
	}

	private void OnInteractionRangeInteracted()
	{
		Conduct?.Invoke();
	}
}