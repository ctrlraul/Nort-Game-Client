using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CtrlRaul.Godot.Linq;
using Nort.UI;
using Nort.Listing;
using System.Linq;
using CtrlRaul.Godot;
using Nort.Popups;
using Nort.Interface;
using Shouldly;

namespace Nort.Pages.CraftBuilder;

public partial class CraftBuilderPage : Page
{
    private enum MouseAction
    {
        None,
        MouseDown,
        Panning,
        DraggingPart,
    }
    
    private readonly Vector2 gridSnap = Vector2.One * 16.0f;
    private const float ZoomStep = 0.1f;
    private const float ZoomMin = 0.5f;
    private const float ZoomMax = 1.0f;

    [Export] private PackedScene BlueprintSelectorPopupScene;
    [Export] private PackedScene CoresListItemScene;

    [Ready("%SubViewport")] public SubViewport subViewport;
    [Ready("%MouseArea")] public Area2D mouseArea;
    [Ready("%CoreArea")] public Area2D coreArea;
    [Ready("%PartAreas")] public Node2D partAreas;
    [Ready("%Camera2D")] public Camera2D camera;

    [Ready("%PartDragArea")] public Control partDragArea;
    [Ready("%OverallDragReceiver")] public DragReceiver overallDragReceiver;
    [Ready("%Canvas")] public Control canvas;
    [Ready("%HoveredPartOutline")] public Control hoveredPartOutline;
    [Ready("%HoveredPartOutlineSprite")] public TextureRect hoveredPartOutlineSprite;
    [Ready("%PartTransformControls")] public PartTransformControls partTransformControls;
    [Ready("%BlueprintIDInput")] public LineEdit blueprintIdInput;
    [Ready("%CraftDisplay")] public DisplayCraft displayCraft;
    [Ready("%PartsInventory")] public PartsInventory partsInventory;
    [Ready("%CraftSummary")] public CraftSummary craftSummary;
    [Ready("%PartInspector")] public PartInspector partInspector;
    [Ready("%BlueprintButtons")] public HBoxContainer blueprintButtons;
    [Ready("%CoresListContainer")] public Control coresListContainer;
    [Ready("%CoresList")] public Control coresList;
    [Ready("%DraggedPartPreview")] public DraggedPartPreview draggedPartPreview;
    [Ready("%PartsInventoryDragReceiver")] public DragReceiver partsInventoryDragReceiver;
    [Ready("%CoresListDragReceiver")] public DragReceiver coresListDragReceiver;

    private Vector2 dragOffset = Vector2.Zero;
    private DisplayCraftPart draggedPart;
    private DisplayCraftPart hoveredPart;
    private DisplayCraftPart selectedPart;
    private readonly Dictionary<object, Area2D> areaForPart = new();
    private readonly Dictionary<object, DisplayCraftPart> partForArea = new();
    private bool editorMode;
    private bool panning;

    private MouseAction mouseAction = MouseAction.None;

    private Color _color = GameConfig.FactionlessColor;
    private Color Color
    {
        get => _color;
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

            _color = value;
        }
    }

    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();

        partsInventory.PartHovered += partData => partInspector.SetPartData(partData);
        partTransformControls.Rotate += OnPartTransformControlsRotate;
        partTransformControls.Flip += OnPartTransformControlsFlip;
        overallDragReceiver.DragEntered += OnOverallDragReceiverDragEntered;
        overallDragReceiver.DragExited += OnOverallDragReceiverDragExited;
        overallDragReceiver.DragDrop += OnOverallDragReceiverDragDrop;
        overallDragReceiver.DragOver += OnOverallReceiverDragOver;
        partsInventoryDragReceiver.DragDrop += OnPartsInventoryDragReceiverDragDrop;
        coresListDragReceiver.DragDrop += OnCoresListDragReceiverDragDrop;

        DragManager.Instance.DragStart += OnDragStart;

        GetTree().Root.SizeChanged += UpdateSupViewportSize;
        UpdateSupViewportSize();
    }

    private void OnDragStart(DragData dragData)
    {
        PartData partData = DragManager.Instance.DragData.data switch
        {
            PartData partData2 => partData2,
            BlueprintPart blueprintPart => PartData.From(blueprintPart.Part),
            _ => null
        };

        if (partData != null)
        {
            draggedPartPreview.PartData = partData;
            draggedPartPreview.Visible = true;
        }
        else
        {
            draggedPartPreview.Clear();
        }
    }

    private void OnOverallDragReceiverDragEntered()
    {
        draggedPartPreview.Visible = false;
        switch (DragManager.Instance.DragData.data)
        {
            case PartData partData:
                AddAndDragPart(BlueprintPart.From(partData));
                break;
            
            case BlueprintPart blueprintPart:
                AddAndDragPart(blueprintPart);
                break;
            
            default:
                logger.Log($"Fuck is this {DragManager.Instance.DragData.data.GetType().Name}");
                break;
        }
    }

    private void OnOverallDragReceiverDragExited()
    {
        
        draggedPartPreview.Show();
        RemovePart(draggedPart);
        draggedPart = null;
    }
    
    private void OnOverallDragReceiverDragDrop(DragData dragData)
    {
        logger.Log("dropped in!");
    }
    
    private void OnOverallReceiverDragOver(InputEventMouseMotion mouseMotionEvent)
    {
        if (draggedPart == null)
        {
            canvas.Position += mouseMotionEvent.Relative;
            UpdateCamera();
            partTransformControls.UpdateTransform(canvas);
        }
        else
        {
            Vector2 place = (canvas.GetLocalMousePosition() - dragOffset).Snapped(gridSnap);
            draggedPart.Position = place;
            GetAreaForPart(draggedPart).Position = place;
        }
    }
    
    public override void _ExitTree()
    {
        base._ExitTree();
        GetTree().Root.SizeChanged -= UpdateSupViewportSize;
    }

    public override async Task Initialize()
    {
        await Game.Instance.Initialize();

        editorMode = Game.Instance.Dev;

        if (editorMode)
        {
            blueprintIdInput.Visible = true;
            blueprintButtons.Visible = true;
            SetBlueprint(Assets.Instance.InitialBlueprint);
            Color = Assets.Instance.EnemyFaction1.Color;
        }
        else
        {
            blueprintIdInput.Visible = false;
            blueprintButtons.Visible = false;
            SetBlueprint(Game.Instance.CurrentPlayer.CurrentBlueprint);
            Color = Assets.Instance.PlayerFaction.Color;
        }
    }
    
    private void AddCoreButton(PartData partData)
    {
        CoresListItem item = CoresListItemScene.Instantiate<CoresListItem>();
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

        if (Game.Instance.Dev)
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

        partTransformControls.CallDeferred("UpdateTransform", canvas);
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
        area.CollisionLayer = 256; // ??? TODO
        area.Rotation = blueprint.angle;
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
        part.ShouldNotBeNull();

        Area2D area = GetAreaForPart(part);

        partForArea.Remove(area);
        areaForPart.Remove(part);

        area.QueueFree();
        part.QueueFree();

        craftSummary.SetBlueprint(displayCraft.Blueprint);
    }

    private void UpdateHoveredPart()
    {
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
            hoveredPartOutline.Rotation = hoveredPart.Angle;
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

    private void OnPartDragAreaMouseButtonEvent(InputEventMouseButton mouseButtonEvent)
    {
        switch (mouseButtonEvent.ButtonIndex)
        {
            case MouseButton.Left:
                if (mouseButtonEvent.Pressed)
                {
                    mouseAction = MouseAction.MouseDown;
                }
                else
                {
                    OnPartDragAreaMouseUp();
                }
                break;

            case MouseButton.WheelUp:
                Zoom(1);
                break;

            case MouseButton.WheelDown:
                Zoom(-1);
                break;
        }
    }

    public void OnPartDragAreaMouseEntered()
    {
        if (draggedPart != null)
        {
            draggedPart.Show();
            draggedPartPreview.Hide();
        }
    }

    public void OnPartDragAreaMouseExited()
    {
        if (draggedPart != null)
        {
            draggedPart.Hide();
            draggedPartPreview.Show();
        }
    }

    private void OnPartDragAreaMouseUp()
    {
        switch (mouseAction)
        {
            case MouseAction.MouseDown:
                partTransformControls.Part = hoveredPart;
                partTransformControls.UpdateTransform(canvas);
                break;

            case MouseAction.Panning:
                // Stopped panning
                break;
                        
            case MouseAction.DraggingPart:
                // if it was dropped into the inventory it's queued for deletion
                if (!draggedPart.IsQueuedForDeletion())
                {
                    draggedPart.Show();
                    partTransformControls.Part = draggedPart;
                    partTransformControls.UpdateTransform(canvas);
                }
                draggedPartPreview.Clear();
                draggedPart = null;
                dragOffset = Vector2.Zero;
                break;

            // default:
            //     throw new ArgumentOutOfRangeException();
        }
        mouseAction = MouseAction.None;
    }

    private void OnPartDragAreaMouseMotionEvent(InputEventMouseMotion mouseMotionEvent)
    {
        switch (mouseAction)
        {
            case MouseAction.None:
                mouseArea.GlobalPosition = mouseArea.GetGlobalMousePosition();
                UpdateHoveredPart();
                break;
            
            case MouseAction.MouseDown:
                if (hoveredPart != null)
                {
                    mouseAction = MouseAction.DraggingPart;
                    draggedPart = hoveredPart;
                    draggedPartPreview.PartData = draggedPart.partData;
                    dragOffset = mouseMotionEvent.Position - hoveredPart.GlobalPosition;
                    partTransformControls.Clear();
                    hoveredPartOutline.Visible = false;
                    DragManager.Instance.Drag(draggedPart, draggedPart.Blueprint);
                }
                else
                {
                    mouseAction = MouseAction.Panning;
                }
                break;
                    
            case MouseAction.Panning:
                canvas.Position += mouseMotionEvent.Relative;
                UpdateCamera();
                partTransformControls.UpdateTransform(canvas);
                break;
            
            case MouseAction.DraggingPart:
                Vector2 place = (canvas.GetLocalMousePosition() - dragOffset).Snapped(gridSnap);
                draggedPart.Position = place;
                GetAreaForPart(draggedPart).Position = place;
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnPartDragAreaGuiInput(InputEvent inputEvent)
    {
        switch (inputEvent)
        {
            case InputEventMouseButton mouseButtonEvent:
                OnPartDragAreaMouseButtonEvent(mouseButtonEvent);
                break;
            
            case InputEventMouseMotion mouseMotionEvent:
                OnPartDragAreaMouseMotionEvent(mouseMotionEvent);
                break;
        }
    }
    
    private void OnPartsInventoryPartsListItemHovered(PartsListItem partsListItem)
    {
        partInspector.SetPartData(partsListItem.Value);
    }

    private void OnPartTransformControlsRotate(float angle)
    {
        hoveredPartOutline.Visible = false;
        GetAreaForPart(partTransformControls.Part).Rotation = angle;
    }

    private void OnPartTransformControlsFlip(bool flipped)
    {
        hoveredPartOutline.Visible = false;
    }

    private void OnExportButtonPressed()
    {
        Blueprint blueprint = displayCraft.Blueprint;

        if (!string.IsNullOrEmpty(blueprintIdInput.Text))
        {
            blueprint.id = blueprintIdInput.Text;
        }

        string path = GameConfig.ConfigPath
            .PathJoin(Assets.BlueprintsDirectoryName)
            .PathJoin(blueprint.id + ".json");
        
        bool success = false;

        try
        {
            blueprint.SaveJson(path);
            Assets.Instance.AddBlueprint(blueprint);
            success = true;
        }
        catch (Exception exception)
        {
            string message = $"Failed to export blueprint: {exception.Message}";
            logger.Error(message);
            PopupsManager.Instance.Error(message);
        }

        if (success)
        {
            logger.Log($"Exported blueprint '{path}'");
            DialogPopup popup = PopupsManager.Instance.Info($"Exported to '{path}'");
            popup.Width = 700;
        }
    }

    private void OnImportButtonPressed()
    {
        BlueprintSelectorPopup popup = PopupsManager.Instance.Custom<BlueprintSelectorPopup>(BlueprintSelectorPopupScene);
        popup.BlueprintSelected += SetBlueprint;
    }

    private void OnPartsInventoryDragReceiverDragDrop(DragData dragData)
    {
        if (dragData.data is not BlueprintPart blueprintPart)
            return;
        
        partsInventory.PutPart(PartData.From(blueprintPart));
        RemovePart(draggedPart);
    }
    
    private void OnCoresListDragReceiverDragDrop(DragData dragData)
    {
        if (dragData.data is PartData partData)
            AddCoreButton(partData);
    }

    private void UpdateSupViewportSize()
    {
        subViewport.Size = GetTree().Root.Size;
    }

    private void OnBuildButtonPressed()
    {
        if (Game.Instance.CurrentPlayer != null)
        {
            Game.Instance.CurrentPlayer.CurrentBlueprint = displayCraft.Blueprint;
            Game.Instance.CurrentPlayer.CurrentBlueprint = Game.Instance.CurrentPlayer.CurrentBlueprint;
            LocalPlayersManager.Instance.StoreLocalPlayer(Game.Instance.CurrentPlayer);
        }

        _ = PagesNavigator.Instance.GoTo(GameConfig.Pages.Lobby);
    }

    private void OnClearButtonPressed()
    {
        Clear();
    }
}