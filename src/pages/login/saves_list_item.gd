class_name SavesListItem
extends PanelContainer

signal select(player_data: PlayerData)



@onready var craft_display: CraftDisplay = %CraftDisplay
@onready var nick_label: Label = %Nick
@onready var score_label: Label = %Score


var __player_data: PlayerData



func set_player_data(player_data: PlayerData) -> void:

	__player_data = player_data

	var part_count = player_data.current_blueprint.parts.size() + 1

	nick_label.text = player_data.nick
	score_label.text = str(player_data.score)
	craft_display.set_blueprint(player_data.current_blueprint)
	craft_display.color = Assets.player_faction.color
	craft_display.scale = Vector2.ONE * clamp(1 / (part_count * 0.225) * 0.5, 0.25, 0.5)



func __delete() -> void:
	var result = PlayerDataManager.delete_local_player(__player_data)
	if result.error != "":
		PopupsManager.error(result.error)



func _on_delete_button_pressed() -> void:
	var popup = PopupsManager.warn("Permanently delete '%s'?" % __player_data.nick)
	popup.add_button("Yes", __delete)
	popup.add_button("No")



func _on_select_button_pressed() -> void:
	if __player_data:
		select.emit(__player_data)
