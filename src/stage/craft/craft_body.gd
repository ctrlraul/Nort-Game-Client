class_name CraftBody extends Node2D

signal part_destroyed(part: CraftPart)



@export var CraftPartScene: PackedScene



@onready var parts_container: Node2D = %PartsContainer
@onready var gimmicks_container: Node2D = %GimmicksContainer



var craft: Craft
var core: CraftPart

var color: Color = GameConfig.FACTIONLESS_COLOR :
	set(value):
		color = value
		for part in parts_container.get_children():
			part.color = color



func _ready() -> void:
	NodeUtils.clear(parts_container)



func set_blueprint(blueprint: CraftBlueprint) -> void:
	for part_blueprint in blueprint.parts:
		__add_part(part_blueprint)
	core = __add_part(blueprint.core)


func get_part(index: int) -> CraftPart:
	return parts_container.get_child(index)


func get_parts() -> Array:
	return parts_container.get_children()



func __add_part(blueprint: CraftBlueprintPart) -> CraftPart:

	var part: CraftPart = CraftPartScene.instantiate()

	parts_container.add_child(part)

	part.color = color
	part.body = self
	part.set_blueprint(blueprint)
	part.destroyed.connect(func(): part_destroyed.emit(part))

	if part.gimmick:
		gimmicks_container.add_child(part.gimmick)

	return part
