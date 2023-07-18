extends Page



@export var PauseOverlayScene: PackedScene

@onready var hull_progress_bar: Control = %HullProgressBar
@onready var core_progress_bar: Control = %CoreProgressBar

var from_editor: bool = false
var mission: Mission = null



func _ready() -> void:
	Stage.player_spawned.connect(_on_player_spawned)
	Stage.player_destroyed.connect(_on_player_destroyed)


func _mount(data) -> void:

	await Game.initialize()

	Stage.clear()

	if data != null:
		from_editor = data.from_editor
		mission = data.mission
		Stage.load_mission(mission)


func _input(_event: InputEvent) -> void:
	if Input.is_action_just_pressed("escape") && visible:
		visible = false
		var pause = PauseOverlayScene.instantiate()
		add_child(pause)
		pause.unpause.connect(set_deferred.bind("visible", true))
		pause.forfeit.connect(forfeit)
		pause.quit.connect(get_tree().quit)



func forfeit() -> void:

	if from_editor:

		Transition.callback(
			PagesManager.go_to.bind(GameConfig.Routes.MISSION_EDITOR, { "mission": mission })
		)

	else:

		Transition.callback(
			PagesManager.go_to.bind(GameConfig.Routes.LOBBY)
		)



func _on_player_spawned(player: Craft) -> void:
	hull_progress_bar.color = player.faction.color
	hull_progress_bar.progress = 1
	core_progress_bar.progress = 1
	player.stats_changed.connect(_on_player_stats_changed)


func _on_player_destroyed() -> void:
	hull_progress_bar.progress = 0
	core_progress_bar.progress = 0
	set_process(false)


func _on_player_stats_changed(player: Craft) -> void:
	hull_progress_bar.progress = player.hull / player.hull_max
	core_progress_bar.progress = player.core / player.core_max
