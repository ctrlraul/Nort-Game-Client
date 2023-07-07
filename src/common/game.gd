extends Node
signal finished_initializing()
signal current_player_changed(new_player: PlayerData)



var __initializing: bool = false
var __initialized: bool = false

var current_player: PlayerData :
	set(value):
		current_player = value
		current_player_changed.emit(value)
	get:
		return current_player

var dev: bool :
	get: return OS.has_feature("editor") && current_player == null



func _ready() -> void:

	DisplayServer.window_set_mode(DisplayServer.WINDOW_MODE_MAXIMIZED)

	if __is_running_main_scene():
		Transition.cover_instantly()
		await initialize()
		PagesManager.go_to(GameConfig.Routes.MAIN_MENU)
		Transition.uncover()
	else:
		Transition.uncover_instantly()



func initialize() -> Signal:

	if __initialized:
		return get_tree().create_timer(0.01).timeout

	if !__initializing:
		__initializing = true
		__initialize.call_deferred()

	return finished_initializing


func __initialize() -> void:
	Assets.import_assets(GameConfig.CONFIG_PATH)
	__initialized = true
	finished_initializing.emit()



func __is_running_main_scene() -> bool:
	var main_scene_path: String = ProjectSettings.get("application/run/main_scene")
	var current_scene_path = get_tree().current_scene.scene_file_path
	return main_scene_path == current_scene_path



func time(seconds) -> Signal:
	return get_tree().create_timer(seconds).timeout
