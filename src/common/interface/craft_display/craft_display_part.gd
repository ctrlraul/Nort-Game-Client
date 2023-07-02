extends Control



@onready var texture_rect: TextureRect = %TextureRect



var color: Color :
	set(value):
		color = value
		texture_rect.self_modulate = color



func set_blueprint(blueprint: CraftBlueprintPart) -> void:
	position = blueprint.place
	texture_rect.texture = Assets.get_part_texture(blueprint.data.definition.id)
