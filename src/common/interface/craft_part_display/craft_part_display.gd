extends CenterContainer



@export var ShinyPartMaterial: ShaderMaterial

@onready var sprite: TextureRect = %Sprite
@onready var sprite_outline: TextureRect = %SpriteOutline



var __core_light: TextureRect = null
var __gimmick_sprite: TextureRect = null

var color: Color :
	set(value): sprite.self_modulate = value
	get: return sprite.self_modulate

var shiny: bool = false :
	set(value):
		sprite.material = ShinyPartMaterial if value else null
		shiny = value

var flipped: bool = false :
	set(value): sprite.flip_h = value
	get: return sprite.flip_h

var angle: bool = false :
	set(value):
		sprite.rotation = value
		sprite_outline.rotation = value
	get: return sprite.rotation

var definition: CraftPartDefinition = null :
	set(value):
		__set_definition(value)
		definition = value

var gimmick: Gimmick = null :
	set(value):
		__set_gimmick(value)
		gimmick = value


var outline: bool = false :
	set(value):
		sprite_outline.visible = value
		outline = value



func _ready() -> void:
	color = GameConfig.FACTIONLESS_COLOR



func set_part_data(part_data: CraftPartData) -> void:
	shiny = part_data.shiny
	gimmick = part_data.gimmick
	definition = part_data.definition
	flipped = false
	angle = 0



func __set_definition(value: CraftPartDefinition) -> void:

	sprite.texture = Assets.get_part_texture(value.id)
	sprite_outline.texture = sprite.texture

	if Assets.is_core(value):
		if __core_light == null:
			__core_light = TextureRect.new()
			__core_light.texture = Assets.CORE_LIGHT
			__core_light.position = __core_light.texture.get_size() * -0.5
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
			add_child(__gimmick_sprite)

		__gimmick_sprite.texture = Assets.get_gimmick_texture(value.id)
		__gimmick_sprite.position = __gimmick_sprite.texture.get_size() * -0.5

	elif __gimmick_sprite != null:
		__gimmick_sprite.queue_free()
		__gimmick_sprite = null
