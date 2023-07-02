class_name CraftDisplay
extends Control



@export var PartScene: PackedScene

@onready var __parts_container: Control = %PartsContainer
@onready var __core_light: Control = %CoreLight



var core: Control = null
var color: Color = Color(1, 0, 1)

var parts: Array :
	get:
		return __parts_container.get_children()



func _ready() -> void:
	NodeUtils.clear(__parts_container)



func set_blueprint(blueprint: CraftBlueprint) -> void:

	for part_blueprint in blueprint.parts:
		add_part(part_blueprint)

	core = add_part(blueprint.core)

	__core_light.visible = true
	__core_light.position = core.position


func set_color(value: Color) -> void:
	color = value
	for part in __parts_container.get_children():
		part.color = color


func add_part(part_blueprint: CraftBlueprintPart) -> Control:

	var part = PartScene.instantiate()

	__parts_container.add_child(part)

	part.set_blueprint(part_blueprint)
	part.color = color

	# Keep core on top
	__parts_container.move_child(core, __parts_container.get_child_count() - 1)

	return part
