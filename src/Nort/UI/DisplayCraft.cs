using Godot;
using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot.Linq;

namespace Nort.UI;

public partial class DisplayCraft : Control
{
	[Export] private PackedScene displayCraftPartScene;
	
	private Control _partsContainer;
	private DisplayCraftPart _core;

	private Color _color;
	public Color Color
	{
		get => _color;
		set
		{
			_color = value;
			if (IsInsideTree())
				UpdateColor();
		}
	}

	public Blueprint Blueprint
	{
		get => GetBlueprint();
		set => SetBlueprint(value);
	}

	public IEnumerable<DisplayCraftPart> Parts => _partsContainer.GetChildren().Cast<DisplayCraftPart>();
	
	public override void _Ready()
	{
		_partsContainer = GetNode<Control>("%PartsContainer");
		_core = GetNode<DisplayCraftPart>("%Core");
		Clear();
	}

	private void Clear()
	{
		_partsContainer.QueueFreeChildren();
	}

	private Blueprint GetBlueprint()
	{
		return new Blueprint
		{
			id = Assets.Instance.GenerateUuid(),
			core = _core.Blueprint,
			hulls = Parts.Where(displayCraftPart => displayCraftPart != _core && !displayCraftPart.IsQueuedForDeletion()) // TODO: Pretty weird to check if it's queued for deletion here
				.Select(displayCraftPart => displayCraftPart.Blueprint)
				.ToList()
		};
	}

	private void SetBlueprint(Blueprint blueprint)
	{
		Clear();

		foreach (BlueprintPart blueprintPart in blueprint.hulls)
		{
			AddPart(blueprintPart);
		}

		SerCoreBlueprint(blueprint.core);
	}

	public DisplayCraftPart AddPart(BlueprintPart blueprintPart)
	{
		DisplayCraftPart part = displayCraftPartScene.Instantiate<DisplayCraftPart>();
		
		_partsContainer.AddChild(part);

		part.Color = Color;
		part.Blueprint = blueprintPart;

		return part;
	}

	public DisplayCraftPart SerCoreBlueprint(BlueprintPart blueprintPart)
	{
		_core.Blueprint = blueprintPart;
		return _core;
	}

	private void UpdateColor()
	{
		_core.Color = _color;
		foreach (DisplayCraftPart displayPart in Parts)
		{
			displayPart.Color = _color;
		}
	}

}
