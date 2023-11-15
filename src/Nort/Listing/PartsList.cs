using Godot;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using Nort.Popups;

namespace Nort.Listing;

public partial class PartsList : MarginContainer
{
	public event Action<PartData> PartPicked;
	public event Action PartStored;
	public event Action<PartData> PartHovered;

	[Export] private PackedScene partBuilderPopupScene;
	[Export] private PackedScene partsListItemScene;

	private Control partsContainer;
	private Label emptyTextLabel;
	private Button addPartButton;

	private Color _color;

	public Color Color
	{
		get => _color;
		set
		{
			foreach (PartsListItem listItem in partsContainer.GetChildren().Cast<PartsListItem>())
			{
				listItem.Color = value;
			}
			_color = value;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		partsContainer = GetNode<Control>("%PartsContainer");
		emptyTextLabel = GetNode<Label>("%EmptyTextLabel");
		addPartButton = GetNode<Button>("%AddPartButton");
		addPartButton.Visible = Game.Instance.Dev;
		Clear();
	}

	public void SetParts(List<PartData> parts)
	{
		emptyTextLabel.Visible = !parts.Any();

		foreach (PartData part in parts)
		{
			AddPartData(part);
		}
	}

	public void AddPartData(PartData partData)
	{
		PartsListItem item = partsListItemScene.Instantiate<PartsListItem>();
		partsContainer.AddChild(item);

		item.SetPart(partData);
		item.Color = Color;
		item.Picked += () => OnItemPicked(item);
		item.MouseEntered += () => OnItemMouseEntered(item);
	}

	public void SetBlueprint(Blueprint blueprint)
	{
		foreach (BlueprintPart blueprintPart in blueprint.hulls)
		{
			foreach (PartsListItem listItem in partsContainer.GetChildren().Cast<PartsListItem>())
			{
				if (PartData.SameKind(listItem.PartData, PartData.From(blueprintPart)))
				{
					listItem.Count -= 1;
				}
			}
		}
	}

	public void Clear()
	{
		foreach (Node item in partsContainer.GetChildren())
		{
			partsContainer.RemoveChild(item);
			item.QueueFree();
		}
	}

	private void OnItemPicked(PartsListItem item)
	{
		item.Count -= 1;
		PartPicked?.Invoke(item.PartData);
	}

	private void OnItemMouseEntered(Control item)
	{
		PartHovered?.Invoke(((PartsListItem)item).PartData);
	}

	private void OnDragReceiverReceived(Control source, RefCounted part)
	{
		PartData partData = null;

		if (part is BlueprintPart blueprintPart)
		{
			partData = PartData.From(blueprintPart);
		}
		else if (part is PartData data)
		{
			partData = data;
		}

		partData.ShouldNotBeNull();

		bool stored = false;

		foreach (PartsListItem listItem in partsContainer.GetChildren().Cast<PartsListItem>())
		{
			if (PartData.SameKind(listItem.PartData, partData))
			{
				listItem.Count += 1;
				stored = true;
			}
		}

		if (!stored && Game.Instance.Dev)
		{
			AddPartData(partData);
		}

		PartStored?.Invoke();
	}

	private void OnAddPartButtonPressed()
	{
		PartBuilderPopup popup = PopupsManager.Instance.Custom<PartBuilderPopup>(partBuilderPopupScene);
		popup.PartBuilt += OnPartBuilt;
	}

	private void OnPartBuilt(PartData partData)
	{
		AddPartData(partData);
	}
}
