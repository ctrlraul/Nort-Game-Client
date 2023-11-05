using Godot;
using System.Threading.Tasks;
using CtrlRaul;

namespace Nort.Page;

public partial class PageScene : Node
{
    public object navigatorData;

    protected readonly Logger logger;

    /// <summary>
    /// Called by the PagesNavigator after _Ready
    /// </summary>
    public virtual Task PostReady()
    {
        return Task.CompletedTask;
    }

    protected PageScene()
    {
        logger = new(GetType().Name);
    }
}