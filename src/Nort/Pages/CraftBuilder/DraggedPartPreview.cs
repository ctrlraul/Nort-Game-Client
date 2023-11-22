using CtrlRaul;
using Godot;
using Nort.UI;

namespace Nort.Pages.CraftBuilder;

public partial class DraggedPartPreview : Control
{
    private DisplayPart displayPart;

    public Color Color
    {
        get => displayPart.Color;
        set => displayPart.Color = value;
    }

    public PartData PartData
    {
        get => displayPart.PartData;
        set
        {
            SetProcessInput(true);
            Position = GetGlobalMousePosition();
            displayPart.PartData = value;
        }
    }

    public override void _Ready()
    {
        base._Ready();
        displayPart = GetNode<DisplayPart>("DisplayPart");
        SetProcessInput(false);
        Hide();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotionEvent)
        {
            Position = mouseMotionEvent.GlobalPosition;
        }
    }

    public void Clear()
    {
        Visible = false;
        SetProcessInput(false);
    }
}
