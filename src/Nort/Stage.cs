using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CtrlRaul;
using CtrlRaul.Godot;
using CtrlRaul.Godot.Linq;
using Godot;
using Nort.Entities;
using Nort.Pages;
using Nort.Popups;

namespace Nort;

public partial class Stage : Node2D
{
    public static Stage Instance { get; private set; }

    public event Action<Craft> PlayerSpawned;
    public event Action PlayerDestroyed;

    [Export] private PackedScene carrierCraftScene;
    [Export] private PackedScene playerCraftScene;
    [Export] private PackedScene droneCraftScene;
    [Export] private PackedScene orphanPartScene;

    [Ready] public Grid grid;
    [Ready] public Node2D entitiesContainer;
    [Ready] public Area2D mouseArea;
    [Ready] public Camera2D camera;

    private readonly Logger logger = new("Stage");

    private readonly List<string> problems = new();
    private readonly Dictionary<string, Entity> entitiesMap = new();
    public readonly List<PartData> partsCollected = new();

    private PlayerCraft player;
    public PlayerCraft Player
    {
        get => IsInstanceValid(player) ? player : null;
        private set => player = value;
    }


    public override void _Ready()
    {
        Instance = this;
        this.InitializeReady();
        Clear();
    }

    public override void _Process(double delta)
    {
        mouseArea.Position = GetGlobalMousePosition();

        if (Game.Instance.InMissionEditor)
            return;

        if (Player != null)
            CameraFollowPlayer();
    }


    public void SetGrid(int size)
    {
        grid.size = size;
        grid.disabled = size == 0;
    }

    public Entity GetEntityOnMouse()
    {
        Area2D nearestArea = mouseArea.GetOverlappingAreas().FindNearest(mouseArea.Position);

        switch (nearestArea)
        {
            case null:
                return null;
            
            case CraftPart craftPart:
                return craftPart.Craft;
            
            case { Owner: OrphanPart }:
                return nearestArea.Owner as OrphanPart;
            
            default:
                throw new Exception($"Unhandled area type on mouse: {nearestArea.Owner.GetType().Name}");
        }
    }

    private void CameraFollowPlayer()
    {
        float velocity = Mathf.Max(Player.Velocity.Length(), 0.001f);
        camera.Zoom = camera.Zoom.Lerp(Vector2.One * Mathf.Clamp(1 / velocity * 0.005f, 0.4f, 0.5f), 0.01f);
        camera.Position = camera.Position.Lerp(Player.Position + Player.Velocity * 120, 0.01f);
    }

    public void LoadMission(Mission mission)
    {
        Clear();

        logger.Log($"Loading mission '{mission.displayName}'");

        foreach (EntitySetup entitySetup in mission.entitySetups)
        {
            try
            {
                Spawn(entitySetup);
            }
            catch (Exception exception)
            {
                AddProblem($"Failed to spawn entity:\n{exception}");
            }
        }

        MakeConnections();
        
        if (problems.Any())
        {
            string problemsList = problems.Aggregate(((current, next) => current + "\n" + next));
            DialogPopup popup = PopupsManager.Instance.Error(problemsList, "There were a few problems when loading this mission!");
            popup.Width = 1000;
        }
    }

    private void AddProblem(string problem)
    {
        problems.Add(problem);
        logger.Error(problem);
    }

    private bool ShouldSaveProgress()
    {
        if (Game.Instance.CurrentPlayer == null)
            return false;

        if (PagesNavigator.Instance.CurrentPage is MissionHud { FromEditor: true })
            return false;

        return true;
    }

    [Connectable]
    public void CompleteMission()
    {
        if (ShouldSaveProgress())
        {
            foreach (PartData partData in partsCollected)
            {
                LocalPlayersManager.Instance.AddPart(Game.Instance.CurrentPlayer, partData);
            }
            
            LocalPlayersManager.Instance.StoreLocalPlayer(Game.Instance.CurrentPlayer);
        }
    }

    public void Clear()
    {
        camera.Zoom = Vector2.One * 0.5f;
        camera.Position = Vector2.Zero;
        entitiesContainer.QueueFreeChildren();
        problems.Clear();
        entitiesMap.Clear();
        partsCollected.Clear();
        Player = null;
    }

    public Entity GetEntityByUuid(string uuid)
    {
        return entitiesMap.TryGetValue(uuid, out Entity entity) ? entity : null;
    }
    

    private Entity InstantiateEntityScene(string typeName)
    {
        return typeName switch
        {
            nameof(PlayerCraft) => playerCraftScene.Instantiate<PlayerCraft>(),
            nameof(CarrierCraft) => carrierCraftScene.Instantiate<CarrierCraft>(),
            nameof(DroneCraft) => droneCraftScene.Instantiate<DroneCraft>(),
            nameof(OrphanPart) => orphanPartScene.Instantiate<OrphanPart>(),
            
            _ => throw new Exception($"No entity scene configured for type '{typeName}'")
        };
    }
    
    public T Spawn<T>() where T : Entity
    {
        T entity = InstantiateEntityScene(typeof(T).Name) as T;
        Spawn(entity);
        return entity;
    }
    
    public Entity Spawn(EntitySetup setup)
    {
        Entity entity = InstantiateEntityScene(setup.typeName);
        Spawn(entity, setup);
        return entity;
    }

    public void Spawn(Entity entity, EntitySetup setup = default)
    {
        entitiesContainer.AddChild(entity);
        
        if (setup != default)
            Entity.SetSetup(entity, setup);

        // Important that this comes after setting the setup since that's when the entity's UUID is set.
        entitiesMap.Add(entity.Uuid, entity);

        switch (entity)
        {
            case PlayerCraft newPlayer:
                
                if (Player != null)
                {
                    logger.Warn("Spawning a player while another instance already exists, freeing old instance");
                    Player.Destroyed -= OnPlayerDestroyed;
                    Player.QueueFree();
                }

                Player = newPlayer;
                Player.Destroyed += OnPlayerDestroyed;

                if (!Game.Instance.InMissionEditor)
                    camera.Position = Player.Position;

                PlayerSpawned?.Invoke(Player);
                
                break;
            
            case OrphanPart orphanPart:
                orphanPart.Collected += () => OnOrphanPartCollected(orphanPart.GetPartData());
                break;
        }
    }

    public IEnumerable<Entity> GetEntities()
    {
        return entitiesContainer.GetChildren().Cast<Entity>();
    }

    public void MakeConnections()
    {
        foreach (Entity entity in GetEntities())
        {
            foreach (EntityConnection connection in entity.Connections)
            {
                Type sourceType = entity.GetType();
                EventInfo eventInfo = sourceType.GetEvent(connection.eventName);

                if (eventInfo == null)
                {
                    AddProblem($"Event '{connection.eventName}' not found in type {sourceType.Name}");
                    continue;
                }
                
                Node target;
                
                if (connection.targetUuid == null)
                {
                    target = this;
                }
                else if (entitiesMap.TryGetValue(connection.targetUuid, out Entity targetEntity))
                {
                    target = targetEntity;
                }
                else
                {
                    AddProblem($"No entity found with UUID '{connection.targetUuid}'");
                    continue;
                }
            
                Type targetType = target.GetType();
                MethodInfo methodInfo = targetType.GetMethod(connection.methodName);

                if (methodInfo == null)
                {
                    AddProblem($"Method '{connection.methodName}' not found in type {targetType.Name}");
                    continue;
                }
                
                Delegate methodDelegate = Delegate.CreateDelegate(eventInfo.EventHandlerType, target, methodInfo);
            
                eventInfo.AddEventHandler(entity, methodDelegate);
            }
        }
    }
    

    private void OnPlayerDestroyed()
    {
        Player = null;
        PlayerDestroyed?.Invoke();
    }

    private void OnOrphanPartCollected(PartData partData)
    {
        partsCollected.Add(partData);
    }
}