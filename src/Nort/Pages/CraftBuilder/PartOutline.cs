using Godot;
using Nort.Entities;

namespace Nort.Pages.CraftBuilder;

public partial class PartOutline : Sprite2D
{
	public void SetPart(CraftPart part)
	{
		Position = part.Position;
		Rotation = part.Rotation;
		Texture = part.sprite2D.Texture;
		FlipH = part.sprite2D.FlipH;
		Show();
	}
}