using System;
using System.Threading.Tasks;
using CtrlRaul;
using Godot;
using Nort.Page;

namespace Nort;

public class Game : Singleton<Game>
{
    private event Action<Player> PlayerChanged;

    private Task initializationTask = null;

    private Player currentPlayer;
    public Player CurrentPlayer
    {
        get => currentPlayer;
        set
        {
            currentPlayer = value;
            PlayerChanged?.Invoke(value);
        }
    }
    
    // public bool Initialized => initializationTask?.IsCompleted ?? false;
    // public bool Initializing => initializationTask is { IsCompleted: false };

    public Game()
    { 
        PagesNavigator.Instance.SetDefaultScene(GameConfig.Pages.MainMenu);
        PagesNavigator.Instance.PageDataLoading += OnPageDataLoading;
        PagesNavigator.Instance.PageDataLoaded += OnPageDataLoaded;
        PagesNavigator.Instance.PageDataLoadError += OnPageDataLoadError;
        PagesNavigator.Instance.PageChangeError += OnPageChangeError;
    }

    public Task Initialize()
    {
        return initializationTask ??= InitializeSub();
    }

    private async Task InitializeSub()
    {
        await Assets.Instance.ImportAssets(GameConfig.ConfigPath);
    }

    private void OnPageDataLoading(PageScene page)
    {
        // if (!Transitions.Instance.Covered)
        // {
        //     Transitions.Instance.Cover(Transitions.Instance.pixelated);
        // }
    }

    private void OnPageDataLoaded(PageScene page)
    {
        //Transitions.Instance.Uncover();
    }

    private void OnPageDataLoadError(PageScene page, string message)
    {
        // if (Transitions.Instance.Covered)
        // {
        //     Transitions.Instance.Uncover();
        // }

        PopupsManager.Instance.Error(message);
    }

    private void OnPageChangeError(PackedScene scene, string message)
    {
        PopupsManager.Instance.Error(message);
    }
}