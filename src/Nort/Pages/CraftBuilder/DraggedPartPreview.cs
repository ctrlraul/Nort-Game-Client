using Godot;
using Nort.UI;

namespace Nort.Pages.CraftBuilder;

public partial class DraggedPartPreview : Control
{
    private DisplayPart displayPart;

    public override void _Ready()
    {
        base._Ready();
        displayPart = GetNode<DisplayPart>("DisplayPart");
        SetProcessInput(false);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotionEvent)
        {
            Position = mouseMotionEvent.GlobalPosition;
        }
    }

    public void SetPartData(PartData partData)
    {
        Visible = true;
        Position = GetGlobalMousePosition();
        SetProcessInput(true);
        displayPart.PartData = partData;
    }

    public void SetColor(Color color)
    {
        displayPart.Modulate = color;
    }

    public void Clear()
    {
        Visible = false;
        SetProcessInput(false);
    }
}
