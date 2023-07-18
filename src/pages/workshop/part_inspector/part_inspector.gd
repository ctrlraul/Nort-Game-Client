extends PanelContainer



@onready var container: HBoxContainer = %Container
@onready var craft_part_display: Control = %PartDisplay
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

	if __part == null:
		clear()

	else:

		var part_data = part.part_data

		set_part_data(part_data)

		if part_data.gimmick != null:
			gimmick_icon.texture = Assets.get_gimmick_texture(part_data.gimmick)
			gimmick_label.text = part_data.gimmick.display_name

		else:
			gimmick_icon.texture = null
			gimmick_label.text = ""



func set_part_data(part_data: PartData) -> void:

	__set_part_data(part_data)

	if part_data.gimmick != null:
		gimmick_icon.texture = Assets.get_gimmick_texture(part_data.gimmick)
		gimmick_label.text = part_data.gimmick.display_name

	else:
		gimmick_icon.texture = null
		gimmick_label.text = ""


func __set_part_data(part_data: PartData) -> void:

	container.modulate.a = 1

	craft_part_display.set_part_data(part_data)

	name_label.text = part_data.definition.display_name
	core_label.text = str(part_data.definition.core)
	hull_label.text = str(part_data.definition.hull)

	if part_data.shiny:
		name_label.text = "Shiny " + name_label.text
		name_label.modulate = Color(0, 1, 1)
	else:
		name_label.modulate = Color.WHITE


func clear() -> void:
	container.modulate.a = 0



func _on_gimmick_options_item_selected(index: int) -> void:
	__part.part_data.gimmick = Assets.gimmicks.values()[index]
