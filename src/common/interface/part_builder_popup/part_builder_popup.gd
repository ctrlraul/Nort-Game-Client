class_name PartBuilderPopup extends GenericPopup

signal part_built(part_data: CraftPartData)



@onready var craft_part_display: Control = %CraftPartDisplay
@onready var part_options: OptionButton = %PartOptions

var part_data = CraftPartData.new()



func _ready() -> void:

	super()

	cancellable = true

	var parts = Assets.parts.values()

	part_options.clear()

	for part in parts:
		part_options.add_icon_item(Assets.get_part_texture(part), part.display_name)

	part_options.selected = 0
	part_data.definition = parts[0]
	craft_part_display.set_part_data(part_data)



func _on_gimmick_options_gimmick_selected(gimmick) -> void:
	part_data.gimmick = gimmick
	craft_part_display.gimmick = gimmick


func _on_build_button_pressed() -> void:
	part_built.emit(part_data)
	remove()


func _on_shiny_check_box_toggled(button_pressed: bool) -> void:
	part_data.shiny = button_pressed
	craft_part_display.shiny = button_pressed


func _on_part_options_item_selected(index: int) -> void:
	var part_definition: CraftPartDefinition = Assets.parts.values()[index]
	part_data.definition = part_definition
	craft_part_display.definition = part_definition
