using Godot;
using System;
using CtrlRaul;
using CtrlRaul.Interfaces;

namespace Nort.UI.Overlays;

public partial class ConsoleOverlay : CanvasLayer, ILogger
{
	private static readonly Color InfoColor = new(0.8f, 0.8f, 0.85f);
	private static readonly Color WarnColor = new(0.9f, 0.9f, 0.5f);
	private static readonly Color ErrorColor = new(0.9f, 0.4f, 0.4f);
	private static readonly LabelSettings ConsoleLabelSettings = GD.Load<LabelSettings>("res://OtherResources/ConsoleLabelSettings.tres");
	
	private Control linesContainer;
	
	
	public override void _Ready()
	{
		base._Ready();
		linesContainer = GetNode<Control>("%LinesContainer");
		Visible = false;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);
		if (Input.IsActionJustPressed("toggle_console"))
		{
			Visible = !Visible;
			GetViewport().SetInputAsHandled();
		}
	}
	

	public void Log(object message)
	{
		Label line = new();
		line.Text = message.ToString();
		line.LabelSettings = ConsoleLabelSettings;
		line.Modulate = InfoColor;
		linesContainer.AddChild(line);
	}

	public void Warn(object message)
	{
		Label line = new();
		line.Text = message.ToString();
		line.LabelSettings = ConsoleLabelSettings;
		line.Modulate = WarnColor;
		linesContainer.AddChild(line);
	}

	public void Error(object message)
	{
		Label line = new();
		line.Text = message.ToString();
		line.LabelSettings = ConsoleLabelSettings;
		line.Modulate = ErrorColor;
		linesContainer.AddChild(line);
	}
}
