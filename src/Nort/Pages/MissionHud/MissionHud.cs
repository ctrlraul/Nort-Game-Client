using System.Linq;
using Godot;
using System.Threading.Tasks;
using CtrlRaul.Godot;
using CtrlRaul.Godot.Linq;
using Nort.UI;
using Nort.UI.Overlays;
using Nort.Entities;
using Nort.Skills;

namespace Nort.Pages;

public partial class MissionHud : Page
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
    [Export] public PackedScene missionCompleteOverlayScene;
    [Export] public PackedScene skillButtonScene;

    [Ready] public Control uiRoot;
    [Ready] public SimpleProgressBar hullProgressBar;
    [Ready] public SimpleProgressBar coreProgressBar;
    [Ready] public Control skillButtons;
    [Ready] public Control interactionIndicator;

    private Mission mission;
    private PlayerCraft player;

    public bool FromEditor { get; private set; }
    
    
    public override void _Ready()
    {
        base._Ready();
        this.InitializeReady();

        interactionIndicator.Visible = false;
        skillButtons.QueueFreeChildren();
        
        Stage.Instance.PlayerSpawned += OnPlayerSpawned;
        Stage.Instance.PlayerDestroyed += OnPlayerDestroyed;
        Stage.Instance.MissionCompleted += OnMissionCompleted;
        
        SetProcess(false);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Stage.Instance.PlayerSpawned -= OnPlayerSpawned;
        Stage.Instance.PlayerDestroyed -= OnPlayerDestroyed;
        Stage.Instance.MissionCompleted -= OnMissionCompleted;
    }

    public override void _Process(double delta)
    {
        hullProgressBar.Progress = player.Hull / player.HullMax;
        coreProgressBar.Progress = player.Core / player.CoreMax;

        if (player.InteractableFocused != null)
        {
            interactionIndicator.Visible = true;
            interactionIndicator.Position = uiRoot.Size * 0.5f -
                                            (Stage.Instance.camera.Position -
                                             player.InteractableFocused.GlobalPosition) * Stage.Instance.camera.Zoom +
                                            Vector2.Down * player.InteractableFocused.Radius;
        }
        else
        {
            interactionIndicator.Visible = false;
        }
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

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey keyEvent)
            return;
        
        
        
        if (Input.IsActionJustPressed("escape") && Visible)
        {
            Visible = false;
            PauseOverlay pause = pauseOverlayScene.Instantiate<PauseOverlay>();
            AddChild(pause);
            pause.Unpaused += OnUnpaused;
            pause.Forfeit += Forfeit;
            pause.Quit += () => GetTree().Quit();

            return;
        }

        switch (keyEvent.Keycode)
        {
            case Key.Key1:
                TryToUseSkill(0);

                break;

            case Key.Key2:
                TryToUseSkill(1);

                break;

            case Key.Key3:
                TryToUseSkill(2);

                break;

            case Key.Key4:
                TryToUseSkill(3);

                break;
        }
    }

    private void TryToUseSkill(int index)
    {
        if (index >= skillButtons.GetChildCount())
            return;

        SkillButton skillButton = (SkillButton)skillButtons.GetChild(index);

        skillButton.Fire();
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

    private void OnPlayerSpawned(PlayerCraft craft)
    {
        player = craft;
        
        hullProgressBar.Modulate = player.Faction.Color;
        hullProgressBar.Progress = 1;
        coreProgressBar.Progress = 1;

        int activeSkills = 0;

        foreach (ISkillNode skillNode in player.skillNodes.OrderBy(skillNode => skillNode.Passive))
        {
            SkillButton button = skillButtonScene.Instantiate<SkillButton>();
            skillButtons.AddChild(button);
            button.SetSkillNode(skillNode);

            if (skillNode.Passive)
            {
                button.SetShortcutLabel(string.Empty);
            }
            else
            {
                activeSkills++;
                button.SetShortcutLabel(activeSkills.ToString());
            }
        }
        
        SetProcess(true);
    }

    private void OnPlayerDestroyed()
    {
        hullProgressBar.Progress = 0;
        coreProgressBar.Progress = 0;
        SetProcess(false);
    }

    private void OnMissionCompleted(MissionCompletion missionCompletion)
    {
        MissionCompleteOverlay overlay = missionCompleteOverlayScene.Instantiate<MissionCompleteOverlay>();
        AddChild(overlay);
        overlay.SetMissionCompletion(missionCompletion);
        Visible = false;
    }

    private void OnUnpaused()
    {
        Visible = true;
    }
}
