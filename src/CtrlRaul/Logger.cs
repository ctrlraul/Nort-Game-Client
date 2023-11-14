using System;
using System.Diagnostics;
using System.Linq;
using Godot;

namespace CtrlRaul;

public class Logger
{
    private const string Format = "{0} {1} [{2}] {3}";

    private readonly string label;

    public Logger(string label)
    {
        this.label = label;
    }

    public void Log(object message)
    {
        Info(message);
    }

    public void Info(object message)
    {
        GD.Print(string.Format(Format, Time.GetTicksMsec() / 1000f, "i", label, message));
    }

    public void Error(object message)
    {
        GD.PrintErr(string.Format(Format, Time.GetTicksMsec() / 1000f, "E", label, message));
    }

    public void Error(Exception exception)
    {
#if DEBUG
        GD.PushError(string.Format(Format, Time.GetTicksMsec() / 1000f, "E", label, exception.Message));
#else
        GD.PrintErr(string.Format(FORMAT, Time.GetTicksMsec() / 1000f, "E", label, exception));
#endif
    }

    public void Warn(object message)
    {
        GD.PushWarning(string.Format(Format, Time.GetTicksMsec() / 1000f, "W", label, message));
    }

    // public static void Error(string label, object message)
    // {
    //     GD.PrintErr(string.Format(FORMAT, Time.GetTicksMsec() / 1000f, "E", label, message));
    // }
}