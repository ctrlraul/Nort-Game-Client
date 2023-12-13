using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot;
using Nort.Entities;
using Nort.UI;

namespace Nort.Listing;

public partial class MissionsListItem : MarginContainer
{
	public event Action Selected;

	private Mission mission;
	public Mission Mission
	{
		get => mission;
		set
		{
			mission = value;
			
			// Dictionary<string, object> entitySetup = mission.entitySetups.First(entityDictionary =>
			// {
			// 	return entityDictionary.TryGetValue("Type", out object type) && (string)type == nameof(PlayerCraft)
			// });
			//
			// if (entitySetup is PlayerCraftSetup playerCraftSetup)
			// 	displayCraft.Blueprint = playerCraftSetup.TestBlueprint;
			
			displayNameLabel.Text = mission.displayName;
			idLabel.Text = mission.id;
			entitiesCountLabel.Text = $"Entities: {mission.entitySetups.Count}";
		}
	}
	
	[Ready] public DisplayCraft displayCraft;
	[Ready] public Label idLabel;
	[Ready] public Label displayNameLabel;
	[Ready] public Label entitiesCountLabel;
	
	public override void _Ready()
	{
		base._Ready();
		this.InitializeReady();
		Initialize();
	}

	private async void Initialize()
	{
		await Game.Instance.Initialize();
		displayCraft.Modulate = Assets.Instance.PlayerFaction.Color;
	}

	private void OnButtonPressed()
	{
		if (Mission != null)
		{
			Selected?.Invoke();
		}
	}
}
