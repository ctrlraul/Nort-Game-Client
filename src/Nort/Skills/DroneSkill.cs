using System;
using System.Linq;
using CtrlRaul.Godot;
using Godot;
using Nort.Entities;

namespace Nort.Skills;

public partial class DroneSkill : Node2D, ISkillNode
{
    #region ISkillNode Implementation
    
    public event Action Fired;

    public CraftPart Part { get; set; }
    
    public float CooldownMax => (float)cooldownTimer.WaitTime;
    public float Cooldown => (float)cooldownTimer.TimeLeft;
    public Texture2D Texture => GetNode<Sprite2D>("Sprite2D").Texture;

    #endregion
    
    
    [Ready] public Timer cooldownTimer;
    
    public DroneCraft Drone { get; private set; }

    public override void _Ready()
    {
        this.InitializeReady();
        SetPhysicsProcess(false);
    }

    public void Fire()
    {
        if (Drone != null)
            return;
        
        if (cooldownTimer.TimeLeft > 0)
            return;
        
        cooldownTimer.Start();

        Drone = Stage.Instance.Spawn<DroneCraft>();
        Drone.Position = GlobalPosition;
        Drone.Blueprint = Assets.Instance.GetBlueprint(Config.DroneBlueprints.First());
        Drone.Faction = Part.Faction;
        
        Fired?.Invoke();
    }
}