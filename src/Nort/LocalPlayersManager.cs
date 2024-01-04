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

    public static Player NewLocalPlayer()
    {
        Player player = new()
        {
            id = Assets.GenerateUuid(),
            nick = "noob",
            blueprint = Blueprint.From(Assets.Instance.InitialBlueprint)
        };

        AddPartsForBlueprint(player, player.blueprint);

        return player;
    }

    public List<Player> GetPlayers()
    {
        var players = new List<Player>();
        const string path = Config.Storage.LocalPlayers;
        
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
        if (!DirAccess.DirExistsAbsolute(Config.Storage.LocalPlayers))
        {
            return false;
        }

        return DirAccess.GetFilesAt(Config.Storage.LocalPlayers).Length > 0;
    }
    
    public void StoreLocalPlayer(Player player)
    {
        string path = Config.Storage.LocalPlayers.PathJoin($"{player.id}.json");

        try
        {
            player.SaveJson(path);
            logger.Log("Progress Stored");
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
    //     if (DirAccess.DirExistsAbsolute(Config.Storage.LocalPlayers))
    //     {
    //         return Error.Ok;
    //     }
    //
    //     return DirAccess.MakeDirRecursiveAbsolute(Config.Storage.LocalPlayers);
    // }

    public void AddPart(Player player, PartData partData)
    {
        player?.parts.Add(partData);
    }

    public void UpdateMissionRecord(Player player, string missionId, float time, float score)
    {
        if (!player.missionRecords.TryGetValue(missionId, out MissionRecord record))
            record = new MissionRecord();

        record.bestTime = Mathf.Min(record.bestTime, time);
        record.bestScore = Mathf.Max(record.bestScore, score);

        player.missionRecords[missionId] = record;
    }

    private static string GetPathForPlayerFile(string playerId)
    {
        return Config.Storage.LocalPlayers.PathJoin(playerId + ".json");
    }
    
    private static void AddPartsForBlueprint(Player player, Blueprint blueprint)
    {
        player.parts.Add(PartData.From(blueprint.core));
        player.parts.AddRange(blueprint.hulls.Select(PartData.From));
    }
}