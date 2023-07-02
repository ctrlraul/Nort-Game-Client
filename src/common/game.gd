extends Node
signal finished_initializing()
signal current_player_changed(new_player: PlayerData)



var initialized: bool = false
var current_player: PlayerData :
	set(value):
		current_player = value
		current_player_changed.emit(value)
	get:
		return current_player



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
	assert(initialized == false, "Already initialized")
	__initialize.call_deferred()
	return finished_initializing


func __initialize() -> void:
	Assets.import_assets(GameConfig.ASSETS_DATA_PATH)
	initialized = true
	finished_initializing.emit()



func __is_running_main_scene() -> bool:
	var main_scene_path: String = ProjectSettings.get("application/run/main_scene")
	var current_scene_path = get_tree().current_scene.scene_file_path
	return main_scene_path == current_scene_path



func time(seconds) -> Signal:
	return get_tree().create_timer(seconds).timeout
