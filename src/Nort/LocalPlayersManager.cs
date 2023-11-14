using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CtrlRaul;
using CtrlRaul.Godot.Linq;
using Newtonsoft.Json;
using Nort.Interface;

namespace Nort;

public class LocalPlayersManager : Singleton<LocalPlayersManager>
{
    public event Action<string> LocalPlayerDeleted;

    public Player NewLocalPlayer()
    {
        Player player = new();
        Blueprint blueprint = Assets.Instance.InitialBlueprint;

        player.id = Assets.Instance.GenerateUuid();
        player.blueprints.Add(blueprint);
        player.parts.Add(PartData.From(blueprint.core));
        player.parts.AddRange(blueprint.hulls.Select(PartData.From));

        return player;
    }

    public List<Player> GetPlayers()
    {
        var players = new List<Player>();
        const string path = GameConfig.Storage.LocalPlayers;
        
        if (!DirAccess.DirExistsAbsolute(path))
        {
            return players;
        }

        DirAccess directory = DirAccess.Open(path);
        
        if (directory == null)
        {
            logger.Error($"Failed to open local players directory: {DirAccess.GetOpenError()}");
            return players;
        }

        foreach (string fileName in directory.Files())
        {
            string filePath = path.PathJoin(fileName);
            try
            {
                FileAccess file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);

                if (file == null)
                    throw new Exception($"Failed to read local player file '{filePath}': {FileAccess.GetOpenError()}");

                string json = file.GetAsText();

                if (string.IsNullOrEmpty(json))
                    throw new Exception($"Empty local player file '{filePath}'");

                players.Add(JsonConvert.DeserializeObject<Player>(json));
            }
            catch (Exception exception)
            {
                logger.Error($"Failed to import local player file '{filePath}': {exception}");
            }
        }

        return players;
    }

    public static bool HasLocalPlayers()
    {
        if (!DirAccess.DirExistsAbsolute(GameConfig.Storage.LocalPlayers))
        {
            return false;
        }

        return DirAccess.GetFilesAt(GameConfig.Storage.LocalPlayers).Length > 0;
    }
    
    public void StoreLocalPlayer(Player player)
    {
        string path = GameConfig.Storage.LocalPlayers.PathJoin($"{player.id}.json");

        try
        {
            player.SaveJson(path);
        }
        catch (Exception exception)
        {
            string message = $"Failed to save local player file to '{path}': {exception.Message}";
            logger.Error(message);
            PopupsManager.Instance.Error(message);
        }
    }

    public Error Delete(string playerId)
    {
        string path = GetPathForPlayerFile(playerId);
        Error error = DirAccess.RemoveAbsolute(path);

        if (error != Error.Ok)
            return error;

        LocalPlayerDeleted?.Invoke(playerId);
        
        return Error.Ok;
    }
    
    // private static Error CreateLocalPlayersDir()
    // {
    //     if (DirAccess.DirExistsAbsolute(GameConfig.Storage.LocalPlayers))
    //     {
    //         return Error.Ok;
    //     }
    //
    //     return DirAccess.MakeDirRecursiveAbsolute(GameConfig.Storage.LocalPlayers);
    // }

    private static string GetPathForPlayerFile(string playerId)
    {
        return GameConfig.Storage.LocalPlayers.PathJoin(playerId + ".json");
    }
}