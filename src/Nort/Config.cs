using Godot;

namespace Nort;

public static class Config
{
    public const string VendorAssetsDirectory = "res://VendorAssets";
    public const string CustomAssetsDirectory = "user://CustomAssets";
    
    public const string SkillsPath = "res://gimmicks";
    public const string PlayerFaction = "emerald";
    public const string DefaultEnemyFaction = "ruby";
    public const string DefaultCoreSkill = "core_bullet";
    public static readonly Color FactionlessColor = new(0.5f, 0.5f, 0.5f);

    public const string InitialBlueprint = "newborn";
    public const float DropRateCore = 1.0f / 1000f;
    public const float DropRateHull = 1.0f / 10f;
    public const float DropRateShiny = 1.0f / 1000f;

    public static class Pages
    {
        public const string MainMenu = "res://Scenes/Pages/MainMenuPage.tscn";
        public const string LocalPlayers = "res://Scenes/Pages/LocalPlayersPage/LocalPlayersPage.tscn";
        public const string Lobby = "res://Scenes/Pages/LobbyPage.tscn";
        public const string CraftBuilder = "res://Scenes/Pages/CraftBuilder/CraftBuilder.tscn";
        public const string Mission = "res://Scenes/Pages/MissionPage.tscn";
        public const string MissionEditor = "res://Scenes/Pages/MissionEditor/MissionEditor.tscn";
    }

    public static class Storage
    {
        public const string LocalPlayers = "user://local_players";
    }
}