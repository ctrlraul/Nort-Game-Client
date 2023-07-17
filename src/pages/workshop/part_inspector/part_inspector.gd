extends PanelContainer



@onready var container: HBoxContainer = %Container
@onready var craft_part_display: Control = %CraftPartDisplay
@onready var name_label: Label = %NameLabel
@onready var gimmick_icon: TextureRect = %GimmickIcon
@onready var gimmick_label: Label = %GimmickLabel
@onready var core_label: Label = %CoreLabel
@onready var hull_label: Label = %HullLabel



var __part: CraftDisplayPart = null

var color: Color :
	set(value): craft_part_display.color = value
	get: return craft_part_display.color



func set_part(part: CraftDisplayPart) -> void:

	__part = part

	var part_data = part.part_data

	if __part == null:
		clear()

	else:
		set_part_data(part_data)

		if part_data.gimmick != null:
			gimmick_icon.texture = Assets.get_gimmick_texture(part_data.gimmick)
			gimmick_label.text = part_data.gimmick.display_name

		else:
			gimmick_icon.texture = null
			gimmick_label.text = ""


func set_part_data(part_data: CraftPartData) -> void:

	__set_part_data(part_data)

	if part_data.gimmick != null:
		gimmick_icon.texture = Assets.get_gimmick_texture(part_data.gimmick)
		gimmick_label.text = part_data.gimmick.display_name

	else:
		gimmick_icon.texture = null
		gimmick_label.text = ""


func __set_part_data(part_data: CraftPartData) -> void:
	container.modulate.a = 1
	craft_part_display.set_part_data(part_data)
	name_label.text = part_data.definition.display_name
	name_label.modulate = Color(0, 1, 1) if part_data.shiny else Color.WHITE
	core_label.text = str(part_data.definition.core)
	hull_label.text = str(part_data.definition.hull)


func clear() -> void:
	container.modulate.a = 0



func _on_gimmick_options_item_selected(index: int) -> void:
	__part.part_data.gimmick = Assets.gimmicks.values()[index]
