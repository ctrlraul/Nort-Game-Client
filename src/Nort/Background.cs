using System;
using Godot;

namespace Nort;

[Tool]
public partial class Background : Sprite2D
{
	private ShaderMaterial shaderMaterial;
	
	[Export] private Camera2D camera;
	
	
	public override void _Ready()
	{
		shaderMaterial = Material as ShaderMaterial;

		if (shaderMaterial == null)
			throw new Exception("Expected shader material");

		if (Engine.IsEditorHint())
		{
			SetProcess(false);
			Scale = new Vector2(
				ProjectSettings.GetSetting("display/window/size/viewport_width").AsInt16(), 
				ProjectSettings.GetSetting("display/window/size/viewport_height").AsInt16()
			);
		}
		else
		{
			GetViewport().SizeChanged += OnViewportSizeChanged;
			OnViewportSizeChanged();
		}
	}

	public override void _Process(double delta)
	{
		Scale = GetViewportRect().Size / camera.Zoom;
		shaderMaterial.SetShaderParameter("position", GlobalPosition / Scale);
		shaderMaterial.SetShaderParameter("zoom", camera.Zoom);
	}


	private void OnViewportSizeChanged()
	{
		Vector2 viewportSize = GetViewportRect().Size;
		Vector2 aspect = (
			viewportSize.X > viewportSize.Y
			? new Vector2(1, viewportSize.Y / viewportSize.X)
			: new Vector2(viewportSize.X / viewportSize.Y, 1)
		);
		
		shaderMaterial.SetShaderParameter("aspect", aspect);
	}
}
