using System;
using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot;
using CtrlRaul.Godot.Linq;
using Godot;

namespace Nort.Entities.Components;

public partial class CraftBodyComponent : EntityComponent
{
    public event Action<CraftBodyPart> PartDestroyed;

    [Export] private PackedScene craftBodyPartScene;

    [Ready] public Node2D partsContainer;
    [Ready] public Node2D skillsContainer;

    public Color Color => Craft.Faction.Color;
    public CraftBodyPart Core { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();
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
        CraftBodyPart part = craftBodyPartScene.Instantiate<CraftBodyPart>();

        partsContainer.AddChild(part);

        part.body = this;
        part.SetBlueprint(blueprint);
        part.Destroyed += () => PartDestroyed?.Invoke(part);

        foreach (CraftBodyPartSkill skill in part.skills)
            skillsContainer.AddChild(skill);

        return part;
    }
}
