using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CtrlRaul;
using CtrlRaul.Godot.Linq;
using Godot;
using Newtonsoft.Json;

namespace Nort;

public class Assets : Singleton<Assets>
{
    public static readonly Texture2D MISSING_PART_TEXTURE;
    //public static readonly Texture2D MISSING_SKILL_TEXTURE;
    public static readonly Texture2D CORE_LIGHT_TEXTURE;
    public static readonly ShaderMaterial SHINY_MATERIAL;
    public static readonly ShaderMaterial PART_OUTLINE_MATERIAL;

    private const string SkillsDirectoryName = "skills";
    private const string PartsDirectoryName = "parts";
    private const string BlueprintsDirectoryName = "blueprints";
    private const string FactionsDirectoryName = "factions";
    private const string MissionsDirectoryName = "missions";
    private const string PartTexturesDirectoryName = "part_textures";
    private const string SkillTexturesDirectory = "res://assets/images/skills";
    public const string LocalMissionsDirectory = "user://local_missions";

    private string _assetsPath;
    private readonly Dictionary<string, Skill> _skills = new();
    private readonly Dictionary<string, Part> _parts = new();
    private readonly Dictionary<string, Blueprint> _blueprints = new();
    private readonly Dictionary<string, Faction> _factions = new();
    private readonly Dictionary<string, Mission> _missions = new();
    private readonly Dictionary<string, Texture2D> _partTextures = new();
    private readonly Dictionary<string, Texture2D> _skillTextures = new();

    public Faction PlayerFaction => _factions[GameConfig.PlayerFaction];
    public Blueprint InitialBlueprint => _blueprints[GameConfig.InitialBlueprint];

    public Skill DefaultCoreSkill => _skills[GameConfig.DefaultCoreSkill];

    static Assets()
    {
        MISSING_PART_TEXTURE = GD.Load<Texture2D>("res://assets/images/part_example.png");
        // MISSING_SKILL_TEXTURE = GD.Load<Texture2D>("res://assets/images/missing_skill.png");
        CORE_LIGHT_TEXTURE = GD.Load<Texture2D>("res://assets/images/core_light.png");
        SHINY_MATERIAL = GD.Load<ShaderMaterial>("res://common/materials/shiny_part_shader_material.tres");
        PART_OUTLINE_MATERIAL = GD.Load<ShaderMaterial>("res://common/materials/part_outline_shader_material.tres");
    }

    public Task ImportAssets(string assetsPath)
    {
        _assetsPath = assetsPath;
        
        logger.Log("Importing Assets...");

        ImportSkills();
        ImportParts();
        ImportBlueprints();
        ImportMissions();
        ImportFactions();
        ImportPartTextures();

        logger.Log($"Skills: {_skills.Count}");
        logger.Log($"Parts: {_parts.Count}");
        logger.Log($"Blueprints: {_blueprints.Count}");
        logger.Log($"Missions: {_missions.Count}");
        logger.Log($"Factions: {_factions.Count}");
        logger.Log($"Skill Textures: {_partTextures.Count}");

        return Task.Delay(100);
    }
    
    private void ImportSkills() => MapJsonDirIntoDict(SkillsDirectoryName, _skills, skill => skill.id);
    private void ImportParts() => MapJsonDirIntoDict(PartsDirectoryName, _parts, part => part.id);
    private void ImportBlueprints()
    {
        MapJsonDirIntoDict(BlueprintsDirectoryName, _blueprints, blueprint => blueprint.id);

        if (!_blueprints.ContainsKey(GameConfig.InitialBlueprint))
            throw new Exception($"No blueprint imported with the id set in 'GameConfig.InitialBlueprint' ({GameConfig.InitialBlueprint})");
    }
    private void ImportMissions() => MapJsonDirIntoDict(MissionsDirectoryName, _missions, mission => mission.id);
    private void ImportFactions() => MapJsonDirIntoDict(FactionsDirectoryName, _factions, faction => faction.id);

    private void MapJsonDirIntoDict<T>(string dirName, IDictionary<string, T> dictionary, Func<T, string> keyFn)
    {
        string dirPath = _assetsPath.PathJoin(dirName);
        
        DirAccess dir = DirAccess.Open(dirPath);

        if (dir == null)
            throw new Exception($"Failed to open '{dirPath}': {DirAccess.GetOpenError()}");

        foreach (string fileName in dir.Files())
        {
            string path = dirPath.PathJoin(fileName);

            FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Read);

            if (file == null)
            {
                PopupsManager.Instance.Error($"Failed to read {typeof(T).Name} file '{path}': {FileAccess.GetOpenError()}");
                continue;
            }

            T result;
            
            try
            {
                result = JsonConvert.DeserializeObject<T>(file.GetAsText());
            }
            catch (Exception exception)
            {
                throw new Exception($"Failed to parse json file '{path}': {exception.Message}");
            }
            
            dictionary.Add(keyFn.Invoke(result), result);
        }
    }

    private void ImportPartTextures()
    {
        if (!_parts.Any())
        {
            logger.Warn("ImportPartTextures :: No parts loaded!");
            return;
        }
        
        _partTextures.Clear();
        
        string dirPath = _assetsPath.PathJoin(PartTexturesDirectoryName);

        foreach (Part part in _parts.Values)
        {
            string path = dirPath.PathJoin(part.textureName);
            Texture2D texture = GD.Load<Texture2D>(path);

            if (texture == null)
            {
                logger.Error($"Failed to load texture for part '{part.displayName}' (id {part.id}) from '{path}'");
                continue;
            }
            
            _partTextures[part.id] = texture;
        }
    }

    public Part GetPart(string id) => _parts[id];
    public Blueprint GetBlueprint(string id) => _blueprints[id];
    public Faction GetFaction(string id) => _factions[id];
    public Skill GetSkill(string id) => _skills[id];

    public string GenerateUuid() => Guid.NewGuid().ToString();
    
    public Texture2D GetPartTexture(string partId)
    {
        return _partTextures.TryGetValue(partId, out Texture2D texture) ? texture : MISSING_PART_TEXTURE;
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
        return _skillTextures.TryGetValue(id, out Texture2D texture) ? texture : null;
        //return _skillTextures.TryGetValue(id, out Texture2D texture) ? texture : MISSING_SKILL_TEXTURE;
    }

    public bool IsCore(Part part)
    {
        return part.type == Part.Type.Core;
    }

    public bool IsCore(BlueprintPart blueprintPart)
    {
        return IsCore(blueprintPart.Part);
    }
}