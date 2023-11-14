using Godot;
using Nort.UI;
using System;

namespace Nort.Listing;

public partial class PartsListItem : Button
{
	public event Action Picked;

	private TextureRect frame;
	private DisplayPart displayPart;
	private Label countLabel;

	public PartData PartData => displayPart.PartData;
	private int _count = 1;

	public int Count
	{
		get => _count;
		set => SetCount(value);
	}

	public Color Color
	{
		set
		{
			displayPart.Modulate = value;
			frame.SelfModulate = value;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		frame = GetNode<TextureRect>("Frame");
		displayPart = GetNode<DisplayPart>("DisplayPart");
		countLabel = GetNode<Label>("Count");
	}

	public void SetPart(PartData partData)
	{
		_count = int.MaxValue;
		frame.Material = partData.shiny ? Assets.SHINY_MATERIAL : null;
		displayPart.PartData = partData;
	}

	public void SetPart(Part part)
	{
		_count = int.MaxValue;
		PartData partData = PartData.From(part);
		frame.Material = partData.shiny ? Assets.SHINY_MATERIAL : null;
		displayPart.PartData = partData;
	}

	private void SetCount(int value)
	{
		_count = value;

		if (value == 0)
		{
			Modulate = Modulate with { A = 0.5f };
			countLabel.Text = "";
		}
		else
		{
			Modulate = Modulate with { A = 1 };
			countLabel.Text = value > 1 && value < 10000 ? $"x{value}" : "";
		}
	}

	private void _onMouseEntered()
	{
		displayPart.Outline = true;
	}

	private void _onMouseExited()
	{
		displayPart.Outline = false;
	}

	private void _onButtonDown()
	{
		if (_count > 0 || Game.Instance.Dev)
		{
			GD.Print($"PartsListItem: Start drag {PartData.partId}");
			//DragEmitter.Drag(this, PartData);
			Picked?.Invoke();
		}
	}
}
