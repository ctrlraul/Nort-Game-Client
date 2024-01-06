using System;
using Godot;
using Nort.Entities;

namespace Nort.Skills;

public interface ISkillNode
{
    public event Action Fired;
    
    public CraftPart Part { get; set; }
    public float CooldownMax { get; }
    public float Cooldown { get; }
    public Texture2D Texture { get; }
    public bool Passive { get; }

    public void Fire();
}