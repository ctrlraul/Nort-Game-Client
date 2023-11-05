using System;
using System.Threading.Tasks;
using Godot;
using Nort.UI;

namespace Nort.Page;

public partial class MainMenuPage : PageScene
{
    private DisplayCraft displayCraft;

    public override void _Ready()
    {
        displayCraft = GetNode<DisplayCraft>("%DisplayCraft");
        displayCraft.Visible = Game.Instance.CurrentPlayer != null;

        Stage.Instance.Clear();
        Stage.Instance.camera.Set("zoom", new Vector2(0.25f, 0.25f));

        Tween tween = Stage.Instance.CreateTween();

        tween.SetEase(Tween.EaseType.Out);
        tween.SetTrans(Tween.TransitionType.Expo);
        tween.TweenProperty(Stage.Instance.camera, "zoom", new Vector2(0.5f, 0.5f), 10);
        
        if (!OS.HasFeature("editor"))
            GetNode("%MissionEditorButton").QueueFree();
    }

    public override async Task PostReady()
    {
        await Game.Instance.Initialize();

        if (Game.Instance.CurrentPlayer != null)
        {
            displayCraft.Blueprint = Game.Instance.CurrentPlayer.CurrentBlueprint;
            displayCraft.Color = Assets.Instance.PlayerFaction.Color;
        }
        else
        {
            displayCraft.QueueFree();
        }
    }

    private void OnStartButtonPressed()
    {
        if (LocalPlayersManager.HasLocalPlayers())
        {
            PagesNavigator.Instance.GoTo(GameConfig.Pages.LocalPlayers);
        }
        else
        {
            Game.Instance.CurrentPlayer = LocalPlayersManager.Instance.NewLocalPlayer();
            LocalPlayersManager.Instance.StoreLocalPlayer(Game.Instance.CurrentPlayer);
            PagesNavigator.Instance.GoTo(GameConfig.Pages.Lobby);
        }
    }

    private void OnMissionEditorButtonPressed()
    {
        PagesNavigator.Instance.GoTo(GameConfig.Pages.MissionEditor);
    }
}