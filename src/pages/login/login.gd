extends Page



@export var SavesListItemScene: PackedScene

@onready var saves_list: VBoxContainer = %SavesList




func _mount() -> void:

	if !Game.initialized:
		await Game.initialize()

	__refresh_saves_list()



func __refresh_saves_list() -> void:

	NodeUtils.clear(saves_list)

	for player in PlayerDataManager.get_local_players():
		var item: SavesListItem = SavesListItemScene.instantiate()
		saves_list.add_child(item)
		item.set_player_data(player)
		item.delete.connect(_on_local_player_delete)
		item.select.connect(_on_local_player_select)



func _on_return_button_pressed() -> void:
	PagesManager.go_to(GameConfig.Routes.MAIN_MENU)


func _on_refresh_button_pressed() -> void:
	__refresh_saves_list()


func _on_local_player_delete(player: PlayerData) -> void:
	PlayerDataManager.delete_local_player(player.id)
	__refresh_saves_list()


func _on_local_player_select(player: PlayerData) -> void:
	Game.current_player = player
	Transition.callback(PagesManager.go_to.bind(GameConfig.Routes.LOBBY))
