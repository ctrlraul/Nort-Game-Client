using System;
using Godot;
using Godot.Collections;

namespace CtrlRaul.Godot;

public class PhysicsLayer
{
    private static Dictionary<string, uint> physicsLayers = new();
    
    static PhysicsLayer()
    {
        for (int i = 0; i < 32; i++)
        {
            string layerName = ProjectSettings.GetSetting($"layer_names/2d_physics/layer_{i + 1}").As<string>();

            if (string.IsNullOrEmpty(layerName))
                continue;
            
            physicsLayers.Add(layerName, (uint)(1 << i));
        }
    }

    public static uint Get(string layerName)
    {
        if (physicsLayers.TryGetValue(layerName, out uint layer))
            return layer;

        throw new Exception($"No physics layer found with name '{layerName}'");
    }
}