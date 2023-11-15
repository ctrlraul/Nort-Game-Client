using CtrlRaul.Godot.Linq;
using Godot.Collections;

namespace Nort.Entities.Components;

using Godot;

public partial class PlayerControlsComponent : EntityComponent
{
    [Export]
    private NodePath cursorAreaPath;
    private Area2D cursorArea;

    private Area2D interactable;
    private FlightComponent flightComponent;
    private TractorComponent tractorComponent;

    public override void _Ready()
    {
        base._Ready();
        SetProcessUnhandledInput(false);
        cursorArea = GetNode<Area2D>(cursorAreaPath);
    }

    public override void Init()
    {
        flightComponent = Craft.GetComponent<FlightComponent>();
        tractorComponent = Craft.GetComponent<TractorComponent>();
    }

    public override void _Process(double delta)
    {
        cursorArea.GlobalPosition = GetGlobalMousePosition();
        UpdateInteractable();
    }

    public override void _PhysicsProcess(double delta)
    {
        flightComponent.Direction = GetKeyboardMotionDirection();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (Input.IsActionJustPressed("select"))
        {
            Interact();
            GetViewport().SetInputAsHandled();
        }
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
            tractorComponent.target = interactable.Owner as TractorTargetComponent;
        }
    }

    private void UpdateInteractable()
    {
        Array<Area2D> areas = cursorArea.GetOverlappingAreas();
        Area2D nearestArea = areas.FindNearest(cursorArea.GlobalPosition, true);

        if (nearestArea == interactable)
            return;
        
        if (interactable is { Owner: TractorTargetComponent })
            ((TractorTargetComponent)interactable.Owner).InRange = false;

        interactable = nearestArea;

        if (nearestArea == null)
        {
            SetProcessUnhandledInput(false);
        }
        else
        {
            SetProcessUnhandledInput(true);
            if (interactable.Owner is TractorTargetComponent component)
                component.InRange = true;
        }
    }

    private void _OnCursorAreaAreaEntered(Area2D area)
    {
        UpdateInteractable();
    }

    private void _OnCursorAreaAreaExited(Area2D area)
    {
        UpdateInteractable();
    }
}
