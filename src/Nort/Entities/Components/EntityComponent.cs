using Godot;

namespace Nort.Entities.Components;

public partial class EntityComponent : Node2D
{
    protected Entity entity;

    public override void _Ready()
    {
        entity = GetParent<Entity>();
        _PostReady();
    }

    protected virtual void _PostReady() { }
}