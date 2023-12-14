using System;
using System.Threading.Tasks;
using CtrlRaul.Godot;
using Godot;
using Nort.UI;

namespace Nort.Pages;

public partial class MainMenuPage : Page
{
    [Ready] public DisplayCraft displayCraft;
    [Ready] public Button missionEditorButton;

    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();
        
        if (!OS.HasFeature("editor"))
            missionEditorButton.QueueFree();

        Game.Instance.PlayerChanged += UpdatePlayerCraftDisplay;
        TreeExiting += () => Game.Instance.PlayerChanged -= UpdatePlayerCraftDisplay;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Game.Instance.PlayerChanged -= UpdatePlayerCraftDisplay;
    }

    public override async Task Initialize()
    {
        await Game.Instance.Initialize();
        UpdatePlayerCraftDisplay(Game.Instance.CurrentPlayer);
        DoCoolZoom();
    }

    private void UpdatePlayerCraftDisplay(Player player)
    {
        if (player != null)
        {
            displayCraft.Visible = true;
            displayCraft.Blueprint = Game.Instance.CurrentPlayer.blueprint;
            displayCraft.Color = Assets.Instance.PlayerFaction.Color;
        }
        else
        {
            displayCraft.Visible = false;
        }
    }

    private static async void OnStartButtonPressed()
    {
        if (LocalPlayersManager.HasLocalPlayers())
        {
            await PagesNavigator.Instance.GoTo(Config.Pages.LocalPlayers);
        }
        else
        {
            Game.Instance.CurrentPlayer = LocalPlayersManager.NewLocalPlayer();
            LocalPlayersManager.Instance.StoreLocalPlayer(Game.Instance.CurrentPlayer);
            await PagesNavigator.Instance.GoTo(Config.Pages.Lobby);
        }
    }

    private static async void OnMissionEditorButtonPressed()
    {
        await PagesNavigator.Instance.GoTo(Config.Pages.MissionEditor);
    }

    private static void DoCoolZoom()
    {
        Stage.Instance.camera.Zoom = new Vector2(0.25f, 0.25f);

        Tween tween = Stage.Instance.CreateTween();

        tween.SetEase(Tween.EaseType.Out);
        tween.SetTrans(Tween.TransitionType.Expo);
        tween.TweenProperty(Stage.Instance.camera, "zoom", new Vector2(0.5f, 0.5f), 10);
    }
}