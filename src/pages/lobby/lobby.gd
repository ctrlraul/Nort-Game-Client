extends Page



func _mount() -> void:

	if !Game.initialized:
		await Game.initialize()

	Stage.clear()
	Stage.spawn_player()#.crippled = true
	Stage.spawn_turret()


func _on_workshop_button_pressed() -> void:
	Transition.callback(
		PagesManager.go_to.bind(GameConfig.Routes.WORKSHOP)
	)
