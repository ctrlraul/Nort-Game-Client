using System;
using System.Linq;
using CtrlRaul.Godot;
using Godot;
using Nort.Entities;

namespace Nort.Skills;

public partial class DroneSkillNode : SkillNode
{
    [Ready] public Timer cooldownTimer;
    
    public DroneCraft Drone { get; private set; }

    public override void _Ready()
    {
        this.InitializeReady();
        SetPhysicsProcess(false);
    }

    public override void Fire()
    {
        if (Drone != null)
            return;
        
        if (cooldownTimer.TimeLeft > 0)
            return;
        
        cooldownTimer.Start();

        Drone = Stage.Instance.Spawn<DroneCraft>();
        Drone.Position = GlobalPosition;
        Drone.Blueprint = Assets.Instance.GetBlueprint(Config.DroneBlueprints.First());
    }
}