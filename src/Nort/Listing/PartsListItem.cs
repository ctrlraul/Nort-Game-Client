using Godot;
using Nort.UI;
using CtrlRaul.Godot;

namespace Nort.Listing;

public partial class PartsListItem : Button
{
	[Ready] public TextureRect frame;
	[Ready] public DisplayPart displayPart;
	[Ready] public Label countLabel;

	private bool outlineEnabled;
	
	public Color Color
	{
		get => displayPart.Color;
		set
		{
			displayPart.Color = value;
			frame.SelfModulate = value;
		}
	}

	public PartData PartData
	{
		get => displayPart.PartData;
		set
		{
			displayPart.PartData = value;
			((ShaderMaterial)frame.Material).SetShaderParameter("shiny", value.shiny);
		}
	}

	private int count = 1;

	public int Count
	{
		get => count;
		set
		{
			count = value;
			countLabel.Text = value > 1 ? $"x{value}" : string.Empty;
		}
	}
	

	public override void _Ready()
	{
		base._Ready();
		this.InitializeReady();
	}

	public void SetOutlineEnabled(bool value)
	{
		if (outlineEnabled)
		{
			MouseEntered -= OnMouseEntered;
			MouseExited -= OnMouseExited;
			displayPart.Outline = false;
		}

		outlineEnabled = value;

		if (outlineEnabled)
		{
			MouseEntered += OnMouseEntered;
			MouseExited += OnMouseExited;
		}
	}
	

	private void OnMouseEntered()
	{
		displayPart.Outline = true;
	}

	private void OnMouseExited()
	{
		displayPart.Outline = false;
	}
}
