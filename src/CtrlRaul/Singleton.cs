namespace CtrlRaul;

public class Singleton<T> where T : class, new()
{
	// ReSharper disable once InconsistentNaming
	public static readonly T Instance;

	static Singleton()
	{
		Instance = new T();
	}

	protected readonly Logger logger = new(typeof(T).Name);
}
