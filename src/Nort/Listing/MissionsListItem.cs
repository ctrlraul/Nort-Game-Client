using Godot;
using System;
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
			foreach (EntitySetup entitySetup1 in mission.entities)
			{
				GD.Print($"entitySetup: {entitySetup1.GetType().Name}");
			}
			
			EntitySetup entitySetup = mission.entities.FirstOrDefault(entity => entity is PlayerCraftSetup);

			if (entitySetup is PlayerCraftSetup playerCraftSetup)
				displayCraft.Blueprint = playerCraftSetup.testBlueprint;
			
			mission = value;
			displayNameLabel.Text = mission.displayName;
			idLabel.Text = mission.id;
			entitiesCountLabel.Text = $"Entities: {mission.entities.Count}";
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
