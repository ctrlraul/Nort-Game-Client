class_name DroppedPart extends Node2D



@onready var __sprite: Sprite2D = %Sprite2D
@onready var __gimmick: Sprite2D = %Gimmick
@onready var __animation_player: AnimationPlayer = %AnimationPlayer



func _ready() -> void:
	__sprite.self_modulate = GameConfig.FACTIONLESS_COLOR
	__gimmick.global_rotation = 0
	__animation_player.play("float")


func set_setup(setup: OrphanPartSetup) -> void:

	position = setup.place
	rotation = setup.angle

	__sprite.texture = Assets.get_part_texture(setup.definition)
	__sprite.flip_h = setup.flipped
	__sprite.material = Assets.MATERIAL_SHINY if setup.shiny else null

	if setup.gimmick:
		__gimmick.texture = Assets.get_gimmick_texture(setup.gimmick.id)
	else:
		__gimmick.queue_free()
