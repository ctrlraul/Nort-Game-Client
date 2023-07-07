extends Page



func _mount() -> void:

	await Game.initialize()

	Stage.clear()
	Stage.spawn_player()#.crippled = true

	var turret = Stage.spawn_turret()

	turret.position.x = 300
#	turret.set_blueprint()


func _on_workshop_button_pressed() -> void:
	Transition.callback(
		PagesManager.go_to.bind(GameConfig.Routes.WORKSHOP)
	)
