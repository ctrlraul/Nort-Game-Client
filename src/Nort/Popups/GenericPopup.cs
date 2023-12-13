using System;
using CtrlRaul.Godot;
using Godot;

namespace Nort.Popups;

public partial class GenericPopup : CanvasLayer
{
	[Signal]
	public delegate void RemovedEventHandler();


	[Ready] public PanelContainer window;
	[Ready] public AnimationPlayer animationPlayer;

	public bool Canceled { get; private set; }


	private bool cancellable;
	public bool Cancellable
	{
		get => cancellable;
		set
		{
			SetProcessUnhandledInput(value);
			cancellable = value;
		}
	}

	public float Width
	{
		set => window.CustomMinimumSize = window.CustomMinimumSize with { X = value };
		get => window.CustomMinimumSize.X;
	}


	public override void _Ready()
	{
		base._Ready();
		this.InitializeReady();

		Visible = false;
		animationPlayer.Play("appear");
		
		SetProcessUnhandledInput(cancellable);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (Input.IsActionJustPressed("escape"))
		{
			Cancel();
			GetViewport().SetInputAsHandled();
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
		if (cancellable)
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