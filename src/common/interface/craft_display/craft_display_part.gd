class_name CraftDisplayPart extends Control



@onready var sprite: TextureRect = %Sprite



var color: Color :
	set(value):
		color = value
		sprite.self_modulate = color

var flipped: bool :
	set(value):
		flipped = value
		sprite.flip_h = value

var angle: float :
	set(value):
		angle = value
		rotation = value

var part_data: CraftPartData



func set_blueprint(blueprint: CraftBlueprintPart) -> void:
	position = blueprint.place
	flipped = blueprint.flipped
	angle = blueprint.angle
	part_data = blueprint.data
	sprite.texture = Assets.get_part_texture(blueprint.data.definition.id)


func to_blueprint() -> CraftBlueprintPart:

	var blueprint = CraftBlueprintPart.new()

	blueprint.data = part_data
	blueprint.place = position
	blueprint.flipped = flipped
	blueprint.angle = angle

	return blueprint
