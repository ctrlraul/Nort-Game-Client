using Godot;
using System;
using CtrlRaul.Godot;
using Nort.UI;

namespace Nort.Listing;

public partial class BlueprintsListItem : MarginContainer
{
	public event Action<Blueprint> Selected;

	[Ready] public DisplayCraft displayCraft;
	[Ready] public Label idLabel;
	[Ready] public Label coreLabel;
	[Ready] public Label partsCountLabel;

	private Blueprint _blueprint;
	public Blueprint Blueprint
	{
		get => _blueprint;
		set
		{
			_blueprint = value;
			displayCraft.Blueprint = value;
			idLabel.Text = value.id;
			coreLabel.Text = $"Core: {value.core.Part.displayName}";
			partsCountLabel.Text = $"Parts: {value.hulls.Count + 1}"; // +1 is the core
		}
	}

	public override void _Ready()
	{
		base._Ready();
		this.InitializeReady();

		displayCraft.Faction = null;
	}

	public void OnButtonPressed()
	{
		if (Blueprint != null)
		{
			Selected?.Invoke(Blueprint);
		}
	}
}