using Nort.Interface;

namespace Nort.Entities.Components;

using Godot;

public partial class PlayerControlsComponent : EntityComponent
{
    [Export]
    private NodePath cursorAreaPath;
    private Area2D cursorArea;

    private Area2D interactable;
    private Craft craft;

    /*public override void _Ready()
    {
        base._Ready();
        playerCraft = entity as PlayerCraft;
        SetProcessUnhandledInput(false);
        cursorArea = GetNode<Area2D>(cursorAreaPath);
    }

    public override void _Process(double delta)
    {
        cursorArea.GlobalPosition = GetGlobalMousePosition();
        UpdateInteractable();
    }

    public override void _PhysicsProcess(double delta)
    {
        //playerCraft.FlightComp.Direction = GetKeyboardMotionDirection();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (Input.IsActionJustPressed("select"))
            Interact();
    }

    private Vector2 GetKeyboardMotionDirection()
    {
        Vector2 direction = Vector2.Zero;

        if (Input.IsActionPressed("move_left"))
            direction.X -= 1;
        if (Input.IsActionPressed("move_right"))
            direction.X += 1;
        if (Input.IsActionPressed("move_up"))
            direction.Y -= 1;
        if (Input.IsActionPressed("move_down"))
            direction.Y += 1;

        return direction;
    }

    private void Interact()
    {
        if (interactable == null)
        {
            GD.Print("Interaction with interactable is not implemented");
            return;
            // Logger.error_static("Player Controls", "Interaction with '%s' is not implemented" % __interactable);
        }

        if (interactable.Owner is TractorTargetComponent)
        {
            playerCraft.TractorComp.target = interactable.Owner as TractorTargetComponent;
        }
    }

    private void UpdateInteractable()
    {
        var areas = cursorArea.GetOverlappingAreas();
        var nearestArea = NodeUtils.FindNearest(areas, cursorArea.GlobalPosition);

        if (nearestArea != interactable)
        {
            if (interactable != null && interactable.Owner is TractorTargetComponent)
            {
                (interactable.Owner as TractorTargetComponent).inRange = false;
            }

            interactable = nearestArea;

            if (nearestArea == null)
            {
                SetProcessUnhandledInput(false);
            }
            else
            {
                SetProcessUnhandledInput(true);
                if (interactable.Owner is TractorTargetComponent)
                {
                    (interactable.Owner as TractorTargetComponent).inRange = true;
                }
            }
        }
    }

    private void _OnCursorAreaAreaEntered(Area2D area)
    {
        UpdateInteractable();
    }

    private void _OnCursorAreaAreaExited(Area2D area)
    {
        UpdateInteractable();
    }*/
}
