using System.Globalization;
using Godot;
using CtrlRaul.Godot;
using Nort.Listing;

namespace Nort.UI.Overlays;

public partial class MissionCompleteOverlay : CanvasLayer
{
	[Ready] public PartsList partsCollectedList;
	[Ready] public Label scoreLabel;
	
	public override void _Ready()
	{
		this.InitializeReady();
		partsCollectedList.Clear();
	}

	public void SetMissionCompletion(MissionCompletion missionCompletion)
	{
		foreach (PartData partData in missionCompletion.PartsCollected)
		{
			PartsListItem listItem = partsCollectedList.AddItem(partData);
			listItem.Color = Assets.Instance.PlayerFaction.Color;
		}

		scoreLabel.Text = missionCompletion.Score.ToString(CultureInfo.CurrentCulture);
	}


	private async void OnContinueButtonPressed()
	{
		await PagesNavigator.Instance.GoBack();
	}
}
