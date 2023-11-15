using Godot;

namespace Nort.Pages.CraftBuilder;

public partial class CraftSummary : PanelContainer
{
	private Label coreLabel;
	private Label hullLabel;
	private Label massLabel;
	private Label partsLabel;

	public override void _Ready()
	{
		base._Ready();
		coreLabel = GetNode<Label>("%CoreLabel");
		hullLabel = GetNode<Label>("%HullLabel");
		massLabel = GetNode<Label>("%MassLabel");
		partsLabel = GetNode<Label>("%PartsLabel");
	}

	public void SetBlueprint(Blueprint blueprint)
	{
		BlueprintStats stats = Blueprint.GetStats(blueprint);
		coreLabel.Text = stats.core.ToString();
		hullLabel.Text = stats.hull.ToString();
		massLabel.Text = stats.mass.ToString();
		partsLabel.Text = $"Parts: {blueprint.hulls.Count + 1}"; // +1 is the core
	}

	public void Clear()
	{
		coreLabel.Text = "0";
		hullLabel.Text = "0";
		massLabel.Text = "0";
		partsLabel.Text = "Parts: 0";
	}
}