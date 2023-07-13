extends Page



func _mount(_data) -> void:
	await Game.initialize()
	Stage.spawn_player()



func _on_workshop_button_pressed() -> void:
	Transition.callback(
		PagesManager.go_to.bind(GameConfig.Routes.WORKSHOP)
	)


func _on_test_button_pressed() -> void:
	Transition.callback(
		PagesManager.go_to.bind(GameConfig.Routes.MISSION, {
			"mission": "basics"
		})
	)
