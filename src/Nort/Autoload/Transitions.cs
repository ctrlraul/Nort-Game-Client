using System;
using Godot;

namespace Nort;

public partial class Transition : Node
{
    public bool Covered { get; private set; }

    public void Uncover()
    {
        throw new NotImplementedException();
    }
}

public partial class Transitions : Node
{
    public static Transitions Instance { get; private set; }
    public static Transition Current { get; private set; }
    public static bool Covered => Current.Covered;

    public Transitions()
    {
        if (Instance == null)
        {
            throw new Exception($"{GetType().Name} is a Singleton, use {GetType().Name}.Instance instead");
        }

        Instance = this;
    }

    public void Uncover()
    {
        if (Covered)
        {
            Current.Uncover();
        }
    }
}