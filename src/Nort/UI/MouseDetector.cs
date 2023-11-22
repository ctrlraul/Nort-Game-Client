using System.Threading.Tasks;
using Godot;

namespace Nort.UI;

public partial class MouseDetector : Control
{
    private bool isMouseDown;
    public bool MouseOver { get; private set; }
    
    public override void _Ready()
    {
        base._Ready();
        Hide();
    }

    public override async void _Input(InputEvent inputEvent)
    {
        base._Input(inputEvent);
        if (inputEvent is InputEventMouseButton mouseButtonEvent)
        {
            isMouseDown = mouseButtonEvent.Pressed;
            if (isMouseDown)
                return;
            await Task.Delay(1);
            Hide();
        }
        else if (isMouseDown && !Visible && inputEvent is InputEventMouseMotion)
        {
            Show();
        }
    }

    private void OnMouseEntered()
    {
        MouseOver = true;
    }

    private void OnMouseExited()
    {
        MouseOver = false;
    }
}