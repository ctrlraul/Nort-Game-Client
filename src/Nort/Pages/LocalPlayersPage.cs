using System.Linq;
using System.Threading.Tasks;
using CtrlRaul.Godot.Linq;
using Godot;
using Nort.Listing;

namespace Nort.Pages;

public partial class LocalPlayersPage : Page
{
    [Export] private PackedScene playerListItemScene;

    private Control playersList;

    public override async Task Initialize()
    {
        await Game.Instance.Initialize();
        RefreshList();
    }

    public override void _Ready()
    {
        playersList = GetNode<Control>("%PlayersList");
        Stage.Instance.Clear();
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
    }

    private void OnLocalPlayerSelected(Player player)
    {
        Game.Instance.CurrentPlayer = player;
        _ = PagesNavigator.Instance.GoTo(GameConfig.Pages.Lobby);
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
}