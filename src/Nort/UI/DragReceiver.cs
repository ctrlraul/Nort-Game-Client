using System;
using Godot;

namespace Nort.UI;

public partial class DragReceiver : Control
{
    [Signal] public delegate void DragEnterEventHandler();
    [Signal] public delegate void DragLeaveEventHandler();
    [Signal] public delegate void DragOverEventHandler();
    [Signal] public delegate void GotDataEventHandler(Control source, object data);

    private ColorRect colorRect;

    private bool mouseOver = false;

    public override void _Ready()
    {
        base._Ready();

        colorRect = GetNode<ColorRect>("ColorRect");

        Visible = false;
        SetProcessInput(false);

        DragEmitter.Instance.DragStart += OnDragStart;
        DragEmitter.Instance.DragStop += OnDragStop;

        if (!GameConfig.Debug)
        {
            Modulate = Colors.Transparent;
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        DragEmitter.Instance.DragStart -= OnDragStart;
        DragEmitter.Instance.DragStop -= OnDragStop;
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (@event is InputEventMouseMotion)
        {
            EmitSignal(SignalName.DragOver);
        }
    }

    private void OnDragStart(Control source, object data)
    {
        Visible = true;

        // When a Control node is made visible, and the mouse cursor is already on top
        // of it, it doesn't get a mouse_enter signal until the cursor is moved.
        // This is a workaround to simulate moving the cursor.
        GetViewport().PushInput(
            new InputEventMouseMotion
            {
                Position = GetGlobalMousePosition()
            }
        );
    }

    private void OnDragStop(Control source, RefCounted data)
    {
        Visible = false;

        if (mouseOver)
        {
            mouseOver = false;
            SetProcessInput(false);
            EmitSignal(SignalName.GotData, source, data);
        }
    }

    private void OnMouseEntered()
    {
        mouseOver = true;
        DragEnter?.Invoke();
        SetProcessInput(true);
    }

    private void OnMouseExited()
    {
        mouseOver = false;
        EmitSignal(SignalName.DragLeave);
        SetProcessInput(false);
    }
}
