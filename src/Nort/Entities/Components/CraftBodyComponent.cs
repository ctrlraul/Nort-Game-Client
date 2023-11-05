using System;
using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot.Linq;
using Godot;

namespace Nort.Entities.Components;

public partial class CraftBodyComponent : EntityComponent
{
    public event Action<CraftBodyPart> PartDestroyed;

    [Export] private static PackedScene _craftBodyPartScene;

    private Node2D partsContainer;
    private Node2D skillsContainer;

    public Craft Craft { get; private set;  }
    private Color color = GameConfig.FactionlessColor;
    public CraftBodyPart Core { get; private set; }

    public override void _Ready()
    {
        partsContainer = GetNode<Node2D>("%PartsContainer");
        skillsContainer = GetNode<Node2D>("%SkillsContainer");
        Craft = entity as Craft;
        partsContainer.QueueFreeChildren();
    }

    public void SetBlueprint(Blueprint blueprint)
    {
        foreach (BlueprintPart blueprintPart in blueprint.hulls)
            AddPart(blueprintPart);

        Core = AddPart(blueprint.core);
    }

    public CraftBodyPart GetPart(int index)
    {
        return partsContainer.GetChild<CraftBodyPart>(index);
    }

    public IEnumerable<CraftBodyPart> GetParts()
    {
        return partsContainer.GetChildren().Cast<CraftBodyPart>();
    }

    private CraftBodyPart AddPart(BlueprintPart blueprint)
    {
        CraftBodyPart part = _craftBodyPartScene.Instantiate<CraftBodyPart>();

        partsContainer.AddChild(part);

        part.Color = Craft.Faction.Color;
        part.body = this;
        part.SetBlueprint(blueprint);
        part.Destroyed += () => PartDestroyed?.Invoke(part);

        foreach (CraftBodyPartSkill skill in part.skills)
            skillsContainer.AddChild(skill);

        return part;
    }
}
