using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CtrlRaul.Godot;
using CtrlRaul.Godot.Linq;
using Godot;
using Nort.Entities;
using CarrierCraft = Nort.Entities.CarrierCraft;
using Nort.Hud;
using Nort.Pages.CraftBuilder;
using Nort.Popups;

namespace Nort.Pages;

[AttributeUsage(AttributeTargets.Property)]
public class SavableAttribute : Attribute { }

public partial class Editor : Page
{
    public class NavigationData
    {
        public Mission Mission { get; }
        public NavigationData(Mission mission) => Mission = mission;
    }


    private enum MouseState
    {
        Up,
        Down,
        Dragging,
        Panning,
    }


    private static Stage Stage => Stage.Instance;
    private static IEnumerable<Entity> Entities => Stage.entitiesContainer.GetChildren().Cast<Entity>();


    [Export] private PackedScene missionSelectorPopupScene;

    [Ready] public Control interfaceRoot;
    [Ready] public Label mousePositionLabel;
    [Ready] public EntityInspector entityInspector;
    [Ready] public Label missionIdLabel;
    [Ready] public LineEdit missionNameLabel;


    private const float ZoomStep = 0.1f;
    private const float ZoomMin = 0.1f;
    private const float ZoomMax = 1f;

    private readonly Vector2I gridSnap = Vector2I.One * 128;
    private readonly Dictionary<Entity, Vector2> dragOffsets = new();
    private readonly Dictionary<Entity, Vector2> copyOffsets = new();
    private readonly List<Entity> selection = new();
    private readonly List<Entity> copied = new();
    private ulong lastZoom;
    private bool panning;
    private bool hasUnsavedChange;
    private Vector2 selectionStart = Vector2.Zero;
    private MouseState mouseState = MouseState.Up;


    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();
        missionIdLabel.Text = Assets.GenerateUuid();
        missionNameLabel.Text = GenerateMissionName();
    }

    public override void _Process(double delta)
    {
        Vector2 mouse = Stage.camera.GetLocalMousePosition();
        mousePositionLabel.Text = $"x{(int)mouse.X} y{(int)mouse.Y}";
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        switch (@event)
        {
            case InputEventMouseButton mouseButtonEvent:
                switch (mouseButtonEvent.ButtonIndex)
                {
                    case MouseButton.Left:
                        if (mouseButtonEvent.Pressed)
                            OnMouseDown();
                        else
                            OnMouseUp(mouseButtonEvent);
                        break;

                    case MouseButton.WheelUp:
                        ApplyZoom(1);
                        break;

                    case MouseButton.WheelDown:
                        ApplyZoom(-1);
                        break;
                }

                break;


            case InputEventMouseMotion mouseMotionEvent:
                OnMouseMove(mouseMotionEvent);
                break;


            case InputEventKey:
                if (Input.IsActionJustPressed("copy"))
                {
                    ShortcutCopy();
                    GetViewport().SetInputAsHandled();
                }
                else if (Input.IsActionJustPressed("paste"))
                {
                    ShortcutPaste();
                    GetViewport().SetInputAsHandled();
                }
                else if (Input.IsActionJustPressed("select_all"))
                {
                    ShortcutSelectAll();
                    GetViewport().SetInputAsHandled();
                }
                else if (Input.IsActionJustPressed("delete"))
                {
                    ShortcutDelete();
                    GetViewport().SetInputAsHandled();
                }

                break;
        }
    }


    public override async Task Initialize()
    {
        await Game.Instance.Initialize();

        Stage.SetGrid(gridSnap.X);

        if (PagesNavigator.Instance.NavigationData is NavigationData data)
        {
            Stage.LoadMission(data.Mission);
            if (Stage.Player != null)
                Stage.camera.Position = Stage.Player.Position;
        }
        else
        {
            Stage.Spawn<PlayerCraft>();
        }
    }

    private void ImportMission(Mission mission)
    {
        hasUnsavedChange = false;
        missionIdLabel.Text = mission.id;
        missionNameLabel.Text = mission.displayName;
        Stage.LoadMission(mission);
    }

    private void ApplyZoom(int delta)
    {
        // Prevent double input

        ulong now = Time.GetTicksMsec();

        if (now - lastZoom < 3) // 3 is arbitrary, just seemed to work well for me
            return;

        lastZoom = now;


        // Actual zooming code

        Vector2 mouse = Stage.camera.GetLocalMousePosition();
        Vector2 oldZoom = Stage.camera.Zoom;
        Vector2 zoomAmount = oldZoom * ZoomStep * delta;
        Vector2 motion = mouse * zoomAmount / (oldZoom + zoomAmount);

        Stage.camera.Zoom += zoomAmount;
        Stage.camera.Position += motion;
    }

    private Mission CreateMission()
    {
        Dictionary<Type, List<PropertyInfo>> propertiesCache = new();
        List<Dictionary<string, object>> entitySetups = new();

        foreach (Entity entity in Entities)
        {
            Type type = entity.GetType();

            List<PropertyInfo> properties;

            if (propertiesCache.TryGetValue(type, out List<PropertyInfo> cachedProperties))
            {
                properties = cachedProperties;
            }
            else
            {
                properties = GetSavableProperties(type);
                propertiesCache.Add(type, properties);
            }

            Dictionary<string, object> entityDict = properties.ToDictionary(
                property => property.Name,
                property => property.GetValue(entity)
            );

            entitySetups.Add(entityDict);
        }

        Mission mission = new()
        {
            id = missionIdLabel.Text,
            displayName = missionNameLabel.Text,
            entitySetups = entitySetups
        };

        return mission;
    }
    
    private void AddEntity<T>() where T : Entity
    {
        T entity = Stage.Spawn<T>();
        entity.Position = Stage.camera.Position.Snapped(gridSnap);
        entityInspector.SetEntity(entity);
    }
    
    
    #region Shortcut methods

    public void ShortcutCopy()
    {
        copyOffsets.Clear();
        copied.Clear();
        copied.AddRange(selection);

        Vector2 canvasCenter = Stage.camera.Position;

        foreach (Entity copiedEntity in copied)
        {
            if (copiedEntity is PlayerCraft)
                continue;
            
            copyOffsets[copiedEntity] = copiedEntity.Position - canvasCenter;
        }
    }

    public void ShortcutPaste()
    {
        selection.Clear();

        Vector2 canvasCenter = Stage.camera.Position;

        foreach (Entity copiedEntity in copied)
        {
            Entity pastedEntity = (copiedEntity.Duplicate() as Entity)!;
            Stage.Spawn(pastedEntity);
            pastedEntity.Position = (canvasCenter + copyOffsets[copiedEntity]).Snapped(gridSnap);
            selection.Add(pastedEntity);
        }
        
        entityInspector.SetEntities(selection);
    }

    public void ShortcutSelectAll()
    {
        foreach (Entity entity in Entities)
        {
            if (!selection.Contains(entity))
            {
                selection.Add(entity);
            }
        }
        
        entityInspector.SetEntities(selection);
    }

    public void ShortcutDelete()
    {
        selection.Clear();
        
        foreach (Entity entity in selection)
        {
            if (entity is PlayerCraft)
                continue;

            entity.QueueFree();
        }
    }

    #endregion


    private void OnMouseDown()
    {
        mouseState = MouseState.Down;
    }

    private void OnMouseUp(InputEventMouseButton mouseButtonEvent)
    {
        switch (mouseState)
        {
            case MouseState.Down:

                Entity hoveredEntity = GetHoveredEntity();

                if (mouseButtonEvent.ShiftPressed)
                {
                    if (hoveredEntity != null)
                    {
                        if (selection.Contains(hoveredEntity))
                        {
                            selection.Remove(hoveredEntity);
                        }
                        else
                        {
                            selection.Add(hoveredEntity);
                        }
                    }
                }
                else
                {
                    if (hoveredEntity == null)
                    {
                        selection.Clear();
                    }
                    else
                    {
                        if (!selection.Contains(hoveredEntity))
                            selection.Clear();

                        selection.Add(hoveredEntity);
                    }
                }

                entityInspector.SetEntities(selection);

                break;

            case MouseState.Dragging:
                dragOffsets.Clear();
                break;
        }

        mouseState = MouseState.Up;
    }

    private void OnMouseMove(InputEventMouseMotion mouseMotionEvent)
    {
        Vector2 motion = mouseMotionEvent.Relative / Stage.camera.Zoom;

        switch (mouseState)
        {
            case MouseState.Down:
            {
                Entity hoveredEntity = GetHoveredEntity();

                if (hoveredEntity == null)
                {
                    mouseState = MouseState.Panning;
                }
                else
                {
                    if (!selection.Contains(hoveredEntity))
                    {
                        if (!mouseMotionEvent.ShiftPressed)
                            selection.Clear();

                        selection.Add(hoveredEntity);
                    }

                    Vector2 mouse = Stage.GetGlobalMousePosition();

                    foreach (Entity entity in selection)
                        dragOffsets.Add(entity, mouse - entity.Position);

                    entityInspector.SetEntities(selection);

                    mouseState = MouseState.Dragging;
                }

                break;
            }

            case MouseState.Panning:
            {
                Stage.camera.Position -= motion;
                break;
            }

            case MouseState.Dragging:
            {
                Vector2 mouse = Stage.GetGlobalMousePosition();

                foreach (Entity entity in selection)
                    entity.Position = (mouse - dragOffsets[entity]).Snapped(gridSnap);

                break;
            }
        }
    }


    private void OnPlayerButtonPressed() => AddEntity<PlayerCraft>();
    
    private void OnCarrierButtonPressed() => AddEntity<CarrierCraft>();
    
    private void OnDroneButtonPressed() => AddEntity<DroneCraft>();
    
    
    private void OnExportButtonPressed()
    {
        try
        {
            Assets.Instance.StoreMission(CreateMission());
        }
        catch (Exception exception)
        {
            logger.Error($"Failed to store mission!\n{exception}");
            PopupsManager.Instance.Error(exception.Message, "Failed to store mission!");
        }
    }

    private void OnImportButtonPressed()
    {
        MissionSelectorPopup popup = PopupsManager.Instance.Custom<MissionSelectorPopup>(missionSelectorPopupScene);

        popup.MissionSelected += missionSelected =>
        {
            if (hasUnsavedChange)
            {
                DialogPopup unsavedChangesPopup = PopupsManager.Instance.Info(null, "Discard unsaved changes?");
                unsavedChangesPopup.AddButton("Save", OnExportButtonPressed);
                unsavedChangesPopup.AddButton("Discard", () => ImportMission(missionSelected));
                unsavedChangesPopup.AddButton("Cancel");
                return;
            }

            ImportMission(missionSelected);
        };
    }

    private void OnTestButtonPressed()
    {
        _ = PagesNavigator.Instance.GoTo(
            Config.Pages.Mission,
            new MissionPage.NavigationData(true, CreateMission())
        );
    }

    private void OnCraftBuilderPressed()
    {
        _ = PagesNavigator.Instance.GoTo(
            Config.Pages.CraftBuilder,
            new CraftBuilderPage.NavigationData(true)
        );
    }

    private void OnMissionNameLabelFocusExited()
    {
        if (string.IsNullOrEmpty(missionNameLabel.Text))
            missionNameLabel.Text = GenerateMissionName();
    }


    private static string GenerateMissionName()
    {
        return $"New Mission {((int)(Time.GetUnixTimeFromSystem() * 1000)).ToString("X")}";
    }

    private static Entity GetHoveredEntity()
    {
        // skull emoji
        return Stage.editorMouseArea.GetOverlappingAreas().FindNearest(Stage.editorMouseArea.Position)?.Owner as Entity;
    }

    public static List<PropertyInfo> GetSavableProperties(Type type)
    {
        return type.GetProperties().Where(info => info.GetCustomAttribute<SavableAttribute>() != null).ToList();
    }
}