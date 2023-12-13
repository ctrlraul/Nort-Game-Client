using System;
using Godot;
using Nort.Pages;

namespace Nort.Entities;

public partial class Entity : Node2D
{
    [Savable] public string Type => GetType().FullName;
    
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
}