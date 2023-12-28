using Godot;
using System;

namespace Nort;

public partial class CoreExplosion : Node2D
{
	public override void _Ready()
	{
		GetNode<GpuParticles2D>("GPUParticles2D").Emitting = true;
		AudioManager.Instance.PlayExplosion(GlobalPosition);
	}

	private void OnGpuParticles2dFinished()
	{
		QueueFree();
	}
}
