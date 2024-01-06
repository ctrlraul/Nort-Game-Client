using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using CtrlRaul.Godot;
using CtrlRaul.Godot.Linq;
using Nort.Entities;
using Nort.UI;
using Nort.Listing;
using Nort.Popups;

namespace Nort.Pages.CraftBuilder;

public partial class CraftBuilderPage : Page
{
    public class NavigationData
    {
        public bool FromEditor { get; }

        public NavigationData(bool fromEditor)
        {
            FromEditor = fromEditor;
        }
    }
    
    private readonly Vector2 gridSnap = Vector2.One * 8.0f;
    private const float ZoomStep = 0.1f;
    private const float ZoomMin = 0.5f;
    private const float ZoomMax = 1.5f;

    [Export] private PackedScene blueprintSelectorPopupScene;
    [Export] private PackedScene coresListItemScene;
    
    [Ready] public Control buildButton;
    [Ready] public Control canvasManipulationHitBox;
    [Ready] public DragReceiver dragReceiver;
    
    [Ready] public PartTransformControls partTransformControls;
    [Ready] public LineEdit blueprintIdInput;

    [Ready] public PartsInventory partsInventory;
    [Ready] public CraftSummary craftSummary;
    [Ready] public PartInspector partInspector;
    [Ready] public HBoxContainer blueprintButtons;
    [Ready] public Control coresListContainer;
    [Ready] public Control coresList;
    [Ready] public DraggedPartPreview draggedPartPreview;
    [Ready] public DragReceiver partsInventoryDragReceiver;
    [Ready] public PartOutline partOutline;

    private ulong lastZoomTime;
    private Vector2 dragOffset = Vector2.Zero;
    private CraftPart draggedPart;
    private CraftPart hoveredPart;

    private CraftPart selectedPart;

    private bool editorMode;
    private bool panning;
    private bool mouseDownOnCanvasManipulationHitBox;

    private Craft craft;

    private Faction faction;

    private Faction Faction
    {
        get => faction;
        set
        {
            faction = value;

            craft.Faction = faction;
            partsInventory.partsList.Faction = faction;
            partInspector.Faction = faction;
            draggedPartPreview.Faction = faction;

            foreach (CoresListItem item in coresList.GetChildren().Cast<CoresListItem>())
                item.Faction = faction;
        }
    }
    

    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();

        dragReceiver.DragEntered += OnDragReceiverDragEntered;
        partsInventory.PartDataDetailsRequested += partInspector.SetPartData;
        partsInventoryDragReceiver.DragEntered += OnPartsInventoryDragReceiverDragEntered;
        partsInventoryDragReceiver.DragExited += OnPartsInventoryDragReceiverDragExited;
        partsInventoryDragReceiver.DragDrop += OnPartsInventoryDragReceiverDragDrop;
        partTransformControls.Rotate += OnPartTransformControlsRotate;
        partTransformControls.Flip += OnPartTransformControlsFlip;

        DragManager.Instance.DragStop += OnDragManagerDragStop;
    }
    
    public override void _ExitTree()
    {
        base._ExitTree();
        DragManager.Instance.DragStop -= OnDragManagerDragStop;
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventMouseMotion mouseMotionEvent)
        {
            if (draggedPart != null)
            {
                Vector2 place = GetSnappedMousePosition();
                draggedPart.Position = place;
            }
            else if (panning)
            {
                Stage.Instance.camera.Position -= mouseMotionEvent.Relative / Stage.Instance.camera.Zoom;
                partTransformControls.UpdateTransform(Stage.Instance.camera);
            }
            else
            {
                UpdateHoveredPart();
            }
        }
    }

    public override async Task Initialize()
    {
        await Game.Instance.Initialize();

        RemoveChild(partOutline);
        Stage.Instance.AddCustomEffect(partOutline);

        craft = Stage.Instance.AddEntity<Craft>();

        editorMode = Game.Instance.Dev || PagesNavigator.Instance.NavigationData is NavigationData { FromEditor: true };

        if (editorMode)
        {
            buildButton.Visible = false;
            blueprintIdInput.Visible = true;
            blueprintButtons.Visible = true;
            Faction = Assets.Instance.DefaultEnemyFaction;
            SetBlueprint(Assets.Instance.InitialBlueprint);
        }
        else
        {
            buildButton.Visible = true;
            blueprintIdInput.Visible = false;
            blueprintButtons.Visible = false;
            Faction = Assets.Instance.PlayerFaction;
            SetBlueprint(Game.Instance.CurrentPlayer.blueprint);
        }

        craft.StatsChanged += UpdateCraftSummary;
    }

    private void UpdateCraftSummary()
    {
        craftSummary.SetStats(craft.Stats);
    }
    
    private void AddCoreButton(PartData partData)
    {
        CoresListItem item = coresListItemScene.Instantiate<CoresListItem>();
        BlueprintPart blueprint = BlueprintPart.From(partData);

        coresList.AddChild(item);

        item.PartData = partData;
        item.Faction = Faction;
        item.Pressed += () => SetCoreBlueprint(blueprint);
        item.MouseEntered += () => partInspector.SetPartData(partData);
    }

    private void SetCoreBlueprint(BlueprintPart blueprint)
    {
        Vector2 corePosition = craft.CorePart?.Position ?? Vector2.Zero;

        craft.SetCoreBlueprint(blueprint);
        craft.CorePart!.Position = corePosition;

        if (hoveredPart == craft.CorePart)
            partOutline.SetPart(craft.CorePart);

        if (partTransformControls.Part == craft.CorePart)
            partTransformControls.Part = craft.CorePart;
    }

    private void SetBlueprint(Blueprint blueprint)
    {
        Clear();

        blueprintIdInput.Text = blueprint.id;

        foreach (BlueprintPart part in blueprint.hulls)
        {
            if (editorMode || partsInventory.TryTakingPart(PartData.From(part)))
                craft.AddPart(part);
        }

        craft.SetCoreBlueprint(blueprint.core);

        // if (editorMode)
        // {
        //     AddPart(blueprint.core, true);
        // }
        // else
        // {
        //     displayCraft.SetCoreBlueprint(blueprint.core); // This way it's not editable
        // }

        partInspector.SetPartData(PartData.From(blueprint.core));
    }

    private void Clear()
    {
        Stage.Instance.camera.Zoom = Vector2.One;
        blueprintIdInput.Text = "";
        partOutline.Hide();

        partTransformControls.Clear();
        craft.ClearParts();
        partInspector.Clear();
        craftSummary.Clear();
        
        coresList.QueueFreeChildren();

        RepopulateInventory();
    }

    private void RepopulateInventory()
    {
        List<PartData> hulls;
        List<PartData> cores;

        if (editorMode)
        {
            hulls = Assets.Instance.GetHullParts().Select(PartData.From).ToList();
            cores = Assets.Instance.GetCoreParts().Select(PartData.From).ToList();
        }
        else
        {
            hulls = Game.Instance.CurrentPlayer.Hulls.ToList();
            cores = Game.Instance.CurrentPlayer.Cores.ToList();
        }

        partsInventory.partsList.Clear();
        partsInventory.partsList.Add(hulls);

        coresList.QueueFreeChildren();

        foreach (PartData core in cores)
        {
            AddCoreButton(core);
        }

        coresListContainer.Visible = cores.Count > 1;
    }
    
    private void Zoom(int delta)
    {
        // Prevent double input

        ulong now = Time.GetTicksMsec();

        if (now - lastZoomTime < 3) // 3 is arbitrary, just seemed to work well for me
            return;

        lastZoomTime = now;


        // Actual zooming code

        Vector2 mouse = Stage.Instance.camera.GetLocalMousePosition();
        Vector2 oldZoom = Stage.Instance.camera.Zoom;
        Vector2 zoomAmount = oldZoom * ZoomStep * delta;
        Vector2 motion = mouse * zoomAmount / (oldZoom + zoomAmount);

        Stage.Instance.camera.Zoom += zoomAmount;
        Stage.Instance.camera.Position += motion;
        partTransformControls.UpdateTransform(Stage.Instance.camera);
    }

    private CraftPart AddPart(BlueprintPart blueprint, bool core = false)
    {
        return core
            ? craft.SetCoreBlueprint(blueprint)
            : craft.AddPart(blueprint);
    }

    private void AddAndDragPart(BlueprintPart blueprint)
    {
        draggedPart = AddPart(blueprint);
        partOutline.Hide();
    }

    private void RemovePart(CraftPart part)
    {
        if (part == draggedPart)
            draggedPart = null;

        part.QueueFree();

        UpdateCraftSummary();
    }

    private void UpdateHoveredPart()
    {
        hoveredPart = Stage.Instance.GetCraftPartOnMouse();
    
        if (hoveredPart == null)
        {
            partOutline.Hide();
        }
        else
        {
            partOutline.SetPart(hoveredPart);
            partInspector.SetPartData(PartData.From(hoveredPart.Blueprint));
        }
    }

    private Vector2 GetSnappedMousePosition()
    {
        return (Stage.Instance.GetLocalMousePosition() - dragOffset).Snapped(gridSnap);
    }

    
    #region EventHandlers

    private void OnPartTransformControlsRotate(float angle)
    {
        partOutline.Hide();
    }

    private void OnPartTransformControlsFlip(bool flipped)
    {
        partOutline.Hide();
    }

    private void OnExportButtonPressed()
    {
        if (string.IsNullOrEmpty(blueprintIdInput.Text))
        {
            TextInputPopup popup = PopupsManager.Instance.TextInput("An ID is required!");

            popup.Submitted += text =>
            {
                blueprintIdInput.Text = text;
                OnExportButtonPressed();
            };

            return;
        }

        Blueprint blueprint = Blueprint.From(craft.GetCurrentBlueprint());
        blueprint.id = blueprintIdInput.Text;

        try
        {
            Assets.Instance.StoreBlueprint(blueprint);
        }
        catch (Exception exception)
        {
            string message = $"Failed to export blueprint: {exception.Message}";
            logger.Error(message);
            PopupsManager.Instance.Error(message);
            return;
        }

        // TODO: Use action texts instead of popup for this
        PopupsManager.Instance.Info("Blueprint exported!");
    }
    
    private void OnImportButtonPressed()
    {
        BlueprintSelectorPopup popup = PopupsManager.Instance.Custom<BlueprintSelectorPopup>(blueprintSelectorPopupScene);
        popup.BlueprintSelected += SetBlueprint;
    }

    private async void OnBuildButtonPressed()
    {
        if (Game.Instance.CurrentPlayer != null)
        {
            Game.Instance.CurrentPlayer.blueprint = craft.GetCurrentBlueprint();
            LocalPlayersManager.Instance.StoreLocalPlayer(Game.Instance.CurrentPlayer);
        }

        await PagesNavigator.Instance.GoBack();
    }

    private void OnClearButtonPressed()
    {
        Clear();
    }
    
    private void OnDragReceiverDragEntered()
    {
        draggedPartPreview.Clear();

        if (draggedPart != null)
            return;
        
        BlueprintPart blueprintPart = DragManager.Instance.DragData.data switch
        {
            PartData partData => BlueprintPart.From(partData),
            BlueprintPart blueprintPart2 => blueprintPart2,
            _ => null,
        };

        if (blueprintPart == null)
            return;
        
        AddAndDragPart(blueprintPart);
        UpdateCraftSummary();
    }

    private void OnDragManagerDragStop(DragData dragData)
    {
        partTransformControls.Part = draggedPart;
        partTransformControls.UpdateTransform(Stage.Instance.camera);
        draggedPart = null;

    }

    private void OnPartsInventoryDragReceiverDragEntered()
    {
        if (draggedPart != null)
        {
            draggedPartPreview.PartData = PartData.From(draggedPart.Blueprint);
            RemovePart(draggedPart);
        }
        else switch (DragManager.Instance.DragData.data)
        {
            case PartData partData:
                draggedPartPreview.PartData = partData;
                break;
        
            case BlueprintPart blueprintPart:
                draggedPartPreview.PartData = PartData.From(blueprintPart);
                break;
        }

        dragOffset = Vector2.Zero;
        draggedPartPreview.Show();
    }

    private void OnPartsInventoryDragReceiverDragExited()
    {
        draggedPartPreview.Clear();
    }

    private void OnPartsInventoryDragReceiverDragDrop(DragData dragData)
    {
        if (editorMode)
            return;
        
        switch (dragData.data)
        {
            case PartData partData:
                partsInventory.PutPart(partData);
                break;
            
            case BlueprintPart blueprintPart:
                partsInventory.PutPart(PartData.From(blueprintPart));
                break;
        }
    }

    private void OnCanvasManipulationHitBoxGuiInput(InputEvent inputEvent)
    {
        switch (inputEvent)
        {
            case InputEventMouseButton mouseButtonEvent:
                switch (mouseButtonEvent.ButtonIndex)
                {
                    case MouseButton.Left:
                        mouseDownOnCanvasManipulationHitBox = mouseButtonEvent.Pressed;
                        if (!mouseButtonEvent.Pressed)
                        {
                            if (hoveredPart != null)
                            {
                                partTransformControls.Part = hoveredPart;
                                partTransformControls.UpdateTransform(Stage.Instance.camera);
                                craft.MovePartToTop(hoveredPart);
                            }
                            else if (!panning)
                            {
                                partTransformControls.Clear();
                            }

                            panning = false;
                        }
                        break;
                    
                    case MouseButton.WheelUp:
                        Zoom(1);
                        break;
                    
                    case MouseButton.WheelDown:
                        Zoom(-1);
                        break;
                }
                break;
            
            case InputEventMouseMotion:
                if (!mouseDownOnCanvasManipulationHitBox || panning || DragManager.Instance.DragData != null)
                    return;
            
                if (hoveredPart != null)
                {
                    draggedPart = hoveredPart;
                    dragOffset = draggedPart.GetLocalMousePosition();
                    partTransformControls.Clear();
                    partOutline.Hide();
                    DragManager.Instance.Drag(canvasManipulationHitBox, PartData.From(draggedPart.Blueprint));
                }
                else
                {
                    panning = true;
                }
            
                break;
        }
    }

    #endregion
}