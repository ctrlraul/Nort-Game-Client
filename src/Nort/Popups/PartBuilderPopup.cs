using Godot;
using Nort.Popups;
using Nort.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nort.Popups;

public partial class PartBuilderPopup : GenericPopup
{
	public event Action<PartData> PartBuilt;

	private DisplayPart displayPart;
	private OptionButton partOptions;
	private SkillOptions skillOptions;

	private readonly PartData partData = new();
	private readonly IEnumerable<Part> hulls = Assets.Instance.GetHullParts();

	public override void _Ready()
	{
		base._Ready();

		displayPart = GetNode<DisplayPart>("%DisplayPart");
		partOptions = GetNode<OptionButton>("%PartOptions");
		skillOptions = GetNode<SkillOptions>("%SkillOptions");

		Cancellable = true;

		partOptions.Clear();

		foreach (Part part in hulls)
		{
			partOptions.AddIconItem(
				Assets.Instance.GetPartTexture(part.id),
				part.displayName
			);
		}

		partOptions.Selected = 0;
		partData.Part = hulls.First();
		displayPart.PartData = partData;

		skillOptions.SkillSelected += OnSkillOptionsSkillSelected;
	}

	private void OnSkillOptionsSkillSelected(Skill skill)
	{
		partData.skillId = skill.id;
		displayPart.Skill = skill;
	}

	private void OnBuildButtonPressed()
	{
		PartBuilt?.Invoke(partData);
		Remove();
	}

	private void OnShinyCheckBoxToggled(bool buttonPressed)
	{
		partData.shiny = buttonPressed;
		displayPart.Shiny = buttonPressed;
	}

	private void OnPartOptionsItemSelected(int index)
	{
		Part part = hulls.ElementAt(index);
		partData.Part = part;
		displayPart.Part = part;
	}
}
