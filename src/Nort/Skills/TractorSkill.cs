using CtrlRaul.Godot;
using Godot;
using Nort.Entities;

namespace Nort.Skills;

public partial class TractorSkill : Node2D
{
    [Ready] public Line2D line2D;
    
    public Entity Target { get; private set; }
    
    
    public override void _Ready()
    {
        this.InitializeReady();
        SetPhysicsProcess(false);
    }
    
    public override void _Process(double delta)
    {
        if (Target == null)
        {
            GlobalRotation = 0;
        }
        else
        {
            LookAt(Target.Position);
            line2D.Points[1] = new Vector2(0, GlobalPosition.DistanceTo(Target.Position));
        }
    }


    public void Fire(Entity target)
    {
        if (Target != null)
            DisconnectTargetEvents(Target);
        
        Target = target;
        
        ConnectTargetEvents(Target);
    }
    

    private void ConnectTargetEvents(Entity target)
    {
        switch (target)
        {
            case Craft craft:
                craft.Destroyed += OnTargetDestroyed;
                break;
            
            case OrphanPart orphanPart:
                orphanPart.Collected += OnTargetCollected;
                break;
        }
    }

    private void DisconnectTargetEvents(Entity target)
    {
        switch (target)
        {
            case Craft craft:
                craft.Destroyed -= OnTargetDestroyed;
                break;
            
            case OrphanPart orphanPart:
                orphanPart.Collected -= OnTargetCollected;
                break;
        }
    }


    private void OnTargetDestroyed(Craft craft)
    {
        DisconnectTargetEvents(Target);
        Target = null;
    }

    private void OnTargetCollected(PartData partData)
    {
        DisconnectTargetEvents(Target);
        Target = null;
    }
}