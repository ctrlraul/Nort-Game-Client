class_name CraftBody extends Node2D

signal damaged()



@export var CraftPartScene: PackedScene



@onready var physics_body: CharacterBody2D = %PhysicsBody
@onready var parts_container: Node2D = %PartsContainer
@onready var flakes_container: Node2D = %FlakesContainer



var color: Color = GameConfig.FACTIONLESS_COLOR :
	set(value):
		color = value
		for part in parts_container.get_children():
			part.set_color(color)



func _ready() -> void:
	__clear()



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
	physics_body.add_child(part.collision_shape)


func __clear() -> void:
	NodeUtils.clear(physics_body)
	NodeUtils.clear(parts_container)
	NodeUtils.clear(flakes_container)
