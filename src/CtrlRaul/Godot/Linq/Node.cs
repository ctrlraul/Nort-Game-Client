using System;
using Godot;

namespace CtrlRaul.Godot.Linq;

public static partial class Extensions
{
	public static void FreeChildren(this Node node)
	{
		foreach(Node child in node.GetChildren())
		{
			node.RemoveChild(child);
			child.Free();
		}
	}
	
	public static void QueueFreeChildren(this Node node)
	{
		foreach(Node child in node.GetChildren())
		{
			node.RemoveChild(child);
			child.QueueFree();
		}
	}
	
	public static void Remove(this Node node)
	{
		Node parent = node.GetParent();
		if (parent == null)
			throw new Exception("Can't remove orphan node");
		parent.RemoveChild(node);
	}
}
