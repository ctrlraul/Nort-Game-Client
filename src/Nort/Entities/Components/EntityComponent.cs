using Godot;

namespace Nort.Entities.Components;

public partial class EntityComponent : Node2D
{
    public Entity Entity { get; private set; }
    public Craft Craft { get; private set; }
    public OrphanPart OrphanPart { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        Entity = GetParent<Entity>();
        Craft = Entity as Craft;
        OrphanPart = Entity as OrphanPart;
    }
}