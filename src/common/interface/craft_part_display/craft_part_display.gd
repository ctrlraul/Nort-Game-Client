extends Control



@export var ShinyPartMaterial: ShaderMaterial
@export var PartOutlineMaterial: ShaderMaterial

@onready var sprite: TextureRect = %Sprite

var __core_light: TextureRect = null
var __gimmick_sprite: TextureRect = null
var __outline: TextureRect = null

var color: Color :
	set(value):
		sprite.self_modulate = value
		color = value

var shiny: bool = false :
	set(value):
		sprite.material = ShinyPartMaterial if value else null
		shiny = value

var flipped: bool = false :
	set(value):
		sprite.flip_h = value
		flipped = value

var angle: float = false :
	set(value):
		rotation = value
		if __gimmick_sprite != null:
			__gimmick_sprite.rotation = -value
		angle = value

var definition: CraftPartDefinition = null :
	set(value):
		__set_definition(value)
		definition = value

var gimmick: Gimmick = null :
	set(value):
		__set_gimmick(value)
		gimmick = value


var outline: bool = false : set = __set_outline



func _ready() -> void:
	color = GameConfig.FACTIONLESS_COLOR



func set_part_data(part_data: CraftPartData) -> void:
	shiny = part_data.shiny
	gimmick = part_data.gimmick
	definition = part_data.definition
	flipped = false
	angle = 0



func __set_definition(value: CraftPartDefinition) -> void:

	var texture = Assets.get_part_texture(value.id)
	var texture_size = texture.get_size()

	sprite.texture = texture
	sprite.position = -texture_size * 0.5
	sprite.size = texture_size

	__set_core_light(Assets.is_core(value))
	__set_outline(outline)



func __set_core_light(value: bool) -> void:
	if value:
		if __core_light == null:
			__core_light = TextureRect.new()
			__core_light.texture = Assets.CORE_LIGHT
			__core_light.position = Assets.CORE_LIGHT.get_size() * -0.5
			__core_light.mouse_filter = Control.MOUSE_FILTER_IGNORE
			add_child(__core_light)

	elif __core_light != null:
		__core_light.queue_free()
		__core_light = null


func __set_gimmick(value: Gimmick) -> void:

	if value != null:

		if __gimmick_sprite == null:
			__gimmick_sprite = TextureRect.new()
			__gimmick_sprite.mouse_filter = Control.MOUSE_FILTER_IGNORE
			__gimmick_sprite.rotation = -angle
			__gimmick_sprite.z_index = 1
			add_child(__gimmick_sprite)

		var texture = Assets.get_gimmick_texture(value.id)
		var texture_size = texture.get_size()

		__gimmick_sprite.texture = texture
		__gimmick_sprite.position = texture_size * -0.5
		__gimmick_sprite.pivot_offset = texture_size * 0.5

	elif __gimmick_sprite != null:
		__gimmick_sprite.queue_free()
		__gimmick_sprite = null


func __set_outline(value: bool) -> void:

	if value:

		if __outline == null:
			__outline = TextureRect.new()
			__outline.mouse_filter = Control.MOUSE_FILTER_IGNORE
			__outline.rotation = angle
			__outline.material = PartOutlineMaterial
			add_child(__outline)

		__outline.texture = sprite.texture
		__outline.position = sprite.position
		__outline.size = sprite.size


	elif __outline != null:
		__outline.queue_free()
		__outline = null

	outline = value
