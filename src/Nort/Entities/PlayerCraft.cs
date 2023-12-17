using System.Collections.Generic;
using System.Linq;
using CtrlRaul.Godot;
using Godot;
using Nort.Entities.Components;
using Nort.Hud;
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


    [Ready] public FlightComponent flightComponent;
    [Ready] public Label label;


    public PlayerCraft() : base()
    {
        faction = Assets.Instance.PlayerFaction;
    }


    public override void _Ready()
    {
        base._Ready();
        label.Visible = Game.Instance.InMissionEditor;
        SetProcessUnhandledInput(!Game.Instance.InMissionEditor);
        SetPhysicsProcess(!Game.Instance.InMissionEditor);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton inputEventMouseButton)
        {
            switch (inputEventMouseButton.ButtonIndex)
            {
                case MouseButton.Left:
                    if (!inputEventMouseButton.Pressed)
                    {
                        Entity entity = Stage.Instance.GetEntityOnMouse();

                        switch (entity)
                        {
                            case OrphanPart:
                                coreTractor.SetTarget(entity);
                                break;
                            
                            case DroneCraft:
                                coreTractor.SetTarget(entity);
                                break;
                        }
                    }

                    break;
            }
        }
    }
    
    // public override void _Process(double delta)
    // {
    //     if (Engine.GetFramesDrawn() % 4 == 0)
    //     {
    //         
    //     }
    // }
    
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        flightComponent.Direction = GetKeyboardMotionDirection();
    }


    protected override void SetBlueprint(Blueprint value)
    {
        base.SetBlueprint(value);

        if (!IsInsideTree())
            return;

        label.Position = label.Position with { Y = blueprintVisualRect.Position.Y + blueprintVisualRect.Size.Y + 20 };
    }

    private void OnCollectionRangeAreaEntered(Area2D area)
    {
        if (Game.Instance.InMissionEditor)
            return;
        
        if (area.Owner is OrphanPart orphanPart)
        {
            orphanPart.Collect();
        }
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
}