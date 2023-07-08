extends Page



func _mount() -> void:

	await Game.initialize()

	Stage.clear()
	Stage.load_mission(Assets.missions["basics"])
	Stage.spawn_player()#.crippled = true



func _on_workshop_button_pressed() -> void:
	Transition.callback(
		PagesManager.go_to.bind(GameConfig.Routes.WORKSHOP)
	)
