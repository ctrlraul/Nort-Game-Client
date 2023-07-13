extends Page



@onready var hull_progress_bar: Control = %HullProgressBar
@onready var core_progress_bar: Control = %CoreProgressBar



func _ready() -> void:
	Stage.player_spawned.connect(_on_player_spawned)
	Stage.player_destroyed.connect(_on_player_destroyed)


func _mount(data) -> void:

	await Game.initialize()

	Stage.clear()

	if data:
		Stage.load_mission(Assets.missions[data.mission])

	Stage.spawn_player()



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
