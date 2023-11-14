using System.Threading.Tasks;

namespace Nort.Pages;

public partial class LobbyPage : Page
{
    public override async Task Initialize()
    {
        await Game.Instance.Initialize();
    }

    public override void _Ready()
    {
        base._Ready();
        Stage.Instance.SpawnPlayerCraft();
    }

    private void OnCraftBuilderButtonPressed()
    {
        _ = PagesNavigator.Instance.GoTo(GameConfig.Pages.CraftBuilder);
    }

    private void OnTestButtonPressed()
    {
        // TODO
        // MissionPageData data = new(){ mission = Assets.Instance.GetMission("basics") };
        // PagesNavigator.Instance.GoTo(GameConfig.Pages.Mission, data);
    }
}