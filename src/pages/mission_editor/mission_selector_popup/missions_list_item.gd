class_name MissionsListItem extends MarginContainer

signal selected(mission: Mission)



@onready var __craft_display: CraftDisplay = %CraftDisplay
@onready var __id_label: Label = %IDLabel
@onready var __display_name_label: Label = %DisplayNameLabel
@onready var __entities_count_label: Label = %EntitiesCountLabel



var __mission: Mission



func _ready() -> void:
	__craft_display.color = GameConfig.FACTIONLESS_COLOR



func set_mission(mission: Mission) -> void:
	__mission = mission
	__display_name_label.text = mission.display_name
	__id_label.text = mission.id
	__entities_count_label.text = "Entities: %s" % str(mission.entities.size())



func _on_button_pressed() -> void:
	if __mission != null:
		selected.emit(__mission)
