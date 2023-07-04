class_name CraftPart
extends Node2D



@export var ShinyPartMaterial: Material

@onready var sprite: Sprite2D = %Sprite2D

var collision_shape: CollisionShape2D = null



func set_blueprint(blueprint: CraftBlueprintPart) -> void:

	if collision_shape != null:
		collision_shape.queue_free()

	position = blueprint.place

	sprite.texture = Assets.get_part_texture(blueprint)
	sprite.flip_h = blueprint.flipped
	sprite.rotation = blueprint.angle
	sprite.material = ShinyPartMaterial if blueprint.data.shiny else null

	collision_shape = CollisionShape2D.new()
	collision_shape.position = position
	collision_shape.shape = RectangleShape2D.new()
	collision_shape.shape.size = sprite.texture.get_size()


func set_color(color: Color) -> void:
	sprite.self_modulate = color
