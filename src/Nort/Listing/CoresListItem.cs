using Godot;
using Nort.UI;

namespace Nort.Listing;

public partial class CoresListItem : Button
{
	[Export] private ShaderMaterial ShinyPartMaterial;

	private DisplayPart displayPart;

	public Faction Faction
	{
		get => displayPart.Faction;
		set => displayPart.Faction = value;
	}

	private PartData _partData;
	public PartData PartData
	{
		set
		{
			displayPart.PartData = value;
			_partData = value;
		}
		get => _partData;
	}

	public override void _Ready()
	{
		base._Ready();
		displayPart = GetNode<DisplayPart>("%DisplayPart");
	}

	public void OnMouseEntered()
	{
		displayPart.Outline = true;
	}

	public void OnMouseExited()
	{
		displayPart.Outline = false;
	}
}