using Godot;

namespace Nort.UI;

public partial class DisplayCraftPart : Control
{
    private DisplayPart displayPart;

    public Color Color
    {
        get => displayPart.Color;
        set => displayPart.Color = value;
    }
	
    public bool Flipped
    {
        get => displayPart.Flipped;
        set => displayPart.Flipped = value;
    }
	
    public float Angle
    {
        get => displayPart.Rotation;
        set => displayPart.Rotation = value;
    }
	
    public PartData partData;

    public BlueprintPart Blueprint
    {
        get => ToBlueprintPart();
        set => FromBlueprintPart(value);
    }

    public override void _Ready()
    {
        displayPart = GetNode<DisplayPart>("%DisplayPart");
    }

    private BlueprintPart ToBlueprintPart()
    {
        return new BlueprintPart
        {
            partId = partData.partId,
            skillId = partData.skillId,
            shiny = partData.shiny,
            Place = Position,
            flipped = displayPart.Flipped,
            angle = displayPart.Angle
        };
    }

    private void FromBlueprintPart(BlueprintPart blueprintPart)
    {
        partData = PartData.From(blueprintPart);
        Position = blueprintPart.Place;
        Flipped = blueprintPart.flipped;
        Angle = blueprintPart.angle;
        displayPart.PartData = partData;
    }
}