using Godot;
using System;
using System.Collections.Generic;
using CtrlRaul.Godot;
using CtrlRaul.Godot.Linq;
using Nort.Entities;

namespace Nort.Skills;

public partial class CoreBulletSkill : Node2D, ISkillNode
{
    #region ISkillNode Implementation
    
    public event Action Fired;

    public CraftPart Part { get; set; }
    
    public float CooldownMax => (float)cooldownTimer.WaitTime;
    public float Cooldown => (float)cooldownTimer.TimeLeft;
    public Texture2D Texture => sprite2D.Texture;
    public bool Passive => true;

    #endregion
    
    
    [Ready] public Area2D range;
    [Ready] public Timer cooldownTimer;
    [Ready] public Sprite2D sprite2D;

    private CraftPart target;
    public CraftPart Target
    {
        get => target;
        set
        {
            if (value == target)
                return;

            target = value;

            if (IsInstanceValid(target))
                CallDeferred(nameof(Fire));
        }
    }
    

    public override void _Ready()
    {
        this.InitializeReady();
        UpdateCollisionMasks();
        sprite2D.Visible = false;
        Part.Craft.FactionChanged += OnCraftFactionChanged;
    }
    

    private void LookForATarget()
    {
        Target = GetPartsInRange().FindNearest(GlobalPosition, true);
    }

    private void UpdateCollisionMasks()
    {
        range.CollisionMask = Assets.Instance.GetFactionCollisionMask(Part.Faction);
        if (Target != null)
        {
            LookForATarget();
        }
    }
    
    private IEnumerable<CraftPart> GetPartsInRange()
    {
        List<CraftPart> result = new();

        foreach (Area2D overlappingArea in range.GetOverlappingAreas())
        {
            if (overlappingArea is CraftPart { IsDestroyed: false } craftPart)
                result.Add(craftPart);
        }

        return result;
    }

    public void Fire()
    {
        if (Game.Instance.InMissionEditor)
            return;

        if (!IsInsideTree())
        {
            GD.Print("what the fuck lol");

            return;
        }
        
        if (Part.IsDestroyed)
            return;

        if (Target == null)
            return;
        
        if (cooldownTimer.TimeLeft > 0)
            return;

        cooldownTimer.Start();
        CoreBulletProjectile projectile = Stage.Instance.AddEntity<CoreBulletProjectile>();
        projectile.SetSource(this);
        Fired?.Invoke();
    }
    

    private void OnCraftFactionChanged()
    {
        UpdateCollisionMasks();
    }

    private void OnRangeAreaEntered(Area2D area)
    {
        Target ??= (CraftPart)area;
    }

    private void OnRangeAreaExited(Area2D area)
    {
        if (area == Target)
            LookForATarget();
    }
    
    private void OnCooldownTimerTimeout()
    {
        CallDeferred(nameof(Fire));
    }
}