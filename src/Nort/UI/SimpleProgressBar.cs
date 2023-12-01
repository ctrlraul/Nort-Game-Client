using CtrlRaul.Godot;
using Godot;

namespace Nort.UI;

public partial class SimpleProgressBar : Control
{
    [Ready] public ColorRect bar;

    private Color color; 
    [Export] public Color Color
    {
        get => color;
        set
        {
            color = value;
            if (IsInsideTree())
                bar.Color = color;
        }
    }

    private float progress;
    [Export] public float Progress
    {
        get => progress;
        set
        {
            progress = value;
            if (IsInsideTree())
                bar.Scale = bar.Scale with { X = Mathf.Clamp(progress, 0, 1) };
        }
    }

    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();
        bar.Color = Color;
        bar.Scale = bar.Scale with { X = Progress };
    }
}