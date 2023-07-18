extends Page



@onready var mission_editor_button: Button = %MissionEditorButton
@onready var craft_display: Control = %CraftDisplay



func _ready() -> void:

	Stage.clear()
	Stage.camera.zoom = Vector2.ONE * 0.25

	var tween = Stage.create_tween()

	tween.set_ease(Tween.EASE_OUT)
	tween.set_trans(Tween.TRANS_EXPO)
	tween.tween_property(Stage.camera, "zoom", Vector2.ONE * 0.5, 10)

	mission_editor_button.visible = OS.has_feature("editor")


func _mount(_data) -> void:

	await Game.initialize()

	if Game.current_player:
		craft_display.set_blueprint(Game.current_player.current_blueprint)
		craft_display.color = Assets.player_faction.color
	else:
		craft_display.queue_free()



func _on_start_button_pressed() -> void:
	if PlayerManager.has_local_players():
		PagesManager.go_to(GameConfig.Routes.LOGIN)
	else:
		Game.current_player = PlayerManager.new_local_player()
		PlayerManager.store_local_player(Game.current_player)
		Transition.callback(PagesManager.go_to.bind(GameConfig.Routes.LOBBY))


func _on_mission_editor_button_pressed() -> void:
	Transition.callback(PagesManager.go_to.bind(GameConfig.Routes.MISSION_EDITOR))
