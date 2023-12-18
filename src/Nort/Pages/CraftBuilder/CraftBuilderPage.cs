using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using CtrlRaul.Godot;
using CtrlRaul.Godot.Linq;
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
    
    private readonly Vector2 gridSnap = Vector2.One * 16.0f;
    private const float ZoomStep = 0.1f;
    private const float ZoomMin = 0.5f;
    private const float ZoomMax = 1.0f;

    [Export] private PackedScene blueprintSelectorPopupScene;
    [Export] private PackedScene coresListItemScene;

    [Ready] public SubViewport subViewport;
    [Ready] public Area2D mouseArea;
    [Ready] public Area2D coreArea;
    [Ready] public Node2D partAreas;
    [Ready] public Camera2D camera;

    [Ready] public Control canvasManipulationHitBox;
    [Ready] public DragReceiver dragReceiver;
    [Ready] public Control canvas;
    [Ready] public Control hoveredPartOutline;
    [Ready] public TextureRect hoveredPartOutlineSprite;
    [Ready] public PartTransformControls partTransformControls;
    [Ready] public LineEdit blueprintIdInput;
    [Ready] public DisplayCraft displayCraft;
    [Ready] public PartsInventory partsInventory;
    [Ready] public CraftSummary craftSummary;
    [Ready] public PartInspector partInspector;
    [Ready] public HBoxContainer blueprintButtons;
    [Ready] public Control coresListContainer;
    [Ready] public Control coresList;
    [Ready] public DraggedPartPreview draggedPartPreview;
    [Ready] public DragReceiver partsInventoryDragReceiver;

    private Vector2 dragOffset = Vector2.Zero;
    private DisplayCraftPart draggedPart;
    private DisplayCraftPart hoveredPart;
    private DisplayCraftPart selectedPart;
    private readonly Dictionary<object, Area2D> areaForPart = new();
    private readonly Dictionary<object, DisplayCraftPart> partForArea = new();
    private bool editorMode;
    private bool panning;
    private bool mouseDownOnCanvasManipulationHitBox;

    private Color color = Config.FactionlessColor;
    private Color Color
    {
        get => color;
        set
        {
            displayCraft.Color = value;
            partsInventory.Color = value;
            partInspector.Color = value;
            draggedPartPreview.Color = value;

            foreach (CoresListItem item in coresList.GetChildren().Cast<CoresListItem>())
            {
                item.Color = value;
            }

            color = value;
        }
    }

    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();

        displayCraft.BlueprintChanged += craftSummary.SetBlueprint;
        dragReceiver.DragEntered += OnDragReceiverDragEntered;
        partsInventory.PartHovered += partInspector.SetPartData;
        partsInventoryDragReceiver.DragEntered += OnPartsInventoryDragReceiverDragEntered;
        partsInventoryDragReceiver.DragExited += OnPartsInventoryDragReceiverDragExited;
        partsInventoryDragReceiver.DragDrop += OnPartsInventoryDragReceiverDragDrop;
        partTransformControls.Rotate += OnPartTransformControlsRotate;
        partTransformControls.Flip += OnPartTransformControlsFlip;

        DragManager.Instance.DragStop += OnDragManagerDragStop;
        
        GetTree().Root.SizeChanged += UpdateSupViewportSize;
        UpdateSupViewportSize();
    }
    
    public override void _ExitTree()
    {
        base._ExitTree();
        GetTree().Root.SizeChanged -= UpdateSupViewportSize;
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
                GetAreaForPart(draggedPart).Position = place;
            }
            else if (panning)
            {
                canvas.Position += mouseMotionEvent.Relative;
                UpdateCamera();
                partTransformControls.UpdateTransform(canvas);
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

        editorMode = Game.Instance.Dev || PagesNavigator.Instance.NavigationData is NavigationData { FromEditor: true };

        if (editorMode)
        {
            blueprintIdInput.Visible = true;
            blueprintButtons.Visible = true;
            SetBlueprint(Assets.Instance.InitialBlueprint);
            Color = Assets.Instance.DefaultEnemyFaction.Color;
        }
        else
        {
            blueprintIdInput.Visible = false;
            blueprintButtons.Visible = false;
            SetBlueprint(Game.Instance.CurrentPlayer.blueprint);
            Color = Assets.Instance.PlayerFaction.Color;
        }
    }
    
    private void AddCoreButton(PartData partData)
    {
        CoresListItem item = coresListItemScene.Instantiate<CoresListItem>();
        BlueprintPart blueprint = BlueprintPart.From(partData);

        coresList.AddChild(item);

        item.PartData = partData;
        item.Color = Color;
        item.Pressed += () => SetCoreBlueprint(blueprint);
        item.MouseEntered += () => partInspector.SetPartData(partData);
    }

    private void SetCoreBlueprint(BlueprintPart blueprint)
    {
        Vector2 corePosition = displayCraft.Core.Position;
        
        displayCraft.SetCoreBlueprint(blueprint);
        displayCraft.Core.Position = corePosition;
        
        if (hoveredPart == displayCraft.Core)
            SetHoveredPartOutline(blueprint);

        if (partTransformControls.Part == displayCraft.Core)
            partTransformControls.Part = displayCraft.Core;

        craftSummary.SetBlueprint(displayCraft.Blueprint);
    }

    private void SetBlueprint(Blueprint blueprint)
    {
        Clear();

        blueprintIdInput.Text = blueprint.id;

        foreach (BlueprintPart part in blueprint.hulls)
        {
            AddPart(part);
        }

        if (Game.Instance.CurrentPlayer != null)
        {
            displayCraft.SetCoreBlueprint(blueprint.core); // This way it's not editable
        }
        else
        {
            AddPart(blueprint.core, true);
        }

        partInspector.SetPart(displayCraft.Core);
        //partsInventory.SetBlueprint(blueprint);
        craftSummary.SetBlueprint(displayCraft.Blueprint);
    }

    private void SetHoveredPartOutline(BlueprintPart blueprint)
    {
        Texture2D texture = Assets.Instance.GetPartTexture(blueprint);
        Vector2 textureSize = texture.GetSize();

        hoveredPartOutlineSprite.Texture = texture;
        hoveredPartOutlineSprite.Size = textureSize;
        hoveredPartOutlineSprite.PivotOffset = textureSize * 0.5f;
        hoveredPartOutlineSprite.Position = -textureSize * 0.5f;
        hoveredPartOutlineSprite.FlipH = blueprint.flipped;
    }

    private void Clear()
    {
        canvas.Scale = Vector2.One * 0.5f;
        blueprintIdInput.Text = "";
        hoveredPartOutline.Visible = false;

        areaForPart.Clear();
        partForArea.Clear();
        partTransformControls.Clear();
        displayCraft.Clear();
        partInspector.Clear();
        craftSummary.Clear();
        Stage.Instance.Clear();
        
        coresList.QueueFreeChildren();
        partAreas.QueueFreeChildren();

        RepopulateInventory();
        UpdateCamera();
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

        partsInventory.Clear();
        partsInventory.AddParts(hulls);

        coresList.QueueFreeChildren();

        foreach (PartData core in cores)
        {
            AddCoreButton(core);
        }

        coresListContainer.Visible = cores.Count > 1;
    }

    private void Zoom(int delta)
    {
        float change = canvas.Scale.X + delta * ZoomStep * canvas.Scale.X;
        Vector2 newZoom = Vector2.One * Mathf.Clamp(change, ZoomMin, ZoomMax);

        if (newZoom == canvas.Scale)
        {
            return;
        }

        Vector2 localMouse = canvas.GetLocalMousePosition();

        canvas.Scale = newZoom;
        canvas.Position -= localMouse * canvas.Scale * ZoomStep * delta;

        UpdateCamera();
        UpdateHoveredPart();
        partTransformControls.UpdateTransform(canvas);
    }

    private void UpdateCamera()
    {
        camera.Zoom = canvas.Scale;
        camera.Position = -canvas.Position / canvas.Scale;
    }

    private DisplayCraftPart AddPart(BlueprintPart blueprint, bool core = false)
    {
        DisplayCraftPart part = core ? displayCraft.SetCoreBlueprint(blueprint) : displayCraft.AddPart(blueprint);
        Area2D area = new();
        CollisionShape2D shape = new();

        area.Position = blueprint.Place;
        area.RotationDegrees = blueprint.angle;
        area.CollisionLayer = PhysicsLayer.Get("mouse_interactable");
        shape.Shape = new RectangleShape2D
        {
            Size = Assets.Instance.GetPartTexture(blueprint.partId).GetSize()
        };

        area.AddChild(shape);
        partAreas.AddChild(area);

        areaForPart[part.GetInstanceId()] = area;
        partForArea[area.GetInstanceId()] = part;

        return part;
    }

    private void AddAndDragPart(BlueprintPart blueprint)
    {
        draggedPart = AddPart(blueprint);
        hoveredPartOutline.Visible = false;
        craftSummary.SetBlueprint(displayCraft.Blueprint);
    }

    private void RemovePart(DisplayCraftPart part)
    {
        if (part == draggedPart)
            draggedPart = null;

        Area2D area = GetAreaForPart(part);

        partForArea.Remove(area);
        areaForPart.Remove(part);

        area.QueueFree();
        part.QueueFree();

        craftSummary.SetBlueprint(displayCraft.Blueprint);
    }

    private void UpdateHoveredPart()
    {
        mouseArea.Position = canvas.GetLocalMousePosition();
        
        Godot.Collections.Array<Area2D> areas = mouseArea.GetOverlappingAreas();

        hoveredPart = areas.Count switch
        {
            0 => null,
            1 => GetPartForArea(areas[0]),
            _ => GetPartForArea(areas.FindNearest(mouseArea.Position))
        };

        if (hoveredPart == null)
        {
            hoveredPartOutline.Visible = false;
        }
        else
        {
            hoveredPartOutline.Visible = true;
            hoveredPartOutline.Position = hoveredPart.Position;
            hoveredPartOutline.RotationDegrees = hoveredPart.Angle;
            SetHoveredPartOutline(hoveredPart.Blueprint);
            partInspector.SetPart(hoveredPart);
        }
    }

    // ReSharper disable once SuggestBaseTypeForParameter
    private Area2D GetAreaForPart(DisplayCraftPart part)
    {
        return areaForPart.TryGetValue(part.GetInstanceId(), out Area2D area) ? area : null;
    }
    
    // ReSharper disable once SuggestBaseTypeForParameter
    private DisplayCraftPart GetPartForArea(Area2D area)
    {
        return partForArea.TryGetValue(area.GetInstanceId(), out DisplayCraftPart part) ? part : null;
    }

    private Vector2 GetSnappedMousePosition()
    {
        return (canvas.GetLocalMousePosition() - dragOffset).Snapped(gridSnap);
    }

    private void UpdateSupViewportSize()
    {
        subViewport.Size = GetTree().Root.Size;
    }

    
    #region EventHandlers

    private void OnPartsInventoryPartsListItemHovered(PartsListItem partsListItem)
    {
        partInspector.SetPartData(partsListItem.Value);
    }

    private void OnPartTransformControlsRotate(float angle)
    {
        hoveredPartOutline.Visible = false;
        GetAreaForPart(partTransformControls.Part).RotationDegrees = angle;
    }

    private void OnPartTransformControlsFlip(bool flipped)
    {
        hoveredPartOutline.Visible = false;
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
            
        Blueprint blueprint = displayCraft.Blueprint;
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
            Game.Instance.CurrentPlayer.blueprint = displayCraft.Blueprint;
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
        craftSummary.SetBlueprint(displayCraft.Blueprint);
    }

    private void OnDragManagerDragStop(DragData dragData)
    {
        partTransformControls.Part = draggedPart;
        partTransformControls.UpdateTransform(canvas);
        draggedPart = null;

    }

    private void OnPartsInventoryDragReceiverDragEntered()
    {
        if (draggedPart != null)
        {
            draggedPartPreview.PartData = draggedPart.partData;
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
                            panning = false;
                            if (hoveredPart != null)
                            {
                                partTransformControls.Part = hoveredPart;
                                partTransformControls.UpdateTransform(canvas);
                                displayCraft.MovePartToTop(hoveredPart);
                            }
                            else
                            {
                                partTransformControls.Clear();
                            }
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
                    hoveredPartOutline.Hide();
                    DragManager.Instance.Drag(canvasManipulationHitBox, draggedPart.partData);
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