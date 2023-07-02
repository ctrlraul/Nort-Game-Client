extends Control



@onready var texture_rect: TextureRect = %TextureRect



func set_blueprint(blueprint: CraftBlueprintPart) -> void:
	position = blueprint.place
	texture_rect.texture = Assets.get_part_texture(blueprint.data.definition.id)


func set_color(color: Color) -> void:
	texture_rect.self_modulate = color
