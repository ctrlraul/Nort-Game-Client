using Godot;
using System;

namespace Nort.Entities;

public partial class ConductorCraft : Craft
{
	public event Action Conduct;
    
	
	public override void _Ready()
	{
		base._Ready();
		Blueprint = Assets.Instance.GetBlueprint("conductor");
		Faction = Assets.Instance.PlayerFaction;
	}


	private void OnInteractionRangeInteracted()
	{
		Conduct?.Invoke();
	}
}