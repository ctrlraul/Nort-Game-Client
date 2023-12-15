using Godot;

namespace Nort;

public partial class Grid : Node2D
{
	[Export] private Camera2D camera;
	[Export] private Color color = Colors.White;
	[Export] public int size = 64;
	[Export] public bool disabled;
	
	public override void _Draw()
	{
		if (disabled)
			return;

		Vector2 scaledViewportSize = GetViewportRect().Size / camera.Zoom / 2;
		Vector2 cam = camera.Position;

		for (int i = (int)((cam.X - scaledViewportSize.X) / size) - 1; i <= (int)((scaledViewportSize.X + cam.X) / size) + 1; i++)
		{
			DrawLine(new Vector2(i * size, cam.Y + scaledViewportSize.Y + 100), new Vector2(i * size, cam.Y - scaledViewportSize.Y - 100), color);
		}

		for (int i = (int)((cam.Y - scaledViewportSize.Y) / size) - 1; i <= (int)((scaledViewportSize.Y + cam.Y) / size) + 1; i++)
		{
			DrawLine(new Vector2(cam.X + scaledViewportSize.X + 100, i * size), new Vector2(cam.X - scaledViewportSize.X - 100, i * size), color);
		}
		
		DrawArc(Vector2.Zero, size * 0.25f, 0, Mathf.Pi * 2, 20, color);
	}

	public override void _Process(double delta)
	{
		QueueRedraw();
	}
} 
