﻿using System;
using System.Collections.Generic;
using Godot;

namespace CtrlRaul.Interfaces;

public interface IItemsList<out TListItem, in TValue> where TListItem : IListItem<TValue>
{
    [Export] public PackedScene listItemScene { get; }
    public Control listItemsContainer { get; }
    public Control emptyTextLabel { get; }
    public IEnumerable<TListItem> AddItems(IEnumerable<TValue> values);
    public TListItem AddItem(TValue value);
    public IEnumerable<TListItem> GetItems();
    public TListItem GetItem(TValue value);
    public void Clear();
}