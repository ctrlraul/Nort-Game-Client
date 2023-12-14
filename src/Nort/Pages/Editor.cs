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
public class SavableAttribute : Attribute {}

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

    [Ready] public Label mousePositionLabel;
    [Ready] public EntityInspector entityInspector;

    
    private const float ZoomStep = 0.1f;
    private const float ZoomMin = 0.1f;
    private const float ZoomMax = 1f;
    
    private readonly Vector2 gridSnap = Vector2.One * 64f;
    private readonly Mission mission = new() { id = Assets.GenerateUuid() };
    private readonly Dictionary<Entity, Vector2> dragOffsets = new();
    private readonly Dictionary<Entity, Vector2> copyOffsets = new();
    private readonly List<Entity> selection = new();
    private readonly List<Entity> copied = new ();
    private ulong lastZoom;
    private bool panning;
    private bool hasUnsavedChange;
    private Vector2 selectionStart = Vector2.Zero;
    private MouseState mouseState = MouseState.Up;
    
    
    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();
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

        if (PagesNavigator.Instance.NavigationData is NavigationData data)
        {
            Stage.LoadMission(data.Mission);
            if (Stage.Player != null)
                Stage.camera.Position = Stage.Player.Position;
        }
        else
        {
            Stage.SpawnPlayerCraft();
        }
    }

    private void ImportMission(Mission missionToImport)
    {
        hasUnsavedChange = false;
        mission.id = missionToImport.id;
        mission.displayName = missionToImport.displayName;
        Stage.LoadMission(missionToImport);
    }
    
    private Entity GetHoveredEntity()
    {
        // skull emoji
        return Stage.editorMouseArea.GetOverlappingAreas().FindNearest(Stage.editorMouseArea.Position)?.Owner as Entity;
    }
    
    
    #region Shortcut methods

    public void ShortcutCopy()
    {
        copyOffsets.Clear();
        copied.Clear();
        copied.AddRange(selection);

        Vector2 canvasCenter = Stage.camera.Position;

        foreach (Entity copiedEntity in copied)
            copyOffsets[copiedEntity] = copiedEntity.Position - canvasCenter;
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
    }

    public void ShortcutDelete()
    {
        foreach (Entity entity in selection)
        {
            if (entity is PlayerCraft)
                continue;

            entity.QueueFree();
        }
    }

    #endregion
    

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
    
    
    private void OnPlayerButtonPressed()
    {
        PlayerCraft entity = Stage.SpawnPlayerCraft();
        entity.Position = Stage.camera.Position;
        entityInspector.SetEntity(entity);
    }
    
    private void OnCarrierButtonPressed()
    {
        CarrierCraft entity = Stage.SpawnCarrierCraft();
        entity.Position = Stage.camera.Position;
        entityInspector.SetEntity(entity);
    }
    
    private void OnExportButtonPressed(bool skipNameIfPopupPossible = false)
    {
        Dictionary<Type, List<PropertyInfo>> propertiesCache = new();
        
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

            mission.entitySetups.Add(entityDict);
        }

        if (!skipNameIfPopupPossible || string.IsNullOrEmpty(mission.displayName))
        {
            TextInputPopup popup = PopupsManager.Instance.TextInput("Mission Name", mission.displayName);
            popup.Submitted += OnMissionNamePopupSubmitted;
        }

        OnMissionNamePopupSubmitted(mission.displayName);
    }

    
    private void OnImportButtonPressed()
    {
        MissionSelectorPopup popup = PopupsManager.Instance.Custom<MissionSelectorPopup>(missionSelectorPopupScene);
        
        popup.MissionSelected += missionSelected =>
        {
            if (hasUnsavedChange)
            {
                DialogPopup unsavedChangesPopup = PopupsManager.Instance.Info(null, "Discard unsaved changes?");
                unsavedChangesPopup.AddButton("Save", () => OnExportButtonPressed(true));
                unsavedChangesPopup.AddButton("Discard", () => ImportMission(missionSelected));
                unsavedChangesPopup.AddButton("Cancel");
                return;
            }

            ImportMission(missionSelected);
        };
    }

    private void OnMissionNamePopupSubmitted(string name)
    {
        mission.displayName = name;
            
        try
        {
            Assets.Instance.StoreMission(mission);
        }
        catch (Exception exception)
        {
            logger.Error($"Failed to store mission!\n{exception}");
            PopupsManager.Instance.Error(exception.Message, "Failed to store mission!");
        }
    }
    
    private void OnTestButtonPressed()
    {
        // _ = PagesNavigator.Instance.GoTo(
        //     Config.Pages.Mission,
        //     new MissionPage.NavigationData(true, )
        // );
    }

    private void OnCraftBuilderPressed()
    {
        _ = PagesNavigator.Instance.GoTo(
            Config.Pages.CraftBuilder,
            new CraftBuilderPage.NavigationData(true)
        );
    }
    

    public static List<PropertyInfo> GetSavableProperties(Type type)
    {
        return type.GetProperties().Where(info => info.GetCustomAttribute<SavableAttribute>() != null).ToList();
    }
}