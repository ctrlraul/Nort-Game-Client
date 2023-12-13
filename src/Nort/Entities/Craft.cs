using System;
using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot;
using Godot;
using Nort.Entities.Components;
using Nort.Hud;
using Nort.Skills;

namespace Nort.Entities;

public partial class Craft : Entity
{
    #region EntityInspector compatibility

    public IEnumerable<string> FactionIdOptions => Assets.Instance.GetFactions().Select(f => f.id);
    
    [Inspect(nameof(FactionIdOptions))]
    public string FactionId
    {
        get => Faction.id;
        set => Faction = Assets.Instance.GetFaction(value);
    }

    #endregion


    public event Action Destroyed;

    public enum ComponentSet
    {
        None,
        Player,
        Fighter,
        Drone,
        Turret,
        Carrier,
        Outpost
    }

    [Ready] public CraftBody body;
    [Ready] public CollisionShape2D editorHitBoxShape;
    [Ready] public Node2D editorStuff;


    protected Faction faction = Assets.Instance.DefaultEnemyFaction;

    public Faction Faction
    {
        get => faction;
        set => SetFaction(value);
    }

    protected Blueprint blueprint = Assets.Instance.InitialBlueprint;

    public Blueprint Blueprint
    {
        get => blueprint;
        set => SetBlueprint(value);
    }

    protected Rect2 blueprintVisualRect;


    public float CoreMax { get; private set; }
    public float Core { get; private set; }
    public float HullMax { get; private set; }
    public float Hull { get; private set; }


    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();

        SetBlueprint(Blueprint);
        SetFaction(Faction);

        if (Game.Instance.InMissionEditor)
        {
            editorHitBoxShape.Disabled = false;
        }
        else
        {
            editorStuff.QueueFree();
            body.PartTookHit += OnPartTookHit;
        }
    }


    protected virtual void UpdateEditorStuff()
    {
        if (!IsInsideTree())
            return;
        
        editorHitBoxShape.Position = blueprintVisualRect.Position + blueprintVisualRect.Size * 0.5f;
        editorHitBoxShape.Shape = new RectangleShape2D { Size = blueprintVisualRect.Size };
    }


    private void SetBlueprint(Blueprint value)
    {
        blueprint = value;
        blueprintVisualRect = Assets.Instance.GetBlueprintVisualRect(Blueprint);

        if (IsInsideTree())
            body.SetBlueprint(blueprint);

        BlueprintStats stats = Blueprint.GetStats(blueprint);
        CoreMax = stats.core;
        HullMax = stats.hull;

        UpdateEditorStuff();
    }
    
    private void SetFaction(Faction value)
    {
        faction = value;

        if (IsInsideTree())
            body.Faction = Faction;
    }


    public void Destroy()
    {
        Hull = 0;
        Core = 0;

        foreach (CraftBodyPart part in body.GetParts())
            part.Destroy();

        QueueFree();
        Destroyed?.Invoke();
    }


    private void OnPartTookHit(CraftBodyPart part, SkillNode from, float damage)
    {
        if (from is BulletSkillNode)
        {
            Hull -= damage;

            if (Hull >= 0)
                return;

            if (part == body.Core)
            {
                Core += Hull;

                if (Core <= 0)
                    Destroy();
            }
            else
            {
                part.TakeDamage(Hull * -1);
            }

            Hull = 0;
        }
    }
}