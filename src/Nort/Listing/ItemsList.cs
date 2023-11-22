
// Classes with type parameters like this seem to break asset unloading in godot, so don't...

/*using System;
using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Interfaces;
using Godot;

namespace Nort.Listing;



public partial class ItemsList<TListItem, TValue> : Control, IItemsList<TListItem, TValue> where TListItem : class, IListItem<TValue>
{
    [Export] public PackedScene ListItemScene { get; private set; }

    public Control ListItemsContainer { get; private set; }
    public Control EmptyTextLabel { get; private set; }

    private readonly Dictionary<TValue, TListItem> valueToListItem = new();

    public override void _Ready()
    {
        base._Ready();
        ListItemsContainer = GetNode<Control>("%ListItemsContainer");
        EmptyTextLabel = GetNode<Control>("%EmptyTextLabel");
        Clear();
    }

    public IEnumerable<TListItem> GetItems()
    {
        return ListItemsContainer.GetChildren().Cast<TListItem>();
    }

    public TListItem GetItem(TValue value)
    {
        return valueToListItem.TryGetValue(value, out TListItem listItem) ? listItem : null;
    }

    public IEnumerable<TListItem> AddItems(IEnumerable<TValue> values)
    {
        return values.Select(AddItem);
    }

    public TListItem AddItem(TValue value)
    {
        EmptyTextLabel.Visible = false;
        TListItem listItem = ListItemScene.Instantiate<TListItem>();
        ListItemsContainer.AddChild(listItem as Node);
        listItem.SetFor(value);
        valueToListItem.Add(value, listItem);
        return listItem;
    }

    public bool RemoveItem(TValue value)
    {
        Node listItem = (valueToListItem[value] as Node)!;
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
}*/