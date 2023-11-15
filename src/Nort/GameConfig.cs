using Godot;

namespace Nort;

public class GameConfig
{
    public const bool Debug = true;
    public const string ConfigPath = "res://config";
    public const string SkillsPath = "res://gimmicks";
    public const string PlayerFaction = "emerald";
    public const string DefaultCoreSkill = "core_bullet";
    public const string EnemyFaction1 = "ruby";
    public const string EnemyFaction2 = "cerulean";
    public const string EnemyFaction3 = "amber";
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
        public const string Mission = "res://pages/mission/mission.tscn";
        public const string MissionEditor = "res://pages/mission_editor/mission_editor.tscn";
    }

    public static class Storage
    {
        public const string LocalPlayers = "user://local_players";
    }
}