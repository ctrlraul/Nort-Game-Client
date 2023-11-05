using System;
using System.Collections.Generic;
using Godot;

namespace CtrlRaul.Godot.Linq;

public static partial class Extensions
{
    public static IEnumerable<string> Files(this DirAccess dir)
    {
        dir.ListDirBegin();
        
        string fileName = dir.GetNext();

        while (!string.IsNullOrEmpty(fileName))
        {
            if (!dir.CurrentIsDir())
                yield return fileName;

            fileName = dir.GetNext();
        }
    }


}