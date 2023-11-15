using System;
using CtrlRaul;
using Godot;
using Nort.UI;

namespace Nort;

public partial class DragEmitter : Node
{
    public static DragEmitter Instance { get; private set; }

    public static DragReceiver CurrentReceiver;
    
    public event Action<Control, RefCounted> DragStart;
    public event Action<Control, RefCounted> DragStop;

    public Control Source { get; private set; }
    public RefCounted Data { get; private set; }

    public DragEmitter()
    {
        Instance = this;
    }
    
    public override void _Ready()
    {
        base._Ready();
        SetProcessInput(false);
    }

    public void Drag(Control source, RefCounted data = null)
    {
        Source = source;
        Data = data;

        DragStart?.Invoke(source, data);

        SetProcessInput(true);
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: false })
        {
            CurrentReceiver?.Receive(Source, Data);
            SetProcessInput(false);
            DragStop?.Invoke(Source, Data);
            Source = null;
            Data = null;
        }
    }
}