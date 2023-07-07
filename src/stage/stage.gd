extends Node2D



@export var CraftScene: PackedScene
@export var PlayerCraftScene: PackedScene
@export var TurretCraftScene: PackedScene

@onready var entities_container: Node2D = %EntitiesContainer
@onready var camera: Camera2D = $Camera2D



func _ready() -> void:
	pass # Replace with function body.



func spawn_player() -> PlayerCraft:
	var craft = PlayerCraftScene.instantiate()
	entities_container.add_child(craft)
	return craft


func spawn_turret() -> TurretCraft:
	var craft = TurretCraftScene.instantiate()
	entities_container.add_child(craft)
	return craft



func clear() -> void:
	camera.position = Vector2.ZERO
	camera.zoom = Vector2.ONE * 0.5
	NodeUtils.clear(entities_container)
