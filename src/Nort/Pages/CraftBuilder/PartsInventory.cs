using System;
using CtrlRaul.Godot;
using Godot;
using Nort.Listing;
using Nort.Popups;

namespace Nort.Pages.CraftBuilder;

public partial class PartsInventory : MarginContainer
{
    public event Action<PartData> PartDataDetailsRequested;
    
    [Export] private PackedScene partBuilderPopupScene;

    [Ready] public PartsList partsList;
    [Ready] public Button addPartButton;

    private PartsListItem listItemPressed;
    

    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();
        partsList.ListItemAdded += OnPartsListListItemAdded;
        addPartButton.Visible = Game.Instance.Dev;
    }


    public bool TryTakingPart(PartData partData)
    {
        if (!partsList.Has(partData))
            return false;

        partsList.Remove(partData);

        return true;
    }

    public void PutPart(PartData partData)
    {
        partsList.Add(partData);
    }
    

    private void OnAddPartButtonPressed()
    {
        PartBuilderPopup popup = PopupsManager.Instance.Custom<PartBuilderPopup>(partBuilderPopupScene);
        popup.PartBuilt += partData => partsList.Add(partData);
    }

    private void OnPartsListListItemAdded(PartsListItem partsListItem)
    {
        partsListItem.MouseEntered += () => PartDataDetailsRequested?.Invoke(partsListItem.PartData);
        partsListItem.GuiInput += inputEvent => OnListItemGuiInput(partsListItem, inputEvent);
    }

    private void OnListItemGuiInput(PartsListItem partsListItem, InputEvent inputEvent)
    {
        if (partsListItem.Count <= 0)
            return;

        switch (inputEvent)
        {
            case InputEventMouseButton mouseButton:
                if (mouseButton.ButtonIndex == MouseButton.Left)
                    listItemPressed = mouseButton.Pressed ? partsListItem : null;

                break;

            case InputEventMouseMotion:
                if (listItemPressed != null)
                {
                    DragManager.Instance.Drag(listItemPressed, listItemPressed.PartData);

                    if (!Game.Instance.Dev)
                        partsList.Remove(partsListItem.PartData);
                    
                    listItemPressed = null;
                }

                break;
        }
    }
}