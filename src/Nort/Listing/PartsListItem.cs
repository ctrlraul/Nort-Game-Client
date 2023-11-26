using CtrlRaul;
using Godot;
using Nort.UI;
using CtrlRaul.Godot;
using CtrlRaul.Interfaces;

namespace Nort.Listing;

public partial class PartsListItem : Button, IListItem<PartData>
{
	#region IListItem Implementation

	public PartData Value { get; private set; }

	public void SetFor(PartData partData)
	{
		Value = partData;
		Count = int.MaxValue;
		frame.Material = partData.shiny ? Assets.SHINY_MATERIAL : null;
		displayPart.PartData = partData;
	}

	#endregion

	[Ready] public TextureRect frame;
	[Ready] public DisplayPart displayPart;
	[Ready] public Label countLabel;

	private bool mouseDown;

	private int _count = 1;
	public int Count
	{
		get => _count;
		set
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
				countLabel.Text = value is > 1 and < 10000 ? $"x{value}" : "";
			}
		}
	}

	public Color Color
	{
		get => displayPart.Color;
		set
		{
			displayPart.Color = value;
			frame.SelfModulate = value;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		this.InitializeReady();
		SetProcessInput(false);
		DragManager.Instance.DragStop += OnDragStop;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		DragManager.Instance.DragStop -= OnDragStop;
	}

	private void OnDragStop(DragData dragData)
	{
		mouseDown = false;
	}

	private void OnMouseEntered()
	{
		displayPart.Outline = true;
		if (!mouseDown)
			GuiInput += OnGuiInput;
	}

	private void OnMouseExited()
	{
		displayPart.Outline = false;
		if (!mouseDown)
			GuiInput -= OnGuiInput;
	}

	private void OnGuiInput(InputEvent inputEvent)
	{
		if (inputEvent is InputEventMouseButton mouseButtonEvent)
		{
			mouseDown = mouseButtonEvent.Pressed;
		}
		else if (inputEvent is InputEventMouseMotion && mouseDown)
		{
			GuiInput -= OnGuiInput;
			DragManager.Instance.Drag(this, displayPart.PartData);
		}
	}
}
