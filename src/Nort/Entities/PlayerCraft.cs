using System;
using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot;
using CtrlRaul.Godot.Linq;
using Godot;
using Nort.Entities.Components;
using Nort.Pages;

namespace Nort.Entities;

public partial class PlayerCraft : Craft
{
    #region EntityInspector compatibility

    [Savable]
    [Inspect(nameof(TestBlueprintIdOptions))]
    public string TestBlueprintId
    {
        get => Blueprint.id;
        set => Blueprint = Assets.Instance.GetBlueprint(value);
    }

    public IEnumerable<string> TestBlueprintIdOptions => Assets.Instance.GetBlueprints().Select(b => b.id);

    #endregion


    public event Action<InteractionRange> InteractableFocusedChanged;
    

    [Ready] public FlightComponent flightComponent;
    [Ready] public CoreTractor coreTractor;
    [Ready] public Node2D controllerIcon;

    private List<InteractionRange> interactablesInRange = new();

    private InteractionRange interactableFocused;

    public InteractionRange InteractableFocused
    {
        get => interactableFocused;
        set
        {
            if (interactableFocused == value)
                return;

            interactableFocused = value;
            InteractableFocusedChanged?.Invoke(interactableFocused);
        }
    }


    public PlayerCraft() : base()
    {
        faction = Assets.Instance.PlayerFaction;
    }


    public override void _Ready()
    {
        base._Ready();
        controllerIcon.Visible = Game.Instance.InMissionEditor;
        SetProcessUnhandledInput(!Game.Instance.InMissionEditor);
        SetPhysicsProcess(!Game.Instance.InMissionEditor);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        switch (@event)
        {
            case InputEventMouseButton inputEventMouseButton:
                OnInputEventMouseButton(inputEventMouseButton);

                break;

            case InputEventKey inputEventKey:
                OnInputEventKey(inputEventKey);

                break;
        }
    }
    
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        
        flightComponent.Direction = GetKeyboardMotionDirection();

        if (Engine.GetFramesDrawn() % 4 != 0)
            return;

        FindNearestInteractable();
    }


    protected override void SetBlueprint(Blueprint value)
    {
        base.SetBlueprint(value);

        if (!DidSpawn)
            return;

        controllerIcon.Position = controllerIcon.Position with
        {
            Y = blueprintVisualRect.Position.Y + blueprintVisualRect.Size.Y + 40
        };
    }
    
    private static Vector2 GetKeyboardMotionDirection()
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

    private void FindNearestInteractable()
    {
        InteractableFocused = (
            interactablesInRange.Any()
                ? interactablesInRange.FindNearest(GlobalPosition, true)
                : null
        );
    }


    private void OnInputEventKey(InputEventKey inputEventKey)
    {
        if (Game.Instance.InMissionEditor)
            return;

        if (Input.IsActionJustPressed("interact"))
        {
            InteractableFocused?.Interact();
            GetViewport().SetInputAsHandled();
        }
    }

    private void OnInputEventMouseButton(InputEventMouseButton inputEventMouseButton)
    {
        if (Game.Instance.InMissionEditor)
            return;

        switch (inputEventMouseButton.ButtonIndex)
        {
            case MouseButton.Left:
                if (!inputEventMouseButton.Pressed)
                {
                    Entity entity = Stage.Instance.GetEntityOnMouse();

                    if (coreTractor.Target == entity)
                    {
                        coreTractor.Target = null;
                    }
                    else
                    {
                        switch (entity)
                        {
                            case OrphanPart:
                                coreTractor.Target = entity;
                                GetViewport().SetInputAsHandled();

                                break;

                            case DroneCraft:
                                coreTractor.Target = entity;
                                GetViewport().SetInputAsHandled();

                                break;
                        }
                    }
                }

                break;
        }
    }
    
    
    private void OnCollectionRangeAreaEntered(Area2D area)
    {
        if (Game.Instance.InMissionEditor)
            return;
        
        if (area.Owner is OrphanPart { Collectable: true } orphanPart)
        {
            orphanPart.Collect();
        }
    }

    private void OnInteractingRangeAreaEntered(Area2D area)
    {
        if (area is not InteractionRange interactable)
            throw new Exception("Interaction area detected non-interactable object");

        if (!interactablesInRange.Any())
            InteractableFocused = interactable;

        interactablesInRange.Add(interactable);
    }

    private void OnInteractingRangeAreaExited(Area2D area)
    {
        if (area is not InteractionRange interactable)
            throw new Exception("Interaction area detected non-interactable object");

        interactablesInRange.Remove(interactable);

        if (interactable == InteractableFocused)
            FindNearestInteractable();
    }
}