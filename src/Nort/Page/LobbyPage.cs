using System.Threading.Tasks;

namespace Nort.Page;

public partial class LobbyPage : PageScene
{
    public override async Task PostReady()
    {
        await Game.Instance.Initialize();
        Stage.Instance.SpawnPlayerCraft();
    }

    private void OnCraftBuilderButtonPressed()
    {
        PagesNavigator.Instance.GoTo(GameConfig.Pages.CraftBuilder);
    }

    private void OnTestButtonPressed()
    {
        // MissionPageData data = new(){ mission = Assets.Instance.GetMission("basics") };
        // PagesNavigator.Instance.GoTo(GameConfig.Pages.Mission, data);
    }
}