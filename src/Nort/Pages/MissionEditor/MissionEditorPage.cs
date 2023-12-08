using System;
using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CtrlRaul.Godot;
using CtrlRaul.Godot.Linq;
using Nort.Entities;
using Nort.Pages.CraftBuilder;
using Nort.Popups;

namespace Nort.Pages.MissionEditor;

public interface IEditorEntity<T> where T : EntitySetup
{
    public T Setup { get; set; }
}

public partial class MissionEditorPage : Page
{
    public class NavigationData
    {
        public Mission Mission { get; }

        public NavigationData(Mission mission)
        {
            Mission = mission;
        }
    }
    
    private enum Action
    {
        None,
        MultiSelect,
        DragSelection,
        MouseDown,
        Panning
    }

    private readonly Vector2 gridSnap = Vector2.One * 64f;
    private const float ZoomStep = 0.1f;
    private const float ZoomMin = 0.1f;
    private const float ZoomMax = 1f;

    [Export] private PackedScene missionSelectorPopupScene;
    [Export] private PackedScene editorCraftScene;
    [Export] private PackedScene editorPlayerCraftScene;
    [Export] private PackedScene editorOrphanPartScene;

    [Ready] public Control sandbox;
    [Ready] public Control canvas;
    [Ready] public Control entitiesContainer;
    [Ready] public Panel selectionRect;
    [Ready] public Explorer explorer;
    [Ready] public LineEdit missionNameLineEdit;
    [Ready] public Label missionIdLabel;

    private readonly Dictionary<EditorEntity, Vector2> dragOffsets = new();
    private readonly Dictionary<EditorEntity, Vector2> copyOffsets = new();
    private readonly List<EditorEntity> selection = new();
    private readonly List<EditorEntity> copied = new ();
    private Vector2 selectionStart = Vector2.Zero;
    private Action action = Action.None;

    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();
        UpdateStageCameraTransform();
    }

    public override async Task Initialize()
    {
        await Game.Instance.Initialize();

        entitiesContainer.QueueFreeChildren();

        if (PagesNavigator.Instance.NavigationData is NavigationData navigationData)
        {
            Mission = navigationData.Mission;
        }
        else
        {
            missionIdLabel.Text = Assets.GenerateUuid();
            AddEntity(new PlayerCraftSetup { testBlueprint = Assets.Instance.InitialBlueprint });
        }

        Stage.Instance.Clear();
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (Input.IsActionJustPressed("copy"))
        {
            ShortcutCopy();
        }
        else if (Input.IsActionJustPressed("paste"))
        {
            ShortcutPaste();
        }
        else if (Input.IsActionJustPressed("select_all"))
        {
            ShortcutSelectAll();
        }
        else if (Input.IsActionJustPressed("delete"))
        {
            ShortcutDelete();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (action == Action.None)
        {
            return;
        }

        if (@event is not InputEventMouseMotion mouseMotion)
            return;
        
        Vector2 mouse = canvas.GetLocalMousePosition();

        switch (action)
        {
            case Action.MultiSelect:
                selectionRect.Size = (mouse - selectionStart).Abs();
                selectionRect.Position = new Vector2(
                    mouse.X < selectionStart.X ? mouse.X : selectionStart.X,
                    mouse.Y < selectionStart.Y ? mouse.Y : selectionStart.Y
                );
                break;

            case Action.DragSelection:
                foreach (EditorEntity entity in selection)
                {
                    entity.Position = (canvas.GetLocalMousePosition() - dragOffsets[entity]).Snapped(gridSnap);
                }

                break;

            case Action.MouseDown:
                if (Input.IsActionPressed("shift"))
                {
                    action = Action.MultiSelect;
                    selectionStart = canvas.GetLocalMousePosition();
                    selectionRect.Position = Vector2.Zero;
                    selectionRect.Size = Vector2.Zero;
                    selectionRect.Visible = true;
                }
                else
                {
                    action = Action.Panning;
                    canvas.Position += mouseMotion.Relative;
                    UpdateStageCameraTransform();
                }

                break;

            case Action.Panning:
                canvas.Position += mouseMotion.Relative;
                UpdateStageCameraTransform();
                break;
        }
    }

    public void ShortcutCopy()
    {
        copyOffsets.Clear();
        copied.Clear();
        copied.AddRange(selection);

        Vector2 canvasCenter = GetVisualCanvasCenter();

        foreach (EditorEntity copiedEntity in copied)
            copyOffsets[copiedEntity] = copiedEntity.Position - canvasCenter;
    }

    public void ShortcutPaste()
    {
        ClearSelection();

        Vector2 canvasCenter = GetVisualCanvasCenter();
        List<EditorEntity> pastedEntities = new();

        foreach (EditorEntity copiedEntity in copied)
        {
            EditorEntity pastedEntity;
            
            switch (copiedEntity)
            {
                // This case should come before EditorCraft, otherwise both will run.
                case EditorPlayerCraft:
                    continue;
                
                case EditorCraft copiedCraft:
                    pastedEntity = AddEntity(copiedCraft.Setup);
                    break;
                
                case EditorOrphanPart copiedOrphanPart:
                    pastedEntity = AddEntity(copiedOrphanPart.Setup);
                    break;
                
                default:
                    throw new NotImplementedException();
            }
            
            pastedEntity.Position = (canvasCenter + copyOffsets[copiedEntity]).Snapped(gridSnap);
            pastedEntities.Add(pastedEntity);

            Select(pastedEntity, clearCurrentSelection: false);
        }
    }

    public void ShortcutSelectAll()
    {
        foreach (EditorEntity entity in GetEntities())
        {
            if (!selection.Contains(entity))
            {
                Select(entity, clearCurrentSelection: false);
            }
        }
    }

    public void ShortcutDelete()
    {
        foreach (EditorEntity entity in selection)
        {
            if (entity is EditorPlayerCraft)
                continue;

            entity.QueueFree();
        }
    }

    public void Zoom(int delta)
    {
        float change = canvas.Scale.X + delta * ZoomStep * canvas.Scale.X;
        Vector2 newZoom = Vector2.One * Mathf.Clamp(change, ZoomMin, ZoomMax);

        if (newZoom == canvas.Scale)
            return;
        
        Vector2 localMouse = canvas.GetLocalMousePosition();

        canvas.Scale = newZoom;
        canvas.Position -= localMouse * canvas.Scale * ZoomStep * delta;

        UpdateStageCameraTransform();
    }

    public void UpdateStageCameraTransform()
    {
        Stage.Instance.camera.Zoom = canvas.Scale;
        Stage.Instance.camera.Position = -canvas.Position / canvas.Scale;
    }

    public void SandboxMoused(InputEventMouseButton @event)
    {
        if (@event.Pressed)
        {
            action = Action.MouseDown;
        }
        else
        {
            switch (action)
            {
                case Action.MultiSelect:
                    Rect2 selectionArea = selectionRect.GetGlobalRect();
                    foreach (EditorEntity entity in GetEntities())
                    {
                        if (!selection.Contains(entity) && selectionArea.Intersects(entity.hitBox.GetGlobalRect()))
                        {
                            Select(entity);
                        }
                    }

                    selectionRect.Visible = false;
                    break;

                case Action.MouseDown:
                    ClearSelection();
                    explorer.Clear();
                    break;
            }

            action = Action.None;
        }
    }

    private Mission Mission
    {
        get
        {
            Mission mission = new()
            {
                id = string.IsNullOrEmpty(missionIdLabel.Text) ? Assets.GenerateUuid() : missionIdLabel.Text,
                displayName = missionNameLineEdit.Text,
            };

            foreach (EditorEntity entity in GetEntities())
            {
                logger.Log(entity.GetType().Name);
                switch (entity)
                {
                    // This case should come before EditorCraft, otherwise both will run.
                    case EditorPlayerCraft editorPlayerCraft:
                        mission.entities.Add(editorPlayerCraft.Setup);
                        break;
                
                    case EditorCraft editorCraft:
                        mission.entities.Add(editorCraft.Setup);
                        break;
                
                    case EditorOrphanPart editorOrphanPart:
                        mission.entities.Add(editorOrphanPart.Setup);
                        break;
                
                    default:
                        throw new NotImplementedException();
                }
            }

            return mission;
        }
        
        set
        {
            Clear();

            missionNameLineEdit.Text = value.displayName;
            missionIdLabel.Text = value.id;

            foreach (EntitySetup entitySetup in value.entities)
                AddEntity(entitySetup); 
        }
    }

    public void Clear()
    {
        missionNameLineEdit.Text = "";
        missionIdLabel.Text = "";

        explorer.Clear();
        ClearSelection();

        entitiesContainer.QueueFreeChildren();
    }

    private IEnumerable<EditorEntity> GetEntities()
    {
        return entitiesContainer.GetChildren().Cast<EditorEntity>();
    }

    public EditorEntity AddEntity(EntitySetup setup)
    {
        switch (setup)
        {
            case CraftSetup craftSetup:
                return AddEntityWithScene<EditorCraft, CraftSetup>(craftSetup, editorCraftScene);
            
            case PlayerCraftSetup playerCraftSetup:
                return AddEntityWithScene<EditorPlayerCraft, PlayerCraftSetup>(playerCraftSetup, editorPlayerCraftScene);
            
            case OrphanPartSetup orphanPartSetup:
                return AddEntityWithScene<EditorOrphanPart, OrphanPartSetup>(orphanPartSetup, editorOrphanPartScene);
            
            default:
                throw new NotImplementedException($"Add case for type '{setup.Type}'");
        }
    }

    public T AddEntityWithScene<T, S>(S setup, PackedScene scene) where T : EditorEntity, IEditorEntity<S> where S : EntitySetup
    {
        T entity = scene.Instantiate<T>();
        entitiesContainer.AddChild(entity);

        entity.Setup = setup;
        entity.Pressed += () => OnEntityPressed(entity);
        entity.DragStart += () => OnEntityDragStart(entity);
        entity.DragStop += OnEntityDragStop;

        return entity;
    }

    public void ClearSelection()
    {
        foreach (EditorEntity selectedEntity in selection)
            selectedEntity.Selected = false;

        selection.Clear();
    }

    public void Select(EditorEntity entity, bool clearCurrentSelection = true)
    {
        if (selection.Contains(entity))
        {
            if (Input.IsActionPressed("shift"))
            {
                entity.Selected = false;
                selection.Remove(entity);
            }

            return;
        }

        if (clearCurrentSelection && !Input.IsActionPressed("shift"))
        {
            ClearSelection();
        }

        entity.Selected = true;
        selection.Add(entity);
    }

    public void StoreMission()
    {
        Mission mission = Mission;

        try
        {
            Assets.Instance.StoreMission(mission);
        }
        catch (Exception exception)
        {
            string message = $"Failed to export mission: {exception.Message}";
            logger.Error(message);
            PopupsManager.Instance.Error(message);
            return;
        }

        logger.Log("Exported mission!");
        PopupsManager.Instance.Info("Exported mission!");
    }

    private void OnSandboxGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButtonEvent)
        {
            missionNameLineEdit.ReleaseFocus();
            
            switch (mouseButtonEvent.ButtonIndex)
            {
                case MouseButton.Left:
                    SandboxMoused(mouseButtonEvent);
                    break;
                case MouseButton.WheelUp:
                    Zoom(1);
                    break;
                case MouseButton.WheelDown:
                    Zoom(-1);
                    break;
            }
        }
    }

    private void OnAddCraftButtonPressed()
    {
        CraftSetup craftSetup = new()
        {
            Blueprint = Assets.Instance.InitialBlueprint,
            Faction = Assets.Instance.DefaultEnemyFaction,
            componentSet = Craft.ComponentSet.Fighter
        };
        EditorEntity entity = AddEntity(craftSetup);

        entity.Position = GetVisualCanvasCenter().Snapped(gridSnap);

        if (!selection.Any())
        {
            Select(entity);
            explorer.SetEntity(entity);
        }
    }

    private Vector2 GetVisualCanvasCenter()
    {
        return -(canvas.Position - sandbox.GetRect().Size * 0.5f) / canvas.Scale;
    }
    

    private void OnEntityPressed(EditorEntity entity)
    {
        explorer.SetEntity(entity);
        Select(entity);
    }

    private void OnEntityDragStart(EditorEntity entity)
    {
        explorer.Clear();

        foreach (EditorEntity selectedEntity in selection)
            dragOffsets[selectedEntity] = canvas.GetLocalMousePosition() - selectedEntity.Position;

        action = Action.DragSelection;
    }

    private void OnEntityDragStop()
    {
        dragOffsets.Clear();
        action = Action.None;
    }

    private void OnTestButtonPressed()
    {
        _ = PagesNavigator.Instance.GoTo(
            Config.Pages.Mission,
            new MissionPage.NavigationData(true, Mission)
        );
    }

    private void OnCraftBuilderPressed()
    {
        _ = PagesNavigator.Instance.GoTo(
            Config.Pages.CraftBuilder,
            new CraftBuilderPage.NavigationData(true)
        );
    }

    private void OnExportButtonPressed()
    {
        logger.Log($"missionNameLineEdit.Text: {missionNameLineEdit.Text}");
        
        if (!string.IsNullOrEmpty(missionNameLineEdit.Text))
        {
            StoreMission();
            return;
        }
        
        DialogPopup popup = PopupsManager.Instance.Info("Your mission needs a name!");

        popup.AddButton("Ok", missionNameLineEdit.GrabFocus);
        popup.AddButton("Randomize", () => missionNameLineEdit.Text = $"Mission - {GD.Randi().ToString()}");
        
        popup.Removed += () =>
        {
            logger.Log($"missionNameLineEdit.Text 2: {missionNameLineEdit.Text}");
            if (!string.IsNullOrEmpty(missionNameLineEdit.Text))
                StoreMission();
        };
    }

    private void OnImportButtonPressed()
    {
        MissionSelectorPopup popup = PopupsManager.Instance.Custom<MissionSelectorPopup>(missionSelectorPopupScene);
        popup.MissionSelected += mission => Mission = mission;
    }
}
