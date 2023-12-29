using Godot;
using System;

[Tool]
public partial class KeyboardKey : Control
{
	[Export]
	private string Text
	{
		get => label?.Text ?? "";
		set
		{
			if (label != null)
				label.Text = value;
		}
	}

	private Label label;

	public override void _Ready()
	{
		label = GetNode<Label>("%Label");
	}
}