using System;
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

    public bool Dev => OS.HasFeature("editor");

    public Game()
    {
        Instance = this;
        logger = new(GetType().Name);
    }

    public override void _Ready()
    {
        base._Ready();
        PagesNavigator.Instance.SetDefaultScene(GameConfig.Pages.MainMenu);
        PagesNavigator.Instance.AddMiddleware(PageChangeMiddleware);
    }

    public Task Initialize()
    {
        return initializationTask ??= InitializeSub();
    }

    private async Task InitializeSub()
    {
        await Assets.Instance.ImportAssets(GameConfig.ConfigPath);
        Initialized?.Invoke();
    }

    private async Task PageChangeMiddleware(Node node)
    {
        if (node is not Page page)
        {
            if (!PagesNavigator.Instance.IsFirstScene(node)) // Likely not testing something
            {
                logger.Warn($"'{node.GetType().Name}' does not extend {nameof(Page)}");
            }
            return;
        }

        Task coveringTask = TransitionsManager.Instance.Cover();

        try
        {
            await page.Initialize();
        }
        catch (Exception exception)
        {
            logger.Error(exception);

            // if (PagesNavigator.Instance.IsFirstScene(page))
            // {
            //     
            // }

            PopupsManager.Instance.Error(exception.Message, $"Failed to initialize {page.Name} page!");
            PagesNavigator.Instance.Cancel();
        }

        if (!coveringTask.IsCompleted)
            await coveringTask;

        _ = TransitionsManager.Instance.Uncover();
    }
}