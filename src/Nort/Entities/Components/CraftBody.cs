using System;
using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot;
using CtrlRaul.Godot.Linq;
using Godot;

namespace Nort.Entities.Components;

public partial class CraftBody : EntityComponent
{
    public event Action<CraftBodyPart, SkillNode, float> PartTookHit;
    public event Action<CraftBodyPart> PartDestroyed;

    [Export] private PackedScene craftBodyPartScene;

    [Ready] public Area2D parts;
    [Ready] public Node2D skillsContainer;
    
    public CraftBodyPart Core { get; private set; }

    
    private Faction faction;
    public Faction Faction
    {
        get => faction;
        set
        {
            faction = value;

            if (IsInsideTree())
            {
                foreach (CraftBodyPart part in GetParts())
                    part.Faction = Faction;
            }
        }
    }
    
    
    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();
    }

    
    public void SetBlueprint(Blueprint blueprint)
    {
        Clear();
        
        foreach (BlueprintPart blueprintPart in blueprint.hulls)
            AddPart(blueprintPart);

        Core = AddPart(blueprint.core);
    }

    public CraftBodyPart GetPart(int index)
    {
        return parts.GetChild<CraftBodyPart>(index);
    }

    public IEnumerable<CraftBodyPart> GetParts()
    {
        return parts.GetChildren().Cast<CraftBodyPart>();
    }

    private CraftBodyPart AddPart(BlueprintPart blueprint)
    {
        CraftBodyPart part = craftBodyPartScene.Instantiate<CraftBodyPart>();

        part.Faction = Faction;
        part.Blueprint = blueprint;

        if (!Game.Instance.InMissionEditor)
        {
            part.Destroyed += () => PartDestroyed?.Invoke(part);
            part.HitTaken += (from, damage) => PartTookHit?.Invoke(part, from, damage);
        }
        
        parts.AddChild(part);

        foreach (SkillNode skill in part.skillNodes)
            skillsContainer.AddChild(skill);

        return part;
    }

    private void Clear()
    {
        skillsContainer.QueueFreeChildren();
        parts.QueueFreeChildren();
    }


    // public static bool IsCraftBodyArea(Area2D area, out CraftBody craftBody)
    // {
    //     if (area is { Owner: CraftBody })
    //     {
    //         craftBody = area.Owner as CraftBody;
    //         return true;
    //     }
    //
    //     craftBody = null;
    //     return false;
    // }
}
