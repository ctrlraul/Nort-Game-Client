namespace CtrlRaul;

public interface ILogger
{
    public void Log(object message);
    public void Warn(object message);
    public void Error(object message);
}