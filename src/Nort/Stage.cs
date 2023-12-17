using System;
using System.Collections.Generic;
using System.Reflection;
using CtrlRaul;
using CtrlRaul.Godot;
using CtrlRaul.Godot.Linq;
using Godot;
using Nort.Entities;
using Nort.Pages;

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
        return mouseArea.GetOverlappingAreas().FindNearest(mouseArea.Position)?.Owner as Entity;
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

        foreach (Dictionary<string, object> entitySetup in mission.entitySetups)
        {
            try
            {
                Spawn(entitySetup);
            }
            catch (Exception exception)
            {
                logger.Error($"Failed to spawn entity:\n{exception}");
            }
        }
    }

    private bool ShouldSaveProgress()
    {
        if (Game.Instance.CurrentPlayer == null)
            return false;

        if (PagesNavigator.Instance.CurrentPage is MissionPage { FromEditor: true })
            return false;

        return true;
    }

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
        partsCollected.Clear();
    }
    

    private Entity InstantiateEntityScene(string typeName)
    {
        PackedScene scene = typeName switch
        {
            nameof(PlayerCraft) => playerCraftScene,
            nameof(CarrierCraft) => carrierCraftScene,
            nameof(DroneCraft) => droneCraftScene,
            nameof(OrphanPart) => orphanPartScene,
            
            _ => throw new Exception($"No entity scene configured for type '{typeName}'")
        };

        return scene.Instantiate<Entity>();
    }
    
    public T Spawn<T>() where T : Entity
    {
        T entity = InstantiateEntityScene(typeof(T).Name) as T;
        entitiesContainer.AddChild(entity);
        return entity;
    }
    
    public Entity Spawn(Dictionary<string, object> setup)
    {
        if (!setup.TryGetValue("Type", out object value) || value is not string typeName)
            throw new Exception("Invalid or missing 'Type' property in entity setup");
        
        Entity entity = InstantiateEntityScene(typeName);

        foreach (PropertyInfo property in Editor.GetSavableProperties(entity.GetType()))
        {
            if (property.Name == "Type")
                continue;

            if (!setup.TryGetValue(property.Name, out object savedValue))
                continue;
            
            property.SetValue(entity, savedValue is double dbl ? (float)dbl : savedValue);
        }
        
        Spawn(entity);
        
        return entity;
    }

    public void Spawn(Entity entity)
    {
        entitiesContainer.AddChild(entity);

        switch (entity)
        {
            case PlayerCraft newPlayer:
                
                if (Player != null)
                {
                    logger.Warn("Spawning a player while another instance already exists, destroying old instance");
                    Player.Destroyed -= OnPlayerDestroyed;
                    Player.Destroy();
                }

                Player = newPlayer;
                Player.Destroyed += OnPlayerDestroyed;

                if (!Game.Instance.InMissionEditor)
                    camera.Position = Player.Position;

                PlayerSpawned?.Invoke(Player);
                
                break;
            
            case OrphanPart orphanPart:
                orphanPart.Collected += OnOrphanPartCollected;
                break;
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