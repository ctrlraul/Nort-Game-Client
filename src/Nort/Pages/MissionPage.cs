using Godot;
using System.Threading.Tasks;
using CtrlRaul.Godot;
using Nort.UI;
using Nort.UI.Overlays;
using Craft = Nort.Entities.Craft;

namespace Nort.Pages;

public partial class MissionPage : Page
{
    public class NavigationData
    {
        public bool FromEditor { get; }
        public Mission Mission { get; }

        public NavigationData(bool fromEditor, Mission mission)
        {
            FromEditor = fromEditor;
            Mission = mission;
        }
    }

    [Export] public PackedScene pauseOverlayScene;

    [Ready] public SimpleProgressBar hullProgressBar;
    [Ready] public SimpleProgressBar coreProgressBar;

    private Mission mission;
    private Craft player;

    public bool FromEditor { get; private set; }
    
    
    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();
        Stage.Instance.PlayerSpawned += OnPlayerSpawned;
        Stage.Instance.PlayerDestroyed += OnPlayerDestroyed;
        SetProcess(false);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Stage.Instance.PlayerSpawned -= OnPlayerSpawned;
        Stage.Instance.PlayerDestroyed -= OnPlayerDestroyed;
    }

    public override void _Process(double delta)
    {
        hullProgressBar.Progress = player.Hull / player.HullMax;
        coreProgressBar.Progress = player.Core / player.CoreMax;
    }
    
    public override async Task Initialize()
    {
        await Game.Instance.Initialize();
        
        hullProgressBar.Color = Assets.Instance.PlayerFaction.Color;

        if (PagesNavigator.Instance.NavigationData is NavigationData navigationData)
        {
            FromEditor = navigationData.FromEditor;
            mission = navigationData.Mission;
            Stage.Instance.LoadMission(mission);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionJustPressed("escape") && Visible)
        {
            Visible = false;
            PauseOverlay pause = pauseOverlayScene.Instantiate<PauseOverlay>();
            AddChild(pause);
            pause.Unpaused += OnUnpaused;
            pause.Forfeit += Forfeit;
            pause.Quit += () => GetTree().Quit();
        }
    }

    public async void Forfeit()
    {
        if (FromEditor)
        {
            await PagesNavigator.Instance.GoBack(new Editor.NavigationData(mission));
        }
        else
        {
            await PagesNavigator.Instance.GoBack();
        }
    }

    private void OnPlayerSpawned(Craft craft)
    {
        player = craft;
        hullProgressBar.Modulate = player.Faction.Color;
        hullProgressBar.Progress = 1;
        coreProgressBar.Progress = 1;
        SetProcess(true);
    }

    private void OnPlayerDestroyed()
    {
        hullProgressBar.Progress = 0;
        coreProgressBar.Progress = 0;
        SetProcess(false);
    }

    private void OnUnpaused()
    {
        Visible = true;
    }
}
