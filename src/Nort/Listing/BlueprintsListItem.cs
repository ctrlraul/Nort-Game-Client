using Godot;
using System;
using Nort.UI;

namespace Nort;

public partial class BlueprintsListItem : MarginContainer
{
	public event Action<Blueprint> Selected;

	private DisplayCraft displayCraft;
	private Label idLabel;
	private Label coreLabel;
	private Label partsCountLabel;

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

		displayCraft = GetNode<DisplayCraft>("DisplayCraft");
		idLabel = GetNode<Label>("IDLabel");
		coreLabel = GetNode<Label>("CoreLabel");
		partsCountLabel = GetNode<Label>("PartsCountLabel");

		displayCraft.Color = Config.FactionlessColor;
	}

	public void OnButtonPressed()
	{
		if (Blueprint != null)
		{
			Selected?.Invoke(Blueprint);
		}
	}
}