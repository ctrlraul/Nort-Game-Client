class_name BlueprintsListItem extends MarginContainer

signal selected(blueprint: CraftBlueprint)



@onready var __craft_display: CraftDisplay = %CraftDisplay
@onready var __id_label: Label = %IDLabel
@onready var __core_label: Label = %CoreLabel
@onready var __parts_count_label: Label = %PartsCountLabel



var __blueprint: CraftBlueprint



func _ready() -> void:
	__craft_display.color = GameConfig.FACTIONLESS_COLOR


func set_blueprint(blueprint: CraftBlueprint) -> void:
	__blueprint = blueprint
	__craft_display.set_blueprint(blueprint)
	__id_label.text = blueprint.id
	__core_label.text = "Core: %s" % blueprint.core.data.definition.display_name
	__parts_count_label.text = "Parts: %s" % str(blueprint.parts.size() + 1)


func _on_button_pressed() -> void:
	if __blueprint != null:
		selected.emit(__blueprint)
