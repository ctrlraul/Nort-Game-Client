using Godot;
using CtrlRaul.Godot;
using Nort.Entities;
using Nort.Skills;

namespace Nort.Pages;

public partial class SkillButton : TextureButton
{
	[Ready] public TextureRect textureRect;
	[Ready] public Control cooldownBar;
	[Ready] public Label shortcutLabel;
	[Ready] public AnimationPlayer animationPlayer;

	private ISkillNode skillNode;
	
	
	public override void _Ready()
	{
		this.InitializeReady();
	}


	public void Fire()
	{
		if (skillNode.Passive)
			return;

		skillNode.Fire();
	}

	public void SetSkillNode(ISkillNode value)
	{
		if (skillNode != null)
		{
			skillNode.Fired -= OnSkillNodeFired;
			skillNode.Part.Destroyed -= OnSkillNodePartDestroyed;
		}

		skillNode = value;
		cooldownBar.Visible = false;

		if (skillNode != null)
		{
			textureRect.Texture = skillNode.Texture;
            
			skillNode.Fired += OnSkillNodeFired;
			skillNode.Part.Destroyed += OnSkillNodePartDestroyed;
			
			if (skillNode.CooldownMax > 0)
			{
				cooldownBar.Visible = true;
				animationPlayer.Play("progress");
				animationPlayer.Seek(skillNode.Cooldown);
				animationPlayer.SpeedScale = 1 / skillNode.CooldownMax;
			}
		}
	}

	public void SetShortcutLabel(string text)
	{
		shortcutLabel.Text = text;
	}


	private void OnSkillNodeFired()
	{
		animationPlayer.Stop();
		animationPlayer.Play("progress");
	}

	private void OnSkillNodePartDestroyed(CraftPart part)
	{
		skillNode = null;
		Modulate = Colors.Red * 0.5f;
		cooldownBar.Visible = false;
	}

	private void OnButtonDown()
	{
		cooldownBar.SelfModulate = Colors.White;
	}

	private void OnButtonUp()
	{
		cooldownBar.SelfModulate = Assets.Instance.PlayerFaction.Color;
	}
}
