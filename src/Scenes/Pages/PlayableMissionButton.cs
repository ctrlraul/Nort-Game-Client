using Godot;
using System;
using System.Globalization;
using CtrlRaul.Godot;
using Nort;
using Nort.Entities;
using Nort.Pages;

public partial class PlayableMissionButton : PanelContainer
{
	[Ready] public Label nameLabel;
	[Ready] public Label entitiesLabel;
	[Ready] public Label quoteLabel;
	[Ready] public Label timeLabel;
	[Ready] public Label scoreLabel;

	private Mission mission;


	public override void _Ready()
	{
		this.InitializeReady();
	}


	public void SetMission(Mission value)
	{
		mission = value;

		int enemies = 0;
		int allies = 0;

		foreach (EntitySetup setup in mission.entitySetups)
		{
			if (setup.subTypeData.TryGetValue("FactionId", out object factionId))
			{
				if (Assets.Instance.PlayerFaction.id == (string)factionId)
					allies += 1;
				else
					enemies += 1;
			}
		}

		nameLabel.Text = mission.displayName;
		entitiesLabel.Text = $"{enemies} Enemies\n{allies} Allies";
		quoteLabel.Text = mission.description;

		if (Game.Instance.CurrentPlayer.missionRecords.TryGetValue(mission.id, out MissionRecord record))
		{
			timeLabel.Text = $"{Math.Round(record.bestTime, 2)}s";
			scoreLabel.Text = record.bestScore.ToString(CultureInfo.CurrentCulture);
		}
		else
		{
			timeLabel.Text = "N/A";
			scoreLabel.Text = "N/A";
		}
	}


	private async void OnButtonPressed()
	{
		MissionHud.NavigationData data = new(
			fromEditor: false,
			Assets.Instance.GetMission(mission.id)
		);

		await PagesNavigator.Instance.GoTo(Config.Pages.MissionHud, data);
	}
}