using System;
using System.Collections.Generic;
using Godot;
using Newtonsoft.Json;
using Nort;

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

    public static IEnumerable<T> ParseJsonFiles<T>(this DirAccess dir) where T : class, new()
    {
        dir.ListDirBegin();

        string directoryPath = dir.GetCurrentDir();
        string fileName = dir.GetNext();

        while (!string.IsNullOrEmpty(fileName))
        {
            if (!dir.CurrentIsDir() && fileName.EndsWith(".json"))
            {
                string path = directoryPath.PathJoin(fileName);

                FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Read);

                if (file == null)
                    throw new Exception($"Failed to read file '{path}': {FileAccess.GetOpenError()}");

                T result;
        
                try
                {
                    result = JsonConvert.DeserializeObject<T>(file.GetAsText());
                }
                catch (Exception exception)
                {
                    throw new Exception($"Failed to parse json file '{path}' into class '${typeof(T).Name}': {exception.Message}");
                }
        
                yield return result;
            }

            fileName = dir.GetNext();
        }
    }
}