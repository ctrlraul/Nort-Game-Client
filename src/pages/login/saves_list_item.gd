class_name SavesListItem
extends PanelContainer

signal delete(player_data: PlayerData)
signal select(player_data: PlayerData)



@onready var craft_display: CraftDisplay = %CraftDisplay
@onready var nick_label: Label = %Nick
@onready var score_label: Label = %Score


var __player_data: PlayerData



func set_player_data(player_data: PlayerData) -> void:

	__player_data = player_data

	nick_label.text = player_data.nick
	score_label.text = str(player_data.score)
	craft_display.set_blueprint(player_data.current_blueprint)
	craft_display.set_color(Assets.player_faction.color)



func _on_delete_button_pressed() -> void:
	if __player_data:
		delete.emit(__player_data)


func _on_select_button_pressed() -> void:
	if __player_data:
		select.emit(__player_data)
