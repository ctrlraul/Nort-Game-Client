using System;
using System.Collections.Generic;
using Godot;
using Nort.Listing;
using Nort.Popups;

namespace Nort.Pages.CraftBuilder;

public partial class PartsInventory : MarginContainer
{
    public event Action<PartData> PartHovered;
    
    [Export] private PackedScene partBuilderPopupScene;
    
    private PartsList partsList;
    private Button addPartButton;

    public Color Color
    {
        get => partsList.Color;
        set => partsList.Color = value;
    }

    public override void _Ready()
    {
        base._Ready();
        partsList = GetNode<PartsList>("%PartsList");
        addPartButton = GetNode<Button>("%AddPartButton");
        addPartButton.Visible = Game.Instance.Dev;
    }
    
    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        return data.As<DragData>()?.data is BlueprintPart;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        base._DropData(atPosition, data);
        AddPart(PartData.From(data.As<DragData>().data as BlueprintPart));
    }

    public void Clear()
    {
        partsList.Clear();
    }

    public PartsListItem AddPart(PartData partData)
    {
        PartsListItem listItem = partsList.AddItem(partData);
        //listItem.Count = count;
        listItem.Color = Color;
        listItem.MouseEntered += () => PartHovered?.Invoke(partData);
        return listItem;
    }

    public void AddParts(IEnumerable<PartData> partsData)
    {
        foreach (PartData partData in partsData)
            AddPart(partData);
    }

    public void TakePart(PartData partData)
    {
        partsList.GetItem(partData).Count -= 1;
    }

    public void PutPart(PartData partData)
    {
        PartsListItem listItem = partsList.GetItem(partData) ?? AddPart(partData);
        //listItem.Count += 1;
    }

    private void OnAddPartButtonPressed()
    {
        PartBuilderPopup popup = PopupsManager.Instance.Custom<PartBuilderPopup>(partBuilderPopupScene);
        popup.PartBuilt += partData =>
        {
            PartsListItem listItem = partsList.GetItem(partData) ?? AddPart(partData);
            //listItem.Count += 1;
        };
    }
}