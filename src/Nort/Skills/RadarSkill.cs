using Godot;
using System;
using Nort.Entities;

namespace Nort.Skills;

public partial class RadarSkill : Node2D, ISkillNode
{
    #region ISkillNode Implementation
    
    public event Action Fired;

    public CraftPart Part { get; set; }
    
    public float CooldownMax => 0;
    public float Cooldown => 0;
    public Texture2D Texture => GetNode<Sprite2D>("Sprite2D").Texture;

    #endregion
}
