using System;
using Godot;
using Newtonsoft.Json;

namespace Nort.Interface;

public interface ISavable {}

public static class ISavableExtensions
{
	public static void SaveJson(this ISavable savable, string path)
    {
        string dirPath = path.GetBaseDir();

        Error createDirError = DirAccess.MakeDirRecursiveAbsolute(dirPath);

        if (createDirError != Error.Ok)
            throw new Exception($"Failed to create directory: {createDirError}");
        
        string json = JsonConvert.SerializeObject(savable, Formatting.Indented);

        FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Write);

        if (file == null)
            throw new Exception($"Failed to write file: {FileAccess.GetOpenError()}");

        file.StoreString(json);
        file.Close();
    }
}