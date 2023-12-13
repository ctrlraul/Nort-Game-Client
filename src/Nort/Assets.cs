using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CtrlRaul;
using CtrlRaul.Godot.Linq;
using Godot;
using Nort.Interface;
using Nort.Popups;

namespace Nort;

public class Assets : Singleton<Assets>
{
    private class AssetsLibrary
    {
        public readonly Dictionary<string, Skill> skills = new();
        public readonly Dictionary<string, Part> parts = new();
        public readonly Dictionary<string, Blueprint> blueprints = new();
        public readonly Dictionary<string, Faction> factions = new();
        public readonly Dictionary<string, Mission> missions = new();

        public void Clear()
        {
            skills.Clear();
            parts.Clear();
            blueprints.Clear();
            factions.Clear();
            missions.Clear();
        }
    }
    
    public static readonly Texture2D MissingPartTexture = GD.Load<Texture2D>("res://images/part_example.png");
    public static readonly Texture2D MissingSkillTexture = GD.Load<Texture2D>("res://images/skill_placeholder.png");
    public static readonly Texture2D CoreLightTexture = GD.Load<Texture2D>("res://images/core_light.png");
    public static readonly ShaderMaterial ShinyMaterial = GD.Load<ShaderMaterial>("res://Materials/shiny_part_shader_material.tres");
    public static readonly ShaderMaterial PartOutlineMaterial = GD.Load<ShaderMaterial>("res://Materials/part_outline_shader_material.tres");

    public const string SkillTexturesDirectory = "res://images/skills";
    public const string SkillsDirectoryName = "skills";
    public const string PartsDirectoryName = "parts";
    public const string PartTexturesDirectoryName = "part_textures";
    public const string FactionsDirectoryName = "factions";
    public const string MissionsDirectoryName = "missions";
    public const string BlueprintsDirectoryName = "blueprints";

    private readonly Dictionary<string, Texture2D> skillTextures = new();
    private readonly Dictionary<string, Texture2D> partTextures = new();
    private readonly AssetsLibrary vendorAssets = new();
    private readonly AssetsLibrary customAssets = new();

    
    public Faction PlayerFaction => GetFaction(Config.PlayerFaction);
    public Faction DefaultEnemyFaction => GetFaction(Config.DefaultEnemyFaction);
    public Blueprint InitialBlueprint => GetBlueprint(Config.InitialBlueprint);
    public Skill DefaultCoreSkill => GetSkill(Config.DefaultCoreSkill);

    
    public async Task ImportAssets()
    {
        logger.Log("Importing Assets...");
        
        if (!DirAccess.DirExistsAbsolute(Config.VendorAssetsDirectory))
            throw new Exception($"Vendor assets directory '{Config.VendorAssetsDirectory}' not found");
        
        DirAccess.MakeDirRecursiveAbsolute(Config.CustomAssetsDirectory);
        
        partTextures.Clear();
        vendorAssets.Clear();
        customAssets.Clear();
        
        await HydrateAssetsLibrary(vendorAssets, Config.VendorAssetsDirectory);
        await HydrateAssetsLibrary(customAssets, Config.CustomAssetsDirectory);

        List<string> problems = FindProblems();

        if (problems.Any())
        {
            string message = problems.Aggregate(
                $"{problems.Count} problem found, the game may break.",
                (current, problem) => current + $"\n - {problem}"
            );
            
            logger.Error(message);
            
            if (OS.HasFeature("editor"))
            {
                PopupsManager.Instance.Info(message);
            }
            else
            {
                DialogPopup popup = PopupsManager.Instance.Info("Error while loading assets!");
                popup.AddButton("Ok", () => (Engine.GetMainLoop() as SceneTree)!.Quit());
            }
        }
        
        logger.Log($"Skills: {vendorAssets.skills.Count + customAssets.skills.Count}");
        logger.Log($"Parts: {vendorAssets.parts.Count + customAssets.parts.Count}");
        logger.Log($"Blueprints: {vendorAssets.blueprints.Count + customAssets.blueprints.Count}");
        logger.Log($"Missions: {vendorAssets.missions.Count + customAssets.missions.Count}");
        logger.Log($"Factions: {vendorAssets.factions.Count + customAssets.factions.Count}");
    }
    
    private Task HydrateAssetsLibrary(AssetsLibrary assetsLibrary, string assetsDirectory)
    {
        string skillsDirectoryPath = assetsDirectory.PathJoin(SkillsDirectoryName);
        string partsDirectoryPath = assetsDirectory.PathJoin(PartsDirectoryName);
        string partTexturesDirectoryPath = assetsDirectory.PathJoin(PartTexturesDirectoryName);
        string blueprintsDirectoryPath = assetsDirectory.PathJoin(BlueprintsDirectoryName);
        string factionsDirectoryPath = assetsDirectory.PathJoin(FactionsDirectoryName);
        string missionsDirectoryPath = assetsDirectory.PathJoin(MissionsDirectoryName);

        if (DirAccess.DirExistsAbsolute(skillsDirectoryPath))
        {
            foreach (Skill skill in OpenDirOrThrow(skillsDirectoryPath).ParseJsonFiles<Skill>())
            {
                assetsLibrary.skills.Add(skill.id, skill);
            
                string path = SkillTexturesDirectory.PathJoin(skill.id + ".png");
                Texture2D texture = GD.Load<Texture2D>(path);

                if (texture == null)
                {
                    logger.Error($"Failed to load texture for skill '{skill.id}' from '{path}'");
                    continue;
                }
            
                skillTextures.Add(skill.id, texture);
            }
        }

        if (DirAccess.DirExistsAbsolute(partsDirectoryPath))
        {
            foreach (Part part in OpenDirOrThrow(partsDirectoryPath).ParseJsonFiles<Part>())
            {
                assetsLibrary.parts.Add(part.id, part);
            
                string path = partTexturesDirectoryPath.PathJoin(part.textureName);
                Texture2D texture = GD.Load<Texture2D>(path);

                if (texture == null)
                {
                    logger.Error($"Failed to load texture for part '{part.id}' from '{path}'");
                    continue;
                }
            
                partTextures.Add(part.id, texture);
            }
        }

        if (DirAccess.DirExistsAbsolute(blueprintsDirectoryPath))
        {
            foreach (Blueprint item in OpenDirOrThrow(blueprintsDirectoryPath).ParseJsonFiles<Blueprint>())
            {
                assetsLibrary.blueprints.Add(item.id, item);
            }
        }

        if (DirAccess.DirExistsAbsolute(factionsDirectoryPath))
        {
            foreach (Faction item in OpenDirOrThrow(factionsDirectoryPath).ParseJsonFiles<Faction>())
            {
                assetsLibrary.factions.Add(item.id, item);
            }
        }

        if (DirAccess.DirExistsAbsolute(missionsDirectoryPath))
        {
            foreach (Mission item in OpenDirOrThrow(missionsDirectoryPath).ParseJsonFiles<Mission>())
            {
                assetsLibrary.missions.Add(item.id, item);
            }
        }

        return Task.CompletedTask;
    }

    private List<string> FindProblems()
    {
        List<string> problems = new();
        
        if (!vendorAssets.blueprints.ContainsKey(Config.InitialBlueprint))
            problems.Add($"Config: No matching blueprint for initial blueprint with id '{Config.InitialBlueprint}'");

        foreach (string blueprintId in Config.CarrierBlueprints)
        {
            if (!vendorAssets.blueprints.ContainsKey(blueprintId))
                problems.Add($"Config: No blueprint for carrier found with id '{blueprintId}'");
        }

        return problems;
    }
    
    private static DirAccess OpenDirOrThrow(string directoryPath)
    {
        DirAccess dir = DirAccess.Open(directoryPath);

        if (dir == null)
            throw new Exception($"Failed to open '{directoryPath}': {DirAccess.GetOpenError()}");

        return dir;
    }

    
    public IEnumerable<Part> GetParts()
    {
        return vendorAssets.parts.Values.Concat(customAssets.parts.Values);
    }
    
    public IEnumerable<Part> GetCoreParts()
    {
        return GetParts().Where(IsCore);
    }

    public IEnumerable<Part> GetHullParts()
    {
        return GetParts().Where(IsNotCore);
    }
    
    public Part GetPart(string id)
    {
        return vendorAssets.parts.TryGetValue(id, out Part item)
            ? item
            : customAssets.parts[id];
    }

    
    public IEnumerable<Blueprint> GetBlueprints()
    {
        return vendorAssets.blueprints.Values.Concat(customAssets.blueprints.Values);
    }

    public Blueprint GetBlueprint(string id)
    {
        return vendorAssets.blueprints.TryGetValue(id, out Blueprint item)
            ? item
            : customAssets.blueprints[id];
    }
    
    public Rect2 GetBlueprintVisualRect(Blueprint blueprint)
    {
        List<BlueprintPart> parts = blueprint.hulls.Concat(new[] { blueprint.core }).ToList();

        if (!parts.Any())
            return new Rect2();
        
        Vector2 topLeft = new(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 bottomRight = new(float.NegativeInfinity, float.NegativeInfinity);

        foreach (BlueprintPart blueprintPart in parts)
        {
            Vector2 textureSize = GetPartTexture(blueprintPart.partId).GetSize();
            Vector2 blueprintPartPlace = blueprintPart.Place;

            topLeft.X = Mathf.Min(topLeft.X, blueprintPartPlace.X - textureSize.X * 0.5f);
            topLeft.Y = Mathf.Min(topLeft.Y, blueprintPartPlace.Y - textureSize.Y * 0.5f);

            bottomRight.X = Mathf.Max(bottomRight.X, blueprintPartPlace.X + textureSize.X * 0.5f);
            bottomRight.Y = Mathf.Max(bottomRight.Y, blueprintPartPlace.Y + textureSize.Y * 0.5f);
        }
        
        return new Rect2(topLeft, (bottomRight - topLeft).Abs());
    }
    
    public void StoreBlueprint(Blueprint blueprint)
    {
        string assetsDirectory;
        AssetsLibrary assetsLibrary;

        if (Game.Instance.Dev)
        {
            assetsDirectory = Config.VendorAssetsDirectory;
            assetsLibrary = vendorAssets;
        }
        else
        {
            assetsDirectory = Config.CustomAssetsDirectory;
            assetsLibrary = customAssets;
        }
        
        string path = assetsDirectory
            .PathJoin(BlueprintsDirectoryName)
            .PathJoin(blueprint.id + ".json");
        
        blueprint.SaveJson(path);

        assetsLibrary.blueprints[blueprint.id] = blueprint;
        
        logger.Log($"Blueprint stored at '{path}'");
    }
    
    public void StoreMission(Mission mission)
    {
        logger.Log($"mission.displayName: {mission.displayName}");
        logger.Log($"mission.id: {mission.id}");
        
        string assetsDirectory;
        AssetsLibrary assetsLibrary;

        if (Game.Instance.Dev)
        {
            assetsDirectory = Config.VendorAssetsDirectory;
            assetsLibrary = vendorAssets;
        }
        else
        {
            assetsDirectory = Config.CustomAssetsDirectory;
            assetsLibrary = customAssets;
        }
        
        string path = assetsDirectory
            .PathJoin(MissionsDirectoryName)
            .PathJoin(mission.id + ".json");
        
        mission.SaveJson(path);

        assetsLibrary.missions[mission.id] = mission;
        
        logger.Log($"Mission stored at '{path}'");
    }
    
    
    public IEnumerable<Faction> GetFactions()
    {
        return vendorAssets.factions.Values.Concat(customAssets.factions.Values);
    }

    public Faction GetFaction(string id)
    {
        return vendorAssets.factions.TryGetValue(id, out Faction item)
            ? item
            : customAssets.factions[id];
    }
    
    
    public IEnumerable<Skill> GetSkills()
    {
        return vendorAssets.skills.Values.Concat(customAssets.skills.Values);
    }

    public Skill GetSkill(string id)
    {
        return vendorAssets.skills.TryGetValue(id, out Skill item)
            ? item
            : customAssets.skills[id];
    }


    public IEnumerable<Mission> GetMissions()
    {
        return vendorAssets.missions.Values.Concat(customAssets.missions.Values);
    }

    public Mission GetMission(string id)
    {
        return vendorAssets.missions.TryGetValue(id, out Mission item)
            ? item
            : customAssets.missions[id];
    }


    public Texture2D GetPartTexture(string partId)
    {
        return partTextures.TryGetValue(partId, out Texture2D texture) ? texture : MissingPartTexture;
    }

    public Texture2D GetPartTexture(Part part)
    {
        return GetPartTexture(part.id);
    }

    public Texture2D GetPartTexture(BlueprintPart blueprintPart)
    {
        return GetPartTexture(blueprintPart.partId);
    }

    
    public Texture2D GetSkillTexture(string id)
    {
        return skillTextures.TryGetValue(id, out Texture2D texture) ? texture : MissingSkillTexture;
    }
    

    public static bool IsCore(Part part)
    {
        return part.type == Part.Type.Core;
    }

    public static bool IsNotCore(Part part)
    {
        return !IsCore(part);
    }


    public static string GenerateUuid()
    {
        return Guid.NewGuid().ToString();
    }
}