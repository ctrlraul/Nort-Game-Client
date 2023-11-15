using System.Linq;
using Godot;
using Nort.UI;

namespace Nort.Pages.CraftBuilder;

public partial class PartInspector : PanelContainer
{
	private HBoxContainer container;
	private DisplayPart displayPart;
	private Label nameLabel;
	private TextureRect skillIcon;
	private Label skillLabel;
	private Label coreLabel;
	private Label hullLabel;

	private DisplayCraftPart _part = null;

	public Color Color
	{
		set => displayPart.Color = value;
		get => displayPart.Color;
	}

	public override void _Ready()
	{
		base._Ready();
		container = GetNode<HBoxContainer>("%Container");
		displayPart = GetNode<DisplayPart>("%DisplayPart");
		nameLabel = GetNode<Label>("%NameLabel");
		skillIcon = GetNode<TextureRect>("%SkillIcon");
		skillLabel = GetNode<Label>("%SkillLabel");
		coreLabel = GetNode<Label>("%CoreLabel");
		hullLabel = GetNode<Label>("%HullLabel");
	}

	public void SetPart(DisplayCraftPart part)
	{
		_part = part;

		if (_part == null)
		{
			Clear();
		}
		else
		{
			var partData = _part.partData;
			SetPartData(partData);

			if (partData.Skill != null)
			{
				skillIcon.Texture = Assets.Instance.GetSkillTexture(partData.Skill.id);
				skillLabel.Text = partData.Skill.displayName;
			}
			else
			{
				skillIcon.Texture = null;
				skillLabel.Text = "";
			}
		}
	}

	public void SetPartData(PartData partData)
	{
		_SetPartData(partData);

		if (partData.Skill != null)
		{
			skillIcon.Texture = Assets.Instance.GetSkillTexture(partData.Skill.id);
			skillLabel.Text = partData.Skill.displayName;
		}
		else
		{
			skillIcon.Texture = null;
			skillLabel.Text = "";
		}
	}

	private void _SetPartData(PartData partData)
	{
		container.Modulate = container.Modulate with { A = 1 };

		displayPart.PartData = partData;

		nameLabel.Text = partData.Part.displayName;
		coreLabel.Text = partData.Part.core.ToString();
		hullLabel.Text = partData.Part.hull.ToString();

		if (partData.shiny)
		{
			nameLabel.Text = "Shiny " + nameLabel.Text;
			nameLabel.Modulate = new Color(0, 1, 1);
		}
		else
		{
			nameLabel.Modulate = Colors.White;
		}
	}

	public void Clear()
	{
		container.Modulate = container.Modulate with { A = 0 };
	}

	private void OnSkillOptionsItemSelected(int index)
	{
		_part.partData.Skill = Assets.Instance.GetSkills().ElementAt(index);
	}
}
