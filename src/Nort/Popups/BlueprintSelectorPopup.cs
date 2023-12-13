using System;
using Godot;
using CtrlRaul.Godot.Linq;

namespace Nort.Popups;

public partial class BlueprintSelectorPopup : GenericPopup
{
	public Action<Blueprint> BlueprintSelected;

	[Export] private PackedScene BlueprintsListItemScene;

	private VBoxContainer blueprintsList;

	public async override void _Ready()
	{
		base._Ready();

		blueprintsList = GetNode<VBoxContainer>("BlueprintsList");
		Cancellable = true;

		blueprintsList.QueueFreeChildren();

		await Game.Instance.Initialize();
		
		foreach (Blueprint blueprint in Assets.Instance.GetBlueprints())
		{
			BlueprintsListItem item = BlueprintsListItemScene.Instantiate<BlueprintsListItem>();
			blueprintsList.AddChild(item);
			item.Blueprint = blueprint;
			item.Selected += blueprint =>
			{
				BlueprintSelected?.Invoke(blueprint);
				QueueFree();
			};
		}
	}
}