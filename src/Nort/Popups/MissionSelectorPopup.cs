using System;
using CtrlRaul.Godot;
using CtrlRaul.Godot.Linq;
using Godot;
using Nort.Listing;

namespace Nort.Popups;

public partial class MissionSelectorPopup : GenericPopup
{
    public event Action<Mission> MissionSelected;

    [Export] private PackedScene missionsListItemScene;

    [Ready] public Control missionsList;

    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();
        missionsList.QueueFreeChildren();
        Cancellable = true;
        Initialize();
    }

    private async void Initialize()
    {
        await Game.Instance.Initialize();

        foreach (Mission mission in Assets.Instance.GetMissions())
        {
            MissionsListItem listItem = missionsListItemScene.Instantiate<MissionsListItem>();
            missionsList.AddChild(listItem);
            listItem.Mission = mission;
            listItem.Selected += () =>
            {
                MissionSelected?.Invoke(mission);
                Remove();
            };
        }
    }
}