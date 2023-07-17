class_name GameConfig



const DEBUG = false



const CONFIG_PATH = "res://config/"


const PLAYER_FACTION = "emerald"
const INITIAL_BLUEPRINT = "newborn"
const FACTIONLESS_COLOR = Color(0.5, 0.5, 0.5)


const DROP_RATE_CORE = 1.0 / 1000
const DROP_RATE_HULL = 1.0 / 10
const DROP_RATE_SHINY = 1.0 / 1000


class Routes:
	const MAIN_MENU = "res://pages/main_menu/main_menu.tscn"
	const LOGIN = "res://pages/login/login.tscn"
	const LOBBY = "res://pages/lobby/lobby.tscn"
	const WORKSHOP = "res://pages/workshop/workshop.tscn"
	const MISSION = "res://pages/mission/mission.tscn"
	const MISSION_EDITOR = "res://pages/mission_editor/mission_editor.tscn"
