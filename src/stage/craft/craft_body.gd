class_name CraftBody extends Node2D

signal damaged()



@export var CraftPartScene: PackedScene



@onready var area: Area2D = %Area2D
@onready var parts_container: Node2D = %PartsContainer
@onready var gimmicks_container: Node2D = %GimmicksContainer



var craft: Craft

var color: Color = GameConfig.FACTIONLESS_COLOR :
	set(value):
		color = value
		for part in parts_container.get_children():
			part.set_color(color)



func _ready() -> void:
	__clear()



func enable() -> void:
	area.monitorable = true



func set_blueprint(blueprint: CraftBlueprint) -> void:
	__clear()
	for part_blueprint in blueprint.parts:
		__add_part(part_blueprint)
	__add_part(blueprint.core)



func __add_part(blueprint: CraftBlueprintPart) -> void:

	var part: CraftPart = CraftPartScene.instantiate()

	parts_container.add_child(part)

	part.set_blueprint(blueprint)
	part.set_color(color)
	part.craft_body = self

	area.add_child(part.collision_shape)

	if part.gimmick:
		gimmicks_container.add_child(part.gimmick)


func __clear() -> void:
	NodeUtils.clear(area)
	NodeUtils.clear(parts_container)
	NodeUtils.clear(gimmicks_container)
