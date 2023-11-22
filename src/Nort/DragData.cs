using Godot;

namespace Nort;

public partial class DragData : RefCounted
{
    public readonly Control source;
    public readonly object data;
    
    public DragData(Control source, object data)
    {
        this.source = source;
        this.data = data;
    }
}