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

    [Ready] public Node2D centerIndicator;
    [Ready] public Node2D entitiesContainer;
    [Ready] public Area2D editorMouseArea;
    [Ready] public Camera2D camera;

    private readonly Logger logger = new("Stage");

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
        editorMouseArea.Position = GetGlobalMousePosition();

        if (Game.Instance.InMissionEditor)
            return;

        if (Player != null)
            CameraFollowPlayer();
    }


    private void CameraFollowPlayer()
    {
        float velocity = Mathf.Max(Player.flightComponent.Velocity.Length(), 0.001f);
        camera.Zoom = camera.Zoom.Lerp(Vector2.One * Mathf.Clamp(1 / velocity * 0.005f, 0.4f, 0.5f), 0.01f);
        camera.Position = camera.Position.Lerp(Player.Position + Player.flightComponent.Velocity * 120, 0.01f);
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

    public void Clear()
    {
        entitiesContainer.QueueFreeChildren();
        camera.Zoom = Vector2.One * 0.5f;
        camera.Position = Vector2.Zero;
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
            
            if (savedValue is double savedValueDouble)
                property.SetValue(entity, (float)savedValueDouble);
            else
                property.SetValue(entity, savedValue);
        }
        
        Spawn(entity);
        
        return entity;
    }

    public void Spawn(Entity entity)
    {
        entitiesContainer.AddChild(entity);

        if (entity is not PlayerCraft newPlayer)
            return;
        
        if (Player != null)
        {
            logger.Error("Spawning a player while another instance already exists, destroying old instance");

            Player.Destroyed -= OnPlayerDestroyed;
            Player.Destroy();
        }

        Player = newPlayer;
        Player.Destroyed += OnPlayerDestroyed;

        if (!Game.Instance.InMissionEditor)
            camera.Position = Player.Position;

        PlayerSpawned?.Invoke(Player);
    }
    

    private void OnPlayerDestroyed()
    {
        Player = null;
        PlayerDestroyed?.Invoke();
    }
}