using System;
using System.Threading.Tasks;
using Godot;
using Nort.UI;

namespace Nort.Pages;

public partial class MainMenuPage : Page
{
    private DisplayCraft displayCraft;

    public override async Task Initialize()
    {
        await Game.Instance.Initialize();
        UpdatePlayerCraftDisplay(Game.Instance.CurrentPlayer);
    }

    public override void _Ready()
    {
        displayCraft = GetNode<DisplayCraft>("%DisplayCraft");

        Stage.Instance.Clear();
        Stage.Instance.camera.Set("zoom", new Vector2(0.25f, 0.25f));

        Tween tween = Stage.Instance.CreateTween();

        tween.SetEase(Tween.EaseType.Out);
        tween.SetTrans(Tween.TransitionType.Expo);
        tween.TweenProperty(Stage.Instance.camera, "zoom", new Vector2(0.5f, 0.5f), 10);
        
        if (!OS.HasFeature("editor"))
            GetNode("%MissionEditorButton").QueueFree();

        Game.Instance.PlayerChanged += UpdatePlayerCraftDisplay;
        TreeExiting += () => Game.Instance.PlayerChanged -= UpdatePlayerCraftDisplay;
    }

    // public override void _ExitTree()
    // {
    //     Game.Instance.PlayerChanged -= UpdatePlayerCraftDisplay;
    // }

    private void UpdatePlayerCraftDisplay(Player player)
    {
        if (player != null)
        {
            displayCraft.Visible = true;
            displayCraft.Blueprint = Game.Instance.CurrentPlayer.CurrentBlueprint;
            displayCraft.Color = Assets.Instance.PlayerFaction.Color;
        }
        else
        {
            displayCraft.Visible = false;
        }
    }

    private void OnStartButtonPressed()
    {
        if (LocalPlayersManager.HasLocalPlayers())
        {
            _ = PagesNavigator.Instance.GoTo(GameConfig.Pages.LocalPlayers);
        }
        else
        {
            Game.Instance.CurrentPlayer = LocalPlayersManager.Instance.NewLocalPlayer();
            LocalPlayersManager.Instance.StoreLocalPlayer(Game.Instance.CurrentPlayer);
            _ = PagesNavigator.Instance.GoTo(GameConfig.Pages.Lobby);
        }
    }

    private async Task OnMissionEditorButtonPressed()
    {
        await PagesNavigator.Instance.GoTo(GameConfig.Pages.MissionEditor);
    }
}