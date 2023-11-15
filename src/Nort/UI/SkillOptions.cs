using Godot;
using System;
using System.Collections.Generic;

namespace Nort.UI;
public partial class SkillOptions : OptionButton
{
	public event Action<Skill> SkillSelected;

	private readonly List<Skill> skills = new(){ null };

	public override void _Ready()
	{
		Clear();
		AddItem("Passive");

		Game.Instance.Initialize();

		AddSkillOptions();
	}

	private void AddSkillOptions()
	{
		foreach (Skill skill in Assets.Instance.GetSkills())
		{
			skills.Add(skill);
			AddIconItem(Assets.Instance.GetSkillTexture(skill.id), " " + skill.displayName);
		}
	}

	public void SetSkill(Skill skill)
	{
		Selected = skills.IndexOf(skill);
	}

	public void OnItemSelected(int index)
	{
		SkillSelected?.Invoke(skills[index]);
	}
}