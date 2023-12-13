using System;
using System.Collections.Generic;
using Godot;
using System.Linq;

namespace CtrlRaul.Godot.Linq;

public static class Node2DExtensions
{
    public static T FindNearest<T>(this IEnumerable<T> nodes, Vector2 position, bool useGlobalPosition = false) where T : Node2D
    {
        string positionProp = useGlobalPosition ? "global_position" : "position";
        return nodes
            .Where(node => !node.IsQueuedForDeletion())
            .MinBy(node => node.Get(positionProp).AsVector2().DistanceTo(position));
    }
}