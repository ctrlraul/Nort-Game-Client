using System;
using CtrlRaul.Godot;
using Godot;

namespace Nort.Skills;

public partial class DroneSkillNode : SkillNode
{
    [Ready] public Timer cooldownTimer;

    public override void _Ready()
    {
        this.InitializeReady();
        SetPhysicsProcess(false);
    }

    public override void Fire()
    {
        if (cooldownTimer.TimeLeft > 0)
            return;
        
        cooldownTimer.Start();
        
        GD.Print("Spawn drone");
    }
}