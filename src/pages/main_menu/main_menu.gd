extends Page



func _ready() -> void:

	Stage.camera.zoom = Vector2.ONE * 0.25

	var tween = Stage.create_tween()

	tween.set_ease(Tween.EASE_OUT)
	tween.set_trans(Tween.TRANS_EXPO)
	tween.tween_property(Stage.camera, "zoom", Vector2.ONE * 0.5, 10)


func _on_play_button_pressed() -> void:
	if PlayerDataManager.has_local_players():
		PagesManager.go_to(GameConfig.Routes.LOGIN)
	else:
		Game.current_player = PlayerDataManager.new_local_player()
		PlayerDataManager.store_local_player(Game.current_player)
		Transition.callback(PagesManager.go_to.bind(GameConfig.Routes.LOBBY))
