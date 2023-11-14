using Godot;
using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot.Linq;

namespace Nort.UI;

public partial class DisplayCraft : Control
{
	[Export] private PackedScene displayCraftPartScene;
	
	private Control _partsContainer;

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
	
	public DisplayCraftPart Core { get; private set; }

	public Blueprint Blueprint
	{
		get => GetBlueprint();
		set => SetBlueprint(value);
	}

	public IEnumerable<DisplayCraftPart> Parts => _partsContainer.GetChildren().Cast<DisplayCraftPart>();
	
	public override void _Ready()
	{
		_partsContainer = GetNode<Control>("%PartsContainer");
		Core = GetNode<DisplayCraftPart>("%Core");
		Clear();
	}

	public void Clear()
	{
		_partsContainer.QueueFreeChildren();
	}

	private Blueprint GetBlueprint()
	{
		return new Blueprint
		{
			id = Assets.Instance.GenerateUuid(),
			core = Core.Blueprint,
			hulls = Parts.Where(displayCraftPart => displayCraftPart != Core && !displayCraftPart.IsQueuedForDeletion()) // TODO: Pretty weird to check if it's queued for deletion here
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

		SetCoreBlueprint(blueprint.core);
	}

	public DisplayCraftPart AddPart(BlueprintPart blueprintPart)
	{
		DisplayCraftPart part = displayCraftPartScene.Instantiate<DisplayCraftPart>();
		
		_partsContainer.AddChild(part);

		part.Color = Color;
		part.Blueprint = blueprintPart;

		return part;
	}

	public DisplayCraftPart SetCoreBlueprint(BlueprintPart blueprintPart)
	{
		Core.Blueprint = blueprintPart;
		return Core;
	}

	private void UpdateColor()
	{
		Core.Color = _color;
		foreach (DisplayCraftPart displayPart in Parts)
		{
			displayPart.Color = _color;
		}
	}

}
