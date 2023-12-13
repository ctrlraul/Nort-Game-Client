using Godot;
using Nort.Entities.Components;

namespace Nort;

public abstract partial class SkillNode : Node2D
{
    public CraftBodyPart part;

    public abstract void Fire();
}