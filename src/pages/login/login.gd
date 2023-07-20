extends Page



@export var SavesListItemScene: PackedScene

@onready var saves_list: VBoxContainer = %SavesList



func _ready() -> void:
	Stage.clear()
	PlayerManager.local_player_deleted.connect(__refresh_saves_list)


func _mount(_data) -> void:
	await Game.initialize()
	__refresh_saves_list()



func __refresh_saves_list() -> void:

	NodeUtils.clear(saves_list)

	for player in PlayerManager.get_local_players():
		var item: SavesListItem = SavesListItemScene.instantiate()
		saves_list.add_child(item)
		item.set_player_data(player)
		item.select.connect(_on_local_player_select)



func _on_refresh_button_pressed() -> void:
	__refresh_saves_list()


func _on_local_player_select(player: Player) -> void:
	Game.current_player = player
	Transition.callback(PagesManager.go_to.bind(GameConfig.Routes.LOBBY))
