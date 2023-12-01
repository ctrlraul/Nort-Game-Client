using Godot;
using System;
using CtrlRaul.Godot;
using Nort;
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
			displayNameLabel.Text = mission.displayName;
			idLabel.Text = mission.id;
			entitiesCountLabel.Text = $"Entities: {mission.entities.Count}";
		}
	}
	
	[Ready] private DisplayCraft displayCraft;
	[Ready] private Label idLabel;
	[Ready] private Label displayNameLabel;
	[Ready] private Label entitiesCountLabel;
	
	public override void _Ready()
	{
		base._Ready();
		this.InitializeReady();
		displayCraft.Modulate = Config.FactionlessColor;
	}

	private void OnButtonPressed()
	{
		if (Mission != null)
		{
			Selected?.Invoke();
		}
	}
}
