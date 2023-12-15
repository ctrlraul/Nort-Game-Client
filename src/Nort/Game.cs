using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using CtrlRaul;
using Godot;
using Nort.Pages;

namespace Nort;

public partial class Game : Node
{
    public static Game Instance { get; private set; }
    private readonly Logger logger;
    private event Action Initialized;
    public event Action<Player> PlayerChanged;

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

    public bool IsInitialized => initializationTask?.IsCompleted ?? false;
    public bool IsInitializing => initializationTask is { IsCompleted: false };

    public bool Dev => CurrentPlayer == null;

    public bool InMissionEditor { get; private set; }
    
    
    public Game()
    {
        Instance = this;
        logger = new(GetType().Name);
    }

    
    public override void _Ready()
    {
        base._Ready();
        PagesNavigator.Instance.SetDefaultScene(Config.Pages.MainMenu);
        PagesNavigator.Instance.AddMiddleware(ClearStageMiddleware);
        PagesNavigator.Instance.AddMiddleware(InitializePageMiddleware);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (Input.IsActionJustPressed("toggle_fullscreen"))
        {
            ToggleFullscreen();
            GetViewport().SetInputAsHandled();
        }
    }

    
    public Task Initialize()
    {
        return initializationTask ??= InitializeSub();
    }

    private async Task InitializeSub()
    {
        await Assets.Instance.ImportAssets();
        Initialized?.Invoke();
    }
    
    
    private static Task ClearStageMiddleware(Node node)
    {
        Stage.Instance.Clear();
        Stage.Instance.SetGrid(0);
        return Task.CompletedTask;
    }

    private async Task InitializePageMiddleware(Node node)
    {
        if (node is not Page page)
        {
            InMissionEditor = false;
            
            if (!PagesNavigator.Instance.IsFirstScene(node)) // Likely not testing something
                logger.Warn($"'{node.GetType().Name}' does not extend {nameof(Page)}");
            
            return;
        }
        
        InMissionEditor = page is Editor;

        Task coveringTask = TransitionsManager.Instance.Cover();

        try
        {
            await page.Initialize();
        }
        catch (Exception shittyException)
        {
            Exception exception = ExceptionDispatchInfo.Capture(shittyException).SourceException;
            
            logger.Error(exception);

            // if (PagesNavigator.Instance.IsFirstScene(page))
            // {
            //     // Show something special about the game itself failing to launch and dev contact
            // }

            PopupsManager.Instance.Error(exception.Message, $"Failed to initialize {page.Name} page!");
            PagesNavigator.Instance.Cancel();
        }

        if (!coveringTask.IsCompleted)
            await coveringTask;

        _ = TransitionsManager.Instance.Uncover();
    }

    
    private static void ToggleFullscreen()
    {
        DisplayServer.WindowSetMode(
            DisplayServer.WindowGetMode() == DisplayServer.WindowMode.ExclusiveFullscreen
                ? DisplayServer.WindowMode.Windowed
                : DisplayServer.WindowMode.ExclusiveFullscreen
        );
    }
}