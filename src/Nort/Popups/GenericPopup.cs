using System;
using Godot;

namespace Nort.Popups;

public partial class GenericPopup : CanvasLayer
{
	[Signal]
	public delegate void RemovedEventHandler();


	protected PanelContainer window;
	protected AnimationPlayer animationPlayer;

	public bool Canceled { get; private set; }


	private bool cancelable = false;
	public bool Cancelable
	{
		set
		{
			SetProcessUnhandledInput(value);
			cancelable = value;
		}

		get => cancelable;
	}

	public float Width
	{
		set => window.CustomMinimumSize = window.CustomMinimumSize with { X = value };
		get => window.CustomMinimumSize.X;
	}


	public override void _Ready()
	{
		window = GetNode<PanelContainer>("%Window");
		animationPlayer = GetNode<AnimationPlayer>("%AnimationPlayer");

		Visible = false;
		animationPlayer.Play("appear");
	}

	public override void _UnhandledInput(InputEvent _event)
	{
		if (Input.IsActionJustPressed("escape"))
		{
			Cancel();
		}
	}

	public override void _Notification(int what)
	{
		if (what == NotificationWMGoBackRequest)
		{
			Cancel();
		}
	}

	
	public void Remove()
	{
		animationPlayer.Play("remove");
		EmitSignal(SignalName.Removed);
	}


	public void Cancel()
	{
		if (cancelable)
		{
			Canceled = true;
			Remove();
		}
	}


	public void SetError()
	{
		window.ThemeTypeVariation = "PanelContainerRuby";
	}

	public void SetWarn()
	{
		window.ThemeTypeVariation = "PanelContainerRuby";
	}


	public void OnOutsidePressed()
	{
		Cancel();
	}


	public void OnAnimationPlayerAnimationFinished(StringName animName)
	{
		if (animName == "remove")
		{
			QueueFree();
		}
	}
}