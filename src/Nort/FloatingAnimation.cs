using Godot;
using CtrlRaul.Godot;

namespace Nort;

public partial class FloatingAnimation : RemoteTransform2D
{
	[Ready] public AnimationPlayer animationPlayer;
	
	public override void _Ready()
	{
		this.InitializeReady();
		animationPlayer.Play("float");
	}
}
