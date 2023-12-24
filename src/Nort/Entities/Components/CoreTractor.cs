using Godot;
using System;
using CtrlRaul.Godot;

namespace Nort.Entities.Components;

public partial class CoreTractor : Node2D
{
	[Ready] public Sprite2D sprite2D;

	private float textureSizeX;
	private Entity target;

	private float breakDistance = 700;
	private float pullDistance = 450;
	private float pullForce = 30;
	
	
	public override void _Ready()
	{
		base._Ready();
		this.InitializeReady();

		if (Owner is not Craft)
			throw new Exception($"Expected parent to be {nameof(Craft)}");

		textureSizeX = sprite2D.Texture.GetSize().X;
		sprite2D.Visible = false;
        
		SetProcess(false);
		SetPhysicsProcess(false);
	}
	
	public override void _Process(double delta)
	{
		sprite2D.Scale = sprite2D.Scale with { X = GlobalPosition.DistanceTo(target.Position) / textureSizeX };
		sprite2D.LookAt(target.Position);
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		
		float distance = GlobalPosition.DistanceTo(target.GlobalPosition);

		if (distance > pullDistance)
		{
			target.Velocity += target.GlobalPosition.DirectionTo(GlobalPosition) * pullForce * Mathf.Pow(distance / pullDistance, 2) * (float)delta;
		}
	}


	public void SetTarget(Entity value)
	{
		if (target != null)
		{
			DisconnectEntityEvents(target);
		}
		
		target = value;

		if (target == null || GlobalPosition.DistanceTo(target.GlobalPosition) > breakDistance)
		{
			sprite2D.Visible = false;
			SetProcess(false);
			SetPhysicsProcess(false);
		}
		else
		{
			sprite2D.Visible = true;
			SetProcess(true);
			SetPhysicsProcess(true);
			ConnectEntityEvents(target);
		}
	}
	
	private void ConnectEntityEvents(Entity entity)
	{
		switch (entity)
		{
			case Craft craft:
				craft.Destroyed += OnTargetDestroyed;
				break;
            
			case OrphanPart orphanPart:
				orphanPart.Collected += OnTargetCollected;
				break;
		}
	}

	private void DisconnectEntityEvents(Entity entity)
	{
		switch (entity)
		{
			case Craft craft:
				craft.Destroyed -= OnTargetDestroyed;
				break;
            
			case OrphanPart orphanPart:
				orphanPart.Collected -= OnTargetCollected;
				break;
		}
	}


	private void OnTargetDestroyed()
	{
		DisconnectEntityEvents(target);
		SetTarget(null);
	}

	private void OnTargetCollected()
	{
		DisconnectEntityEvents(target);
		SetTarget(null);
	}
}
