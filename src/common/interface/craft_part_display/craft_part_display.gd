extends Control



@export var ShinyPartMaterial: ShaderMaterial

@onready var sprite: TextureRect = %Sprite
@onready var sprite_outline: TextureRect = %SpriteOutline



var color: Color = GameConfig.FACTIONLESS_COLOR :
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
		sprite.texture = Assets.get_part_texture(value.id)
		sprite_outline.texture = sprite.texture
		definition = value

var outline: bool = false :
	set(value):
		sprite_outline.visible = value
		outline = value
