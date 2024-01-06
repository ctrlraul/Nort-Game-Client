using System.Threading.Tasks;
using CtrlRaul.Godot;
using CtrlRaul.Godot.Linq;
using Godot;
using Nort.UI;

namespace Nort.Pages;

public partial class LobbyPage : Page
{
    [Export] private PackedScene playableMissionButtonScene;

    [Ready] public Control missionsContainer;
    [Ready] public DisplayCraft displayCraft;

    
    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();
        missionsContainer.QueueFreeChildren();
    }


    public override async Task Initialize()
    {
        await Game.Instance.Initialize();

        foreach (Mission mission in Assets.Instance.GetMissions())
        {
            PlayableMissionButton listItem = playableMissionButtonScene.Instantiate<PlayableMissionButton>();
            missionsContainer.AddChild(listItem);
            listItem.SetMission(mission);
        }

        displayCraft.Blueprint = Game.Instance.CurrentPlayer.blueprint;
        displayCraft.Faction = Assets.Instance.PlayerFaction;
    }


    private async void OnEditCraftButtonPressed()
    {
        await PagesNavigator.Instance.GoTo(Config.Pages.CraftBuilder);
    }

    private async void OnReturnButtonPressed()
    {
        Game.Instance.CurrentPlayer = null;
        await PagesNavigator.Instance.GoBack();
    }
}