using Godot;

namespace Nort.UI;

public partial class DisplayCraftPart : Control
{
    private DisplayPart displayPart;

    public Faction Faction
    {
        get => displayPart.Faction;
        set => displayPart.Faction = value;
    }
	
    public bool Flipped
    {
        get => displayPart.Flipped;
        set => displayPart.Flipped = value;
    }
	
    public float Angle
    {
        get => displayPart.Angle;
        set => displayPart.Angle = value;
    }
	
    public PartData partData;

    public BlueprintPart Blueprint
    {
        get
        {
            return new BlueprintPart
            {
                partId = partData.partId,
                skillId = partData.skillId,
                shiny = partData.shiny,
                Place = Position,
                flipped = Flipped,
                angle = Angle % 360
            };
        }
        set
        {
            partData = PartData.From(value);
            Position = value.Place;
            displayPart.Flipped = value.flipped;
            displayPart.Angle = value.angle;
            displayPart.PartData = partData;
        }
    }

    public override void _Ready()
    {
        base._Ready();
        displayPart = GetNode<DisplayPart>("%DisplayPart");
    }
}