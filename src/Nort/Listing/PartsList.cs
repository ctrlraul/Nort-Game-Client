using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot;
using CtrlRaul.Interfaces;
using Godot;

namespace Nort.Listing;

public partial class PartsList : MarginContainer, IItemsList<PartsListItem, PartData>
{
	[Export] public PackedScene ListItemScene { get; private set; }

	[Ready("%ListItemsContainer")] public Control ListItemsContainer { get; private set; }
	[Ready("%EmptyTextLabel")] public Control EmptyTextLabel { get; private set; }

	private readonly Dictionary<PartData, PartsListItem> valueToListItem = new();

	
	private Color _color;

	public Color Color
	{
		get => _color;
		set
		{
			_color = value;
			foreach (PartsListItem listItem in GetItems())
				listItem.Color = _color;
		}
	}
	
	public override void _Ready()
	{
		base._Ready();
		this.InitializeReady();
		Clear();
	}

	public void SetBlueprint(Blueprint blueprint)
	{
		foreach (PartsListItem listItem in blueprint.hulls.Select(PartData.From).Select(GetItem))
		{
			listItem.Count -= 1;
		}
	}
	
	public IEnumerable<PartsListItem> GetItems()
	{
		return ListItemsContainer.GetChildren().Cast<PartsListItem>();
	}

	public PartsListItem GetItem(PartData value)
	{
		return valueToListItem.TryGetValue(value, out PartsListItem listItem) ? listItem : null;
	}

	public IEnumerable<PartsListItem> AddItems(IEnumerable<PartData> values)
	{
		return values.Select(AddItem);
	}

	public PartsListItem AddItem(PartData value)
	{
		EmptyTextLabel.Visible = false;
		PartsListItem listItem = ListItemScene.Instantiate<PartsListItem>();
		ListItemsContainer.AddChild(listItem);
		listItem.SetFor(value);
		valueToListItem.Add(value, listItem);
		return listItem;
	}

	public bool RemoveItem(PartData value)
	{
		PartsListItem listItem = valueToListItem[value];
		listItem.QueueFree();
		ListItemsContainer.RemoveChild(listItem);
		if (ListItemsContainer.GetChildCount() == 0)
			EmptyTextLabel.Visible = true;
		return valueToListItem.Remove(value);
	}

	public void Clear()
	{
		EmptyTextLabel.Visible = true;
		foreach (Node child in ListItemsContainer.GetChildren())
		{
			child.QueueFree();
		}
	}
}
