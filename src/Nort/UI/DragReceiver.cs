using System;
using CtrlRaul;
using Godot;

namespace Nort.UI;

public partial class DragReceiver : Control
{
    public event Action DragEntered;
    public event Action DragExited;
    public event Action<InputEventMouseMotion> DragOver;
    public event Action<DragData> DragDrop;
    
    public bool MouseOver { get; private set; }
    
    public override void _Ready()
    {
        base._Ready();
        DragManager.Instance.DragStart += OnDragStart;
        DragManager.Instance.DragStop += OnDragStop;
        Hide();
        SetProcessInput(false);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        DragManager.Instance.DragStart -= OnDragStart;
        DragManager.Instance.DragStop -= OnDragStop;
    }

    public override void _Input(InputEvent inputEvent)
    {
        base._GuiInput(inputEvent);
        if (inputEvent is InputEventMouseMotion mouseMotionEvent)
            DragOver?.Invoke(mouseMotionEvent);
    }

    private void OnDragStart(DragData dragData)
    {
        Show();
        Modulate = Modulate with { A = 0.2f };
    }

    private void OnDragStop(DragData dragData)
    {
        if (MouseOver)
            DragDrop?.Invoke(dragData);
        Hide();
    }

    private void OnMouseEntered()
    {
        SetProcessInput(true);
        Modulate = Modulate with { A = 0.5f };
        MouseOver = true;
        DragEntered?.Invoke();
    }

    private void OnMouseExited()
    {
        SetProcessInput(false);
        Modulate = Modulate with { A = 0.2f };
        MouseOver = false;
        DragExited?.Invoke();
    }
}