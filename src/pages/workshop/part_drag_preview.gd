extends Control



@onready var __sprite: TextureRect = %Sprite


func _ready() -> void:
	if !Game.initialized:
		await Game.initialize()
	__sprite.self_modulate = Assets.player_faction.color


var part_data: CraftPartData :
	set(value):
		if !is_inside_tree():
			await ready
		__sprite.texture = Assets.get_part_texture(value.definition.id)
