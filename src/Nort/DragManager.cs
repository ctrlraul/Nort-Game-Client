using System;
using Godot;

namespace Nort;

public partial class DragManager : Node
{
    public event Action<DragData> DragStart;
    public event Action<DragData> DragStop;
    public static DragManager Instance { get; private set; }

    public DragData DragData { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        Instance = this;
        SetProcessInput(false);
    }

    public override void _Input(InputEvent inputEvent)
    {
        base._Input(inputEvent);
        
        if (inputEvent is not InputEventMouseButton { Pressed: false })
            return;

        DragStop?.Invoke(DragData);
        DragData = null;
        SetProcessInput(false);
    }

    public void Drag(Control source, object data)
    {
        if (DragData != null)
            throw new Exception("Already dragging");
        
        DragData = new DragData(source, data);
        DragStart?.Invoke(DragData);
        SetProcessInput(true);
    }
}