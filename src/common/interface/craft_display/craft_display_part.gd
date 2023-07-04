class_name CraftDisplayPart extends Control



@onready var craft_part_display: Control = %CraftPartDisplay



var part_data: CraftPartData = null

var color: Color :
	set(value): craft_part_display.color = value
	get: return craft_part_display.color

var flipped: bool :
	set(value): craft_part_display.flipped = value
	get: return craft_part_display.flipped

var angle: float :
	set(value): rotation = value
	get: return rotation



func set_blueprint(blueprint: CraftBlueprintPart) -> void:
	position = blueprint.place
	part_data = blueprint.data
	craft_part_display.definition = blueprint.data.definition
	craft_part_display.shiny = blueprint.data.shiny
	flipped = blueprint.flipped
	angle = blueprint.angle



func to_blueprint() -> CraftBlueprintPart:

	var blueprint = CraftBlueprintPart.new()

	blueprint.data = part_data
	blueprint.place = position
	blueprint.flipped = flipped
	blueprint.angle = angle

	return blueprint
