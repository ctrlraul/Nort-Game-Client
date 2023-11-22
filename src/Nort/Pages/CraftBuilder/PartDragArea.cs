using System;
using Godot;

namespace Nort.Pages.CraftBuilder;
public partial class PartDragArea : Button
{
	public event Action<DragData> GotData;
	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		return data.As<DragData>()?.data is PartData;
	}

	public override void _DropData(Vector2 atPosition, Variant data)
	{
		base._DropData(atPosition, data);
		DragData dragData = data.As<DragData>();
		GotData?.Invoke(dragData);
	}
}
