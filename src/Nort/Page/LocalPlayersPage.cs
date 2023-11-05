using System.Linq;
using System.Threading.Tasks;
using CtrlRaul.Godot.Linq;
using Godot;
using Nort.Listing;

namespace Nort.Page;

public partial class LocalPlayersPage : PageScene
{
    [Export] private PackedScene playerListItemScene;
    
    private Control playersList;

    public override void _Ready()
    {
        playersList = GetNode<Control>("%PlayersList");
        Stage.Instance.Clear();
        LocalPlayersManager.Instance.LocalPlayerDeleted += OnLocalPlayerDeleted;
    }

    public override async Task PostReady()
    {
        await Game.Instance.Initialize();
        RefreshList();
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
        PagesNavigator.Instance.GoTo(GameConfig.Pages.Lobby);
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