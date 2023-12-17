using System;
using Godot;
using Nort.Pages;

namespace Nort.Entities;

public partial class Entity : Node2D
{
    private const float Damp = 0.95f;
    
    [Savable] public string Type => GetType().Name;
    
    [Savable]
    public float X
    {
        get => Position.X;
        set => Position = Position with { X = value };
    }
    
    [Savable]
    public float Y
    {
        get => Position.Y;
        set => Position = Position with { Y = value };
    }
    
    [Savable]
    public float Angle
    {
        get => RotationDegrees;
        set => RotationDegrees = value;
    }
    
    
    public Vector2 Velocity { get; set; } = Vector2.Zero;

    
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        
        delta *= Engine.PhysicsTicksPerSecond; // Normalize delta

        Position += Velocity * (float)delta;
        Velocity *= Damp;
    }
}