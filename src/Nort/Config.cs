using Godot;

namespace Nort;

public static class Config
{
    public const bool DebugAi = true;
    
    public const string VendorAssetsDirectory = "res://VendorAssets";
    public const string CustomAssetsDirectory = "user://CustomAssets";

    public static readonly string[] CarrierBlueprints = { "small_carrier" };
    public static readonly string[] DroneBlueprints = { "strike", "brisk" };
    public static readonly string[] TurretBlueprints = { "bullet_turret" };
    
    public const string SkillsPath = "res://Scenes/Skills";
    public const string PlayerFaction = "emerald";
    public const string DefaultEnemyFaction = "ruby";
    public const string DefaultCoreSkill = "core_bullet";
    public static readonly Color FactionlessColor = new(0.5f, 0.5f, 0.5f);

    public const string InitialPart = "core_1";
    public const string InitialBlueprint = "newborn";
    public const float DropRateCore = 1.0f / 1000f;
    public const float DropRateHull = 1.0f / 10f;
    public const float DropRateShiny = 1.0f / 1000f;

    public const string ConsoleOverlayScenePath = "res://Scenes/UI/Overlays/ConsoleOverlay.tscn";

    public static class Pages
    {
        public const string MainMenu = "res://Scenes/Pages/MainMenu.tscn";
        public const string LocalPlayers = "res://Scenes/Pages/LocalPlayersPage/LocalPlayersPage.tscn";
        public const string Lobby = "res://Scenes/Pages/LobbyPage.tscn";
        public const string CraftBuilder = "res://Scenes/Pages/CraftBuilder/CraftBuilder.tscn";
        public const string MissionHud = "res://Scenes/Pages/MissionHud/MissionHud.tscn";
        public const string MissionEditor = "res://Scenes/Pages/Editor/Editor.tscn";
    }

    public static class Storage
    {
        public const string LocalPlayers = "user://local_players";
    }
}