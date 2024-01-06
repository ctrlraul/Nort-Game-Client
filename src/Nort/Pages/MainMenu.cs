using System.Threading.Tasks;
using CtrlRaul.Godot;
using Godot;
using Nort.UI;

namespace Nort.Pages;

public partial class MainMenu : Page
{
    [Ready] public DisplayCraft displayCraft;
    [Ready] public Button missionEditorButton;

    private Tween zoomTween;
    

    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();
        
        // if (!OS.HasFeature("editor"))
        //     missionEditorButton.QueueFree();

        Game.Instance.PlayerChanged += UpdatePlayerCraftDisplay;
        TreeExiting += () => Game.Instance.PlayerChanged -= UpdatePlayerCraftDisplay;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Game.Instance.PlayerChanged -= UpdatePlayerCraftDisplay;
        zoomTween?.Stop();
    }

    public override async Task Initialize()
    {
        await Game.Instance.Initialize();
        UpdatePlayerCraftDisplay(Game.Instance.CurrentPlayer);
        if (PagesNavigator.Instance.IsFirstScene(this))
            DoCoolZoom();
    }

    private void UpdatePlayerCraftDisplay(Player player)
    {
        if (player != null)
        {
            displayCraft.Visible = true;
            displayCraft.Blueprint = Game.Instance.CurrentPlayer.blueprint;
            displayCraft.Faction = Assets.Instance.PlayerFaction;
        }
        else
        {
            displayCraft.Visible = false;
        }
    }

    private void DoCoolZoom()
    {
        Stage.Instance.camera.Zoom = new Vector2(0.25f, 0.25f);

        zoomTween = Stage.Instance.CreateTween();
        zoomTween.SetEase(Tween.EaseType.Out);
        zoomTween.SetTrans(Tween.TransitionType.Expo);
        zoomTween.TweenProperty(Stage.Instance.camera, "zoom", new Vector2(0.5f, 0.5f), 10);
        zoomTween.Finished += () => zoomTween = null;
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
}