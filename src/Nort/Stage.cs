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

    public event Action<MissionCompletion> MissionCompleted;
    public event Action<PlayerCraft> PlayerSpawned;
    public event Action PlayerDestroyed;

    // Entities
    [Export] private PackedScene carrierCraftScene;
    [Export] private PackedScene playerCraftScene;
    [Export] private PackedScene droneCraftScene;
    [Export] private PackedScene conductorCraftScene;
    [Export] private PackedScene turretCraftScene;
    [Export] private PackedScene orphanPartScene;
    [Export] private PackedScene coreBulletProjectileScene;
    
    // Effects
    [Export] private PackedScene coreExplosionScene;

    [Ready] public Grid grid;
    [Ready] public Node2D entitiesContainer;
    [Ready] public Node2D effectsContainer;
    [Ready] public Area2D mouseArea;
    [Ready] public Camera2D camera;

    private readonly Logger logger = new("Stage");

    private readonly List<string> problems = new();
    private readonly Dictionary<string, Entity> entitiesMap = new();
    public readonly List<PartData> partsCollected = new();

    private string missionId;
    private DateTime missionStartTime;
    private int objectivesTotal;
    private int objectivesCompleted;

    private PlayerCraft player;
    public PlayerCraft Player
    {
        get => IsInstanceValid(player) ? player : null;
        private set => player = value;
    }

    private ConductorCraft conductor;


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
        List<Entity> entitiesOnMouse = new();

        foreach (Area2D area in mouseArea.GetOverlappingAreas())
        {
            switch (area)
            {
                case CraftPart craftPart:
                    entitiesOnMouse.Add(craftPart.Craft);
                    break;
                
                case { Owner: OrphanPart }:
                    entitiesOnMouse.Add(area.Owner as OrphanPart);
                    break;
                
                default:
                    logger.Error($"Unhandled area type on mouse: {area.Owner.GetType().Name}");
                    break;
            }
        }

        return entitiesOnMouse.FindNearest(mouseArea.Position);
    }

    private void CameraFollowPlayer()
    {
        Vector2 targetZoom = Vector2.One * 0.75f - Vector2.One * Player.Velocity.Length() * 0.01f;
        Vector2 targetPosition = Player.Position + Player.Velocity * 110;
        camera.Zoom = camera.Zoom.Lerp(targetZoom, 0.005f);
        camera.Position = camera.Position.Lerp(targetPosition, 0.01f);
    }

    public void LoadMission(Mission mission)
    {
        Clear();

        missionId = mission.id;
        missionStartTime = DateTime.Now;

        logger.Log($"Loading mission '{mission.displayName}'");

        foreach (EntitySetup entitySetup in mission.entitySetups)
        {
            try
            {
                AddEntity(entitySetup);
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
        TimeSpan time = DateTime.Now - missionStartTime;
        float score = 1337;

        if (ShouldSaveProgress())
        {
            Player currentPlayer = Game.Instance.CurrentPlayer;
            
            foreach (PartData partData in partsCollected)
                LocalPlayersManager.Instance.AddPart(currentPlayer, partData);

            LocalPlayersManager.Instance.UpdateMissionRecord(currentPlayer, missionId, (float)time.TotalSeconds, score);
            LocalPlayersManager.Instance.StoreLocalPlayer(currentPlayer);
        }

        MissionCompletion missionCompletion = new(
            (float)time.TotalSeconds,
            score,
            partsCollected.ToArray()
        );

        MissionCompleted?.Invoke(missionCompletion);
    }

    public void Clear()
    {
        camera.Zoom = Vector2.One * 0.75f;
        camera.Position = Vector2.Zero;
        entitiesContainer.QueueFreeChildren();
        problems.Clear();
        entitiesMap.Clear();
        partsCollected.Clear();
        missionId = null;
        // missionStartTime = ;
        objectivesTotal = 0;
        objectivesCompleted = 0;
        Player = null;
        conductor = null;
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
            nameof(ConductorCraft) => conductorCraftScene.Instantiate<ConductorCraft>(),
            nameof(TurretCraft) => turretCraftScene.Instantiate<TurretCraft>(),
            nameof(OrphanPart) => orphanPartScene.Instantiate<OrphanPart>(),
            nameof(CoreBulletProjectile) => coreBulletProjectileScene.Instantiate<CoreBulletProjectile>(),
            
            _ => throw new Exception($"No entity scene configured for type '{typeName}'")
        };
    }

    public T AddEntity<T>() where T : Entity
    {
        T entity = InstantiateEntityScene(typeof(T).Name) as T;
        AddEntity(entity);
        return entity;
    }

    public Entity AddEntity(EntitySetup setup)
    {
        Entity entity = InstantiateEntityScene(setup.typeName);
        AddEntity(entity, setup);
        return entity;
    }

    public void AddEntity(Entity entity, EntitySetup setup = default)
    {
        switch (entity)
        {
            case PlayerCraft newPlayer:
                if (Player != null)
                {
                    logger.Warn("Spawning a player while another instance already exists, freeing old instance");
                    Player.QueueFree();
                }

                Player = newPlayer;
                Player.Spawned += () => OnPlayerSpawned(newPlayer);
                Player.Destroyed += OnPlayerDestroyed;

                break;

            case ConductorCraft newConductor:
                if (IsInstanceValid(conductor))
                {
                    logger.Warn("Spawning a conductor while another instance already exists, freeing old instance");
                    conductor.QueueFree();
                }

                conductor = newConductor;
                conductor.Conduct += CompleteMission;

                break;

            case OrphanPart orphanPart:
                orphanPart.Collected += () => OnOrphanPartCollected(orphanPart.GetPartData());

                break;
        }

        entitiesContainer.AddChild(entity);

        setup?.Inject(entity);

        // Important that this comes after injecting the setup since that's when the entity's UUID is set.
        entitiesMap.Add(entity.Uuid, entity);

        if (setup == null || setup is { autoSpawn: true } || Game.Instance.InMissionEditor)
        {
            if (Game.Instance.InMissionEditor)
                entity.SpawnSilently();
            else
                entity.Spawn();
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
            Type sourceType = entity.GetType();

            if (!string.IsNullOrEmpty(entity.PlayerObjective))
            {
                EventInfo eventInfo = sourceType.GetEvent(entity.PlayerObjective)!;
                eventInfo.AddEventHandler(entity, OnObjectiveCompleted);
                objectivesTotal += 1;
            }
            
            foreach (EntityConnection connection in entity.Connections)
            {
                EventInfo eventInfo = sourceType.GetEvent(connection.eventName);

                if (eventInfo == null)
                {
                    AddProblem($"Event '{connection.eventName}' not found in type {sourceType.Name}");
                    continue;
                }
                
                Node target = connection.targetUuid != null ? GetEntityByUuid(connection.targetUuid) : this;
                
                if (target == null)
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

                // Using this to call directly can cause problems due godot's process time stuff or whatever
                // Delegate handler = Delegate.CreateDelegate(eventInfo.EventHandlerType!, target, methodInfo);

                Action handler = () => target.CallDeferred(methodInfo.Name);

                eventInfo.AddEventHandler(entity, handler);
            }
        }
    }

    
    public void AddCoreExplosionEffect(Vector2 position)
    {
        Node2D effect = coreExplosionScene.Instantiate<Node2D>();
        effect.Position = position;
        effectsContainer.AddChild(effect);
    }
    

    private void OnPlayerDestroyed()
    {
        Player = null;
        PlayerDestroyed?.Invoke();
    }

    private void OnPlayerSpawned(PlayerCraft playerCraft)
    {
        Player = playerCraft;

        if (!Game.Instance.InMissionEditor)
            camera.Position = Player.Position;

        PlayerSpawned?.Invoke(Player);
    }

    private void OnOrphanPartCollected(PartData partData)
    {
        partsCollected.Add(partData);
    }

    private void OnObjectiveCompleted()
    {
        objectivesCompleted += 1;
        if (objectivesCompleted >= objectivesTotal)
            conductor.CallDeferred(nameof(conductor.Spawn));
    }
}