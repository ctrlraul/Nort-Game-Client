using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CtrlRaul.Godot.Linq;
using Nort.UI;
using Nort.Listing;
using System.Linq;
using Nort.Popups;
using Nort.Interface;

namespace Nort.Pages.CraftBuilder;

public partial class CraftBuilderPage : Page
{
    private readonly Vector2 gridSnap = Vector2.One * 16.0f;
    private const float ZoomStep = 0.1f;
    private const float ZoomMin = 0.5f;
    private const float ZoomMax = 1.0f;

    [Export] private PackedScene BlueprintSelectorPopupScene;
    [Export] private PackedScene CoresListItemScene;

    private SubViewport subViewport;
    private Area2D mouseArea;
    private Area2D coreArea;
    private Node2D partAreas;
    private Camera2D camera;

    private Control canvas;
    private Control hoveredPartOutline;
    private TextureRect hoveredPartOutlineSprite;
    private PartTransformControls partTransformControls;
    private LineEdit blueprintIdInput;
    private DisplayCraft displayCraft;
    private PartsList partsList;
    private CraftSummary craftSummary;
    private PartInspector partInspector;
    private HBoxContainer blueprintButtons;
    private DraggedPartPreview draggedPartPreview;
    private Control coresListContainer;
    private Control coresList;

    private Vector2 dragOffset = Vector2.Zero;
    private DisplayCraftPart draggedPart;
    private DisplayCraftPart hoveredPart;
    private DisplayCraftPart selectedPart;
    private readonly Dictionary<object, Area2D> areaForPart = new();
    private readonly Dictionary<object, DisplayCraftPart> partForArea = new();
    private bool editorMode;
    private bool panning;

    private Color _color = GameConfig.FactionlessColor;
    private Color Color
    {
        get => _color;
        set
        {
            displayCraft.Color = value;
            partsList.Color = value;
            partInspector.Color = value;

            draggedPartPreview.SetColor(value);

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

        subViewport = GetNode<SubViewport>("%SubViewport");
        mouseArea = GetNode<Area2D>("%MouseArea");
        coreArea = GetNode<Area2D>("%CoreArea");
        partAreas = GetNode<Node2D>("%PartAreas");
        camera = GetNode<Camera2D>("%Camera2D");
        canvas = GetNode<Control>("%Canvas");
        hoveredPartOutline = GetNode<Control>("%HoveredPartOutline");
        hoveredPartOutlineSprite = GetNode<TextureRect>("%HoveredPartOutlineSprite");
        partTransformControls = GetNode<PartTransformControls>("%PartTransformControls");
        blueprintIdInput = GetNode<LineEdit>("%BlueprintIDInput");
        displayCraft = GetNode<DisplayCraft>("%CraftDisplay");
        partsList = GetNode<PartsList>("%PartsList");
        craftSummary = GetNode<CraftSummary>("%CraftSummary");
        partInspector = GetNode<PartInspector>("%PartInspector");
        blueprintButtons = GetNode<HBoxContainer>("%BlueprintButtons");
        draggedPartPreview = GetNode<DraggedPartPreview>("%DraggedPartPreview");
        coresListContainer = GetNode<Control>("%CoresListContainer");
        coresList = GetNode<Control>("%CoresList");

        GetTree().Root.SizeChanged += UpdateSupViewportSize;
        UpdateSupViewportSize();
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
        displayCraft.SetCoreBlueprint(blueprint);
        
        if (hoveredPart == displayCraft.Core)
        {
            SetHoveredPartOutline(blueprint);
        }
        
        if (partTransformControls.Part == displayCraft.Core)
        {
            partTransformControls.Set("part", displayCraft.Core);
        }
        
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
        partsList.SetBlueprint(blueprint);
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
        draggedPartPreview.Clear();
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

        partsList.Clear();
        partsList.SetParts(hulls);

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
        DisplayCraftPart part;

        if (core)
        {
            part = displayCraft.SetCoreBlueprint(blueprint);
        }
        else
        {
            part = displayCraft.AddPart(blueprint);
        }

        var area = new Area2D();
        var shape = new CollisionShape2D();

        area.Position = blueprint.Place;
        area.CollisionLayer = 256;
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
        if (part == null)
        {
            return;
        }

        Area2D area = GetAreaForPart(part);

        partForArea.Remove(area);
        areaForPart.Remove(part);

        area.QueueFree();
        part.QueueFree();

        craftSummary.SetBlueprint(displayCraft.Blueprint);
    }

    private void UpdateHoveredPart()
    {
        var areas = mouseArea.GetOverlappingAreas();

        switch (areas.Count)
        {
            case 0:
                hoveredPart = null;
                break;

            case 1:
                hoveredPart = GetPartForArea(areas[0]);
                break;

            default:
                hoveredPart = GetPartForArea(areas.FindNearest(mouseArea.Position));
                break;
        }

        if (hoveredPart != null)
        {
            hoveredPartOutline.Visible = true;
            hoveredPartOutline.Position = hoveredPart.Position;
            hoveredPartOutline.Rotation = hoveredPart.Angle;
            SetHoveredPartOutline(hoveredPart.Blueprint);
            partInspector.SetPart(hoveredPart);
        }
        else
        {
            hoveredPartOutline.Visible = false;
        }
    }

    private Area2D GetAreaForPart(DisplayCraftPart part)
    {
        return areaForPart.TryGetValue(part.GetInstanceId(), out var area) ? area : null;
    }


    private DisplayCraftPart GetPartForArea(Area2D area)
    {
        return partForArea.TryGetValue(area.GetInstanceId(), out var part) ? part : null;
    }

    private void Select(InputEventMouseButton eventMouseButton)
    {
        if (!eventMouseButton.Pressed)
        {
            panning = false;
            return;
        }

        if (hoveredPart == null)
        {
            panning = true;
            partTransformControls.Clear();
            hoveredPartOutline.Visible = false;
            return;
        }

        if (hoveredPart == displayCraft.Core)
        {
            partInspector.SetPart(hoveredPart);
            partTransformControls.Part = hoveredPart;
            partTransformControls.UpdateTransform(canvas);
        }
        else
        {
            partTransformControls.Clear();
            dragOffset = eventMouseButton.Position - hoveredPart.GlobalPosition;
            //DragEmitter.Drag(this, hoveredPart.Blueprint); TODO
            RemovePart(hoveredPart);
            hoveredPart = null;
        }
    }

    private void OnPartDragAreaGuiInput(InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouseButton eventMouseButton)
        {
            switch (eventMouseButton.ButtonIndex)
            {
                case MouseButton.Left:
                    Select(eventMouseButton);
                    break;

                case MouseButton.WheelUp:
                    Zoom(1);
                    break;

                case MouseButton.WheelDown:
                    Zoom(-1);
                    break;
            }
        }
        else if (inputEvent is InputEventMouseMotion mouseMotion && panning)
        {
            canvas.Position += mouseMotion.Relative;
            UpdateCamera();
            partTransformControls.CallDeferred("UpdateTransform", canvas);
        }
        else if (draggedPart == null)
        {
            mouseArea.GlobalPosition = mouseArea.GetGlobalMousePosition();
            UpdateHoveredPart();
        }
    }

    private void OnPartsInventoryPartPicked(PartData partData)
    {
        partTransformControls.Clear();
        draggedPartPreview.SetPartData(partData);
    }

    private void OnPartsInventoryPartStored()
    {
        draggedPartPreview.Clear();
    }


    private void OnPartsInventoryPartHovered(PartData partData)
    {
        partInspector.SetPartData(partData);
    }

    private void OnPartDragAreaDragReceiverDragEnter()
    {
        draggedPartPreview.Clear();

        BlueprintPart blueprint;

        switch (DragEmitter.Instance.Data)
        {
            case BlueprintPart:
                blueprint = (BlueprintPart)DragEmitter.Instance.Data;
                break;
            case PartData:
                blueprint = BlueprintPart.From((PartData)DragEmitter.Instance.Data);
                break;
            default:
                return;
        }

        blueprint.Place = (canvas.GetLocalMousePosition() - dragOffset).Snapped(gridSnap);

        // Adding children to the tree on the same frame as the mouse entered the
        // drag receiver triggers another mouse enter event, which causes a stack
        // overflow, pretty bizarre.
        //CallDeferred(nameof(AddAndDragPart), blueprint);
        AddAndDragPart(blueprint);
    }

    private void OnPartDragAreaDragReceiverDragLeave()
    {
        draggedPartPreview.SetPartData(draggedPart.partData);

        RemovePart(draggedPart);

        draggedPart = null;
        dragOffset = Vector2.Zero;
    }

    private void OnPartDragAreaDragReceiverDragOver()
    {
        if (draggedPart == null)
            return;
        // Might be null because we defer setting it
        Vector2 place = canvas.GetLocalMousePosition() - dragOffset;
        draggedPart.Position = place.Snapped(gridSnap);
        GetAreaForPart(draggedPart).Position = draggedPart.Position;
    }

    private void OnPartDragAreaDragReceiverReceived(Control source, RefCounted data)
    {
        partTransformControls.Part = draggedPart;
        partTransformControls.CallDeferred("UpdateTransform", canvas);
        draggedPart = null;
    }


    private void OnPartControlsRotated(float angle)
    {
        GetAreaForPart(partTransformControls.Part).Rotation = angle;
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

    private void OnCoresDragReceiverReceived(Control source, RefCounted data)
    {
        logger.Log($"OnCoresDragReceiverReceived :: {data.GetType().Name}");
        
        switch (data)
        {
            case PartData partData:
                AddCoreButton(partData);
                break;

            case BlueprintPart blueprintPart:
                AddCoreButton(PartData.From(blueprintPart));
                break;
        }

        draggedPartPreview.Clear();
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