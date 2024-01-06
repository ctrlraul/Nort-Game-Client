using System.Linq;
using System.Threading.Tasks;
using CtrlRaul.Godot;
using CtrlRaul.Godot.Linq;
using Godot;
using Nort.Listing;

namespace Nort.Pages;

public partial class LocalPlayersPage : Page
{
    [Export] private PackedScene playerListItemScene;

    [Ready] public Control playersList;
    [Ready] public Control refreshButton;

    private Tween refreshButtonTween;
    

    public override async Task Initialize()
    {
        await Game.Instance.Initialize();
        RefreshList();
    }

    public override void _Ready()
    {
        this.InitializeReady();
        LocalPlayersManager.Instance.LocalPlayerDeleted += OnLocalPlayerDeleted;
    }

    private void RefreshList()
    {
        playersList.QueueFreeChildren();

        foreach (Player player in LocalPlayersManager.Instance.GetPlayers())
        {
            PlayerListItem listItem = playerListItemScene.Instantiate<PlayerListItem>();
            playersList.AddChild(listItem);
            listItem.Player = player;
            listItem.Selected += () => OnLocalPlayerSelected(player);
        }

        refreshButtonTween?.Stop();
        refreshButtonTween = CreateTween();
        refreshButtonTween.TweenProperty(refreshButton, "rotation", refreshButton.Rotation + Mathf.Pi * 2, 0.3)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Quad);
    }

    private void OnLocalPlayerSelected(Player player)
    {
        Game.Instance.CurrentPlayer = player;
        _ = PagesNavigator.Instance.GoTo(Config.Pages.Lobby);
    }

    private void OnLocalPlayerDeleted(string playerId)
    {
        foreach (PlayerListItem listItem in playersList.GetChildren().Cast<PlayerListItem>())
        {
            if (listItem.Player.id != playerId)
                continue;
            
            listItem.QueueFree();
            break;
        }
    }

    private async void OnNewSaveButtonPressed()
    {
        Game.Instance.CurrentPlayer = LocalPlayersManager.NewLocalPlayer();
        LocalPlayersManager.Instance.StoreLocalPlayer(Game.Instance.CurrentPlayer);
        await PagesNavigator.Instance.GoTo(Config.Pages.Lobby);
    }
}