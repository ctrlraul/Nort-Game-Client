using System;
using System.Threading.Tasks;
using CtrlRaul;
using Godot;

namespace Nort;

public partial class Transition : Node
{
    public bool Covered { get; private set; }

    public Task Cover()
    {
        throw new NotImplementedException();
    }

    public Task Uncover()
    {
        throw new NotImplementedException();
    }
}

public class TransitionsManager : Singleton<TransitionsManager>
{
    public static Transition Current { get; private set; }
    public static bool Covered => Current.Covered;

    public async Task Cover()
    {
        await Current.Cover();
    }

    public async Task Uncover()
    {
        if (Covered)
        {
            await Current.Uncover();
        }
    }
}