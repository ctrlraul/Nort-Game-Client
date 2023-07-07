extends PanelContainer



@onready var container: HBoxContainer = %Container
@onready var craft_part_display: Control = %CraftPartDisplay
@onready var name_label: Label = %NameLabel
@onready var gimmick_label_container: HBoxContainer = %GimmickLabelContainer
@onready var gimmick_icon: TextureRect = %GimmickIcon
@onready var gimmick_label: Label = %GimmickLabel
@onready var gimmick_options: OptionButton = %GimmickOptions
@onready var core_label: Label = %CoreLabel
@onready var hull_label: Label = %HullLabel



var __part: CraftDisplayPart = null

var color: Color :
	set(value): craft_part_display.color = value
	get: return craft_part_display.color



func _ready() -> void:

	await Game.initialize()

	if Game.dev:

		gimmick_options.clear()

		for gimmick in Assets.gimmicks.values():
			gimmick_options.add_icon_item(
				Assets.get_gimmick_texture(gimmick),
				gimmick.display_name
			)
			print(gimmick)

		gimmick_options.visible = true
		gimmick_label_container.queue_free()

	else:
		gimmick_options.queue_free()


func set_part(part: CraftDisplayPart) -> void:

	__part = part

	var part_data = part.part_data

	if __part == null:
		clear()

	else:
		container.modulate.a = 1
		craft_part_display.set_part_data(part_data)
		name_label.text = part_data.definition.display_name
		name_label.modulate = Color(0, 1, 1) if part_data.shiny else Color.WHITE
		core_label.text = str(part_data.definition.core)
		hull_label.text = str(part_data.definition.hull)

		if Game.dev:

			gimmick_options.select(Assets.gimmicks.values().find(part_data.gimmick))

		elif part_data.gimmick != null:
			gimmick_icon.texture = Assets.get_gimmick_texture(part_data.gimmick)
			gimmick_label.text = part_data.gimmick.display_name

		else:
			gimmick_icon.texture = null
			gimmick_label.text = ""


func clear() -> void:
	container.modulate.a = 0



func _on_gimmick_options_item_selected(index: int) -> void:
	__part.part_data.gimmick = Assets.gimmicks.values()[index]
