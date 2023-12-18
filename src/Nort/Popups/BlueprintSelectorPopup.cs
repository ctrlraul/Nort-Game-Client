using System;
using Godot;
using CtrlRaul.Godot.Linq;
using Nort.Listing;

namespace Nort.Popups;

public partial class BlueprintSelectorPopup : GenericPopup
{
	public event Action<Blueprint> BlueprintSelected;

	[Export] private PackedScene blueprintsListItemScene;

	private VBoxContainer blueprintsList;

	public override async void _Ready()
	{
		base._Ready();

		blueprintsList = GetNode<VBoxContainer>("%BlueprintsList");
		Cancellable = true;

		blueprintsList.QueueFreeChildren();

		await Game.Instance.Initialize();
		
		foreach (Blueprint blueprint in Assets.Instance.GetBlueprints())
		{
			BlueprintsListItem item = blueprintsListItemScene.Instantiate<BlueprintsListItem>();
			blueprintsList.AddChild(item);
			item.Blueprint = blueprint;
			item.Selected += OnBlueprintsListItemSelected;
		}
	}
	
	private void OnBlueprintsListItemSelected(Blueprint blueprint)
	{
		BlueprintSelected?.Invoke(blueprint);
		Remove();
	}
}