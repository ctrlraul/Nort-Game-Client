using Godot;
using System.Collections.Generic;
using System.Linq;
using CtrlRaul;
using CtrlRaul.Godot;
using Nort.Entities;
using Nort.Entities.Components;

namespace Nort.Skills;

public partial class BulletSkillNode : Node2D, ISkillNode
{
    #region ISkillNode Implementation
    
    public event Action Fired;

    public CraftBodyPart Part { get; set; }
    
    public float CooldownMax => (float)cooldownTimer.WaitTime;
    public float Cooldown => (float)cooldownTimer.TimeLeft;
    public Texture2D Texture => GetNode<Sprite2D>("Sprite2D").Texture;

    #endregion
    
    
    private const float Damage = 3;

    [Ready] public Area2D rangeArea;
    [Ready] public CollisionShape2D rangeAreaCollisionShape2D;
    [Ready] public RayCast2D rayCast2D;
    [Ready] public GpuParticles2D particles;
    [Ready] public Timer cooldownTimer;
    
    
    private readonly Dictionary<Craft, List<CraftBodyPart>> foePartsInRange = new();
    
    private CraftBodyPart target;
    public CraftBodyPart Target
    {
        get => target;
        set => SetTarget(value);
    }

    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();
        
        if (cooldownTimer.WaitTime < particles.Lifetime)
            Logger.Warn(GetType().Name, "Firing cooldown should be greater than the particles' lifespan");
        
        rangeArea.CollisionMask = Assets.Instance.GetFactionCollisionMask(part.Faction);
        rayCast2D.CollisionMask = rangeArea.CollisionMask;
        
        SetPhysicsProcess(false);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        LookAt(Target.GlobalPosition);
    }

    public void Fire()
    {
        if (Game.Instance.InMissionEditor)
            return;
        
        if (Target == null || target.IsDestroyed)
            return;
        
        if (cooldownTimer.TimeLeft > 0)
            return;

        cooldownTimer.Start();
        particles.Emitting = true;
        
        if (rayCast2D.GetCollider() is Area2D collider)
        {
            uint shapeId = (uint)rayCast2D.GetColliderShape();
            CraftBodyPart partHit = collider.ShapeOwnerGetOwner(shapeId) as CraftBodyPart;
            target.Craft.TakeHit(partHit, this, Damage);
            Fired?.Invoke();
        }
    }

    private void SetTarget(CraftBodyPart value)
    {
        if (value == target)
            return;

        if (target is { IsDestroyed: false })
        {
            target.Destroyed -= OnTargetDestroyed;
        }

        target = value;

        if (target != null)
        {
            target.Destroyed += OnTargetDestroyed;
            SetPhysicsProcess(true);
            
            LookAt(Target.GlobalPosition);
            rayCast2D.ForceRaycastUpdate();
            Fire();
        }
        else
        {
            Rotation = 0;
            SetPhysicsProcess(false);
        }
    }


    private void OnRangeAreaAreaShapeEntered(Rid areaRid, Area2D area, ulong areaShapeIndex, ulong localShapeIndex)
    {
        if (area.Owner is Craft craft)
        {
            CraftBodyPart foePart = craft.GetPart((uint)areaShapeIndex);
            
            if (foePartsInRange.TryGetValue(craft, out List<CraftBodyPart> foeParts))
            {
                foeParts.Add(foePart);
            }
            else
            {
                foePartsInRange.Add(craft, new(){ foePart });
            }
            
            Target ??= foePart;
        }
    }


    private void OnRangeAreaAreaShapeExited(Rid areaRid, Area2D area, ulong areaShapeIndex, ulong localShapeIndex)
    {
        if (!IsInstanceValid(area)) // Exited because ded
            return;
        
        if (area.Owner is Craft craft)
        {
            List<CraftBodyPart> foeParts = foePartsInRange[craft];
            
            if (foeParts.Count == 1)
            {
                foeParts.Clear(); // Is this needed or does C# handle it properly?
                foePartsInRange.Remove(craft);
                
                Target = null;
            }
            else
            {
                CraftBodyPart foePart = craft.GetPart((uint)areaShapeIndex);
                foeParts.Remove(foePart);
                
                if (Target == foePart)
                {
                    Target = foeParts[0];
                }
            }
        }
    }

    private void OnCooldownTimerTimeout()
    {
        Fire();
    }
    
    private void OnTargetDestroyed()
    {
        Target = foePartsInRange.Any() ? foePartsInRange.First().Value[0] : null;
    }
    
    
    // private void OnRangeAreaAreaEntered(Area2D area)
    // {
    //     if (area.Owner is Craft craft && Faction.Hostile(craft.Faction, part.Faction))
    //     {
    //         foesInRange.Add(craft);
    //         Target ??= craft;
    //     }
    // }
    //
    // private void OnRangeAreaAreaExited(Area2D area)
    // {
    //     if (area.Owner is Craft craft && Faction.Hostile(craft.Faction, part.Faction))
    //     {
    //         foesInRange.Remove(craft);
    //
    //         if (Target == craft)
    //         {
    //             Target = foesInRange.Any() ? foesInRange[0] : null;
    //         }
    //     }
    // }
} 