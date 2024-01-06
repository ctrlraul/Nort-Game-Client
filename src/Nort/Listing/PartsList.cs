using System;
using System.Collections.Generic;
using System.Linq;
using CtrlRaul;
using CtrlRaul.Godot;
using CtrlRaul.Godot.Linq;
using Godot;

namespace Nort.Listing;

public partial class PartsList : Control
{
	public event Action<PartsListItem> ListItemAdded;

	[Export] private PackedScene listItemScene;
	[Export] private bool listItemOutlineEnabled;

	[Ready] public Control listItemsContainer;
	[Ready] public Control emptyTextLabel;

	private readonly Dictionary<string, PartsListItem> discriminatorToListItem = new();

	private Faction faction;

	public Faction Faction
	{
		get => faction;
		set
		{
			faction = value;

			foreach (PartsListItem listItem in discriminatorToListItem.Values)
				listItem.Faction = Faction;
		}
	}
	
	
	public override void _Ready()
	{
		base._Ready();
		this.InitializeReady();
		Clear();
	}


	public void Add(PartData partData, int count = 1)
	{
		emptyTextLabel.Visible = false;

		string discriminator = partData.Discriminator;

		if (discriminatorToListItem.TryGetValue(discriminator, out PartsListItem listItem))
		{
			listItem.Count += count;
		}
		else
		{
			PartsListItem newListItem = listItemScene.Instantiate<PartsListItem>();

			listItemsContainer.AddChild(newListItem);

			newListItem.SetOutlineEnabled(listItemOutlineEnabled);
			newListItem.PartData = partData;
			newListItem.Count = count;
			newListItem.Faction = Faction;

			discriminatorToListItem.Add(partData.Discriminator, newListItem);

			ListItemAdded?.Invoke(newListItem);
		}
	}

	public void Add(IEnumerable<PartData> partsData)
	{
		foreach (PartData partData in partsData)
			Add(partData);
	}

	public void Remove(PartData value, int count = 1)
	{
		string discriminator = value.Discriminator;

		if (discriminatorToListItem.TryGetValue(discriminator, out PartsListItem listItem))
		{
			listItem.Count -= count;

			if (listItem.Count <= 0)
			{
				if (listItem.Count < 0)
					Logger.Warn("PartsList", "listItem.Count < 0");

				discriminatorToListItem.Remove(discriminator);
				listItem.QueueFree();

				if (!discriminatorToListItem.Any())
					emptyTextLabel.Visible = true;
			}
		}
		else
		{
			Logger.Warn("PartsList", "Tried to remove unavailable item");
		}
	}

	public bool Has(PartData partData)
	{
		return discriminatorToListItem.ContainsKey(partData.Discriminator);
	}
	
	public void Clear()
	{
		emptyTextLabel.Visible = true;
		discriminatorToListItem.Clear();
		listItemsContainer.QueueFreeChildren();
	}
}
