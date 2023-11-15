using System;
using Godot;

namespace CtrlRaul;

public class Logger
{
    public static ILogger externalLogger;
    private static string CurrentTime => (Time.GetTicksMsec() / 1000f).ToString("0.000");
    
    private const string Format = "{0} {1} [{2}] {3}";

    private readonly string label;

    public Logger(string label)
    {
        this.label = label;
    }

    public void Log(object message)
    {
        string text = string.Format(Format, CurrentTime, "i", label, message);
        GD.Print(text);
#if DEBUG
        externalLogger?.Log(text);
#endif
    }

    public void Warn(object message)
    {
        string text = string.Format(Format, Time.GetTicksMsec() / 1000f, "W", label, message);
        GD.PushWarning(text);
#if DEBUG
        externalLogger?.Warn(text);
#endif
    }

    public void Error(object message)
    {
        string text = string.Format(Format, Time.GetTicksMsec() / 1000f, "E", label, message);
        GD.PrintErr(text);
#if DEBUG
        externalLogger?.Error(text);
#endif
    }

    public void Error(Exception exception)
    {
        string text = string.Format(Format, Time.GetTicksMsec() / 1000f, "E", label, exception);
        GD.PrintErr(text);
#if DEBUG
        externalLogger?.Error(text);
        
        // Creates a error debuggable in the engine.
        // Even with this we still want the basic PrintErr as the debuggable error's
        // stack trace can point to the wrong location when using async-away.
        GD.PushError(string.Format(Format, Time.GetTicksMsec() / 1000f, "E", label, exception.Message));
#endif
    }

    public static void Log(string label, object message)
    {
        string text = string.Format(Format, CurrentTime, "i", label, message);
        GD.Print(text);
#if DEBUG
        externalLogger?.Log(text);
#endif
    }
}