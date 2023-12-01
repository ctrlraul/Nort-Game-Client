using System;
using CtrlRaul;
using CtrlRaul.Godot.Linq;
using Godot;
using Nort.Entities;
using Nort.Entities.Components;

namespace Nort;

public partial class Stage : Node2D
{
	public static Stage Instance { get; private set; }

	public event Action<Craft> PlayerSpawned; 
	public event Action PlayerDestroyed;
	
	[Export] private PackedScene craftScene;
	[Export] private PackedScene playerCraftScene;
	[Export] private PackedScene orphanPartScene;

	private readonly Logger logger = new("Stage");
	private Node2D entitiesContainer;
	public Camera2D camera;
	
	private Craft player;
	private FlightComponent flightComponent;
	
	public override void _Ready()
	{
		Instance = this;
		entitiesContainer = GetNode<Node2D>("%EntitiesContainer");
		camera = GetNode<Camera2D>("Camera2D");
		Clear();
	}
	
	public override void _Process(double delta)
	{
		float velocity = Mathf.Max(flightComponent.Velocity.Length(), 0.001f);
		camera.Zoom = camera.Zoom.Lerp(Vector2.One * Mathf.Clamp(1 / velocity * 0.005f, 0.4f, 0.5f), 0.01f);
		camera.Position = camera.Position.Lerp(player.Position + flightComponent.Velocity * 120, 0.01f);
	}

	public void LoadMission(Mission mission)
	{
		logger.Log($"Loading mission '{mission.displayName}'");
		
		
	}

	public void Clear()
	{
		SetProcess(false);
		entitiesContainer.QueueFreeChildren();
		camera.Zoom = Vector2.One * 0.5f;
	}

	public void SpawnPlayerCraft()
	{
		PlayerCraftSetup setup = new() { testBlueprint = Assets.Instance.InitialBlueprint };
		player = Spawn(setup);
		flightComponent = player.GetComponent<FlightComponent>();
		PlayerSpawned?.Invoke(player);
	}

	public OrphanPart Spawn(OrphanPartSetup setup)
	{
		OrphanPart entity = orphanPartScene.Instantiate<OrphanPart>();
		entitiesContainer.AddChild(entity);
		entity.SetSetup(setup);
		return entity;
	}

	public Craft Spawn(CraftSetup setup)
	{
		Craft entity = new();
		entitiesContainer.AddChild(entity);
		entity.SetSetup(setup);
		return entity;
	}

	public Craft Spawn(PlayerCraftSetup setup)
	{
		Craft entity = new();
		entitiesContainer.AddChild(entity);
		entity.SetSetup(setup);
		return entity;
	}

	private void OnPlayerDestroyed()
	{
		player = null;
		PlayerDestroyed?.Invoke();
		SetProcess(false);
	}
}