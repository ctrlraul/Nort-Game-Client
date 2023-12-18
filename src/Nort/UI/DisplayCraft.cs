using System;
using Godot;
using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot;
using CtrlRaul.Godot.Linq;

namespace Nort.UI;

public partial class DisplayCraft : Control
{
	public event Action<Blueprint> BlueprintChanged;  

	[Export] private PackedScene displayCraftPartScene;
	
	[Ready] public Control partsContainer;
	[Ready] public DisplayCraftPart Core { get; private set; }

	private Color color;
	public Color Color
	{
		get => color;
		set
		{
			color = value;
			if (IsInsideTree())
				UpdateColor();
		}
	}

	public Blueprint Blueprint
	{
		get
		{
			return new Blueprint
			{
				id = Assets.GenerateUuid(),
				core = Core.Blueprint,
				hulls = Parts.Where(displayCraftPart => displayCraftPart != Core && !displayCraftPart.IsQueuedForDeletion()) // TODO: Pretty weird to check if it's queued for deletion here
					.Select(displayCraftPart => displayCraftPart.Blueprint)
					.ToList()
			};
		}
		set
		{
			Clear();
			
			foreach (BlueprintPart blueprintPart in value.hulls)
				AddPart(blueprintPart);
			
			SetCoreBlueprint(value.core);
			BlueprintChanged?.Invoke(value);
		}
	}

	public IEnumerable<DisplayCraftPart> Parts => partsContainer.GetChildren().Cast<DisplayCraftPart>();
	
	public override void _Ready()
	{
		base._Ready();
		this.InitializeReady();
		Clear();
	}

	public void Clear()
	{
		partsContainer.QueueFreeChildren();
	}

	public DisplayCraftPart AddPart(BlueprintPart blueprintPart)
	{
		DisplayCraftPart part = displayCraftPartScene.Instantiate<DisplayCraftPart>();
		
		partsContainer.AddChild(part);

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
		Core.Color = color;
		foreach (DisplayCraftPart displayPart in Parts)
		{
			displayPart.Color = color;
		}
	}

	public void MovePartToTop(DisplayCraftPart part)
	{
		if (partsContainer.GetChildCount() > 1)
			partsContainer.MoveChild(part, -1);
	}
}
