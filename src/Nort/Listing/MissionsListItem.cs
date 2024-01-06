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
			mission = value;

			EntitySetup displaySetup = mission.entitySetups.FirstOrDefault(setup => setup.IsForType<PlayerCraft>());

			displayCraft.Blueprint = (
				displaySetup == default
				? Assets.Instance.InitialBlueprint
				: Assets.Instance.GetBlueprint((string)displaySetup.subTypeData["TestBlueprintId"])
			);
			
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
		displayCraft.Faction = Assets.Instance.PlayerFaction;
	}

	private void OnButtonPressed()
	{
		if (Mission != null)
		{
			Selected?.Invoke();
		}
	}
}
