class_name CraftPart
extends Node2D



@export var ShinyPartMaterial: Material

@onready var sprite: Sprite2D = %Sprite2D

var collision_shape: CollisionShape2D = null
var gimmick: Node2D
var craft_body: CraftBody



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

	if Assets.is_core(blueprint):
		var core_light = Sprite2D.new()
		core_light.texture = Assets.CORE_LIGHT
		add_child(core_light)

	if blueprint.data.gimmick != null:
		gimmick = blueprint.data.gimmick.scene.instantiate()
		gimmick.position = position
		gimmick.part = self


func set_color(color: Color) -> void:
	sprite.self_modulate = color
