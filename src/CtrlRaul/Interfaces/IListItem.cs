using System;

namespace CtrlRaul.Interfaces;

public interface IListItem<T>
{
    public T Value { get; }
    public void SetFor(T value);
}
