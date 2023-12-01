using Godot;
using System;

namespace Nort.UI.Overlays;

public partial class PauseOverlay : CanvasLayer
{
    public event Action Unpaused;
    public event Action Forfeit;
    public event Action Quit;

    private SceneTree tree;

    public override void _Ready()
    {
        base._Ready();
        tree = GetTree();
        tree.Paused = true;
    }

    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionJustPressed("escape"))
            Unpause();
    }

    private void Unpause()
    {
        tree.Paused = false;
        QueueFree();
        Unpaused?.Invoke();
    }

    private void OnTreeExiting()
    {
        tree.Paused = false;
    }

    private void OnContinueButtonPressed()
    {
        Unpause();
    }

    private void OnForfeitButtonPressed()
    {
        Forfeit?.Invoke();
    }

    private void OnQuitButtonPressed()
    {
        Quit?.Invoke();
    }
}