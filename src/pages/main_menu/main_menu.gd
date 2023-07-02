extends Page



func _on_play_button_pressed() -> void:
	if PlayerDataManager.has_local_players():
		PagesManager.go_to(GameConfig.Routes.LOGIN)
	else:
		Game.current_player = PlayerDataManager.new_local_player()
		PlayerDataManager.store_local_player(Game.current_player)
		Transition.callback(PagesManager.go_to.bind(GameConfig.Routes.LOBBY))
