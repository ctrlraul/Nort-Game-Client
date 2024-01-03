using System.Threading.Tasks;
using Nort.Entities;

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
        Stage.Instance.AddEntity<PlayerCraft>();
    }

    private void OnCraftBuilderButtonPressed()
    {
        _ = PagesNavigator.Instance.GoTo(Config.Pages.CraftBuilder);
    }

    private void OnTestButtonPressed()
    {
        MissionHud.NavigationData data = new(fromEditor:false, Assets.Instance.GetMission("basics"));
        _ = PagesNavigator.Instance.GoTo(Config.Pages.Mission, data);
    }
}