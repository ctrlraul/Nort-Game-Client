using Godot;
using System;

namespace Nort.Entities.Components;

public partial class InteractionRange : Area2D
{
	[Signal]
	public delegate void InteractedEventHandler();

	[Export] public string Label { get; private set; }

	public float Radius => 50;


	public void Interact()
	{
		EmitSignal(SignalName.Interacted);
	}
}