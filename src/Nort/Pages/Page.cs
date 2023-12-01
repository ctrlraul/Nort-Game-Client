using System;
using Godot;
using System.Threading.Tasks;
using CtrlRaul;

namespace Nort.Pages;

public partial class Page : CanvasLayer
{
    protected readonly Logger logger;

    public virtual Task Initialize()
    {
        return Task.CompletedTask;
    }

    protected Page()
    {
        logger = new(GetType().Name);
    }
}