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

	private PartData partData;

	public Faction Faction
	{
		set => displayPart.Faction = value;
		get => displayPart.Faction;
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


	public void SetPartData(PartData value)
	{
		partData = value;

		if (partData == null)
		{
			Clear();

			return;
		}

		container.Modulate = container.Modulate with { A = 1 };

		displayPart.PartData = partData;

		nameLabel.Text = partData.Part.displayName;
		coreLabel.Text = partData.Part.core.ToString();
		hullLabel.Text = partData.Part.hull.ToString();

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

		if (partData.shiny)
		{
			nameLabel.Text = "Shiny " + nameLabel.Text;
			nameLabel.Modulate = Assets.Instance.GetFactionColor(Faction, true);
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
}
